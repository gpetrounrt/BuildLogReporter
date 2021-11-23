
[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet("clean", "build", "unit-test", "coverage", "pack", "help")]
    [string] $action = "")

# Establish and enforce coding rules in expressions, scripts and script blocks.
Set-StrictMode -Version Latest
$errorActionPreference = "Stop"

Set-Variable SolutionPath -Option Constant -Value "$PSScriptRoot\BuildLogReporter.sln" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoveragePath -Option Constant -Value "$PSScriptRoot\tests\coverage" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageResultsPath -Option Constant -Value "$TestCoveragePath\results" -Force -ErrorAction SilentlyContinue
Set-Variable TestSummaryPath -Option Constant -Value "$PSScriptRoot\tests\summary" -Force -ErrorAction SilentlyContinue
Set-Variable ToolsPath -Option Constant -Value "$PSScriptRoot\tools" -Force -ErrorAction SilentlyContinue

function DeleteDirectories([string] $directoryName) {
    $directories = Get-ChildItem $PSScriptRoot $directoryName -Recurse -Directory
    foreach ($directory in $directories) {
        $directoryPath = $directory.FullName
        Remove-Item $directoryPath -Force -Recurse
    }
}

function DeleteDirectory([string] $directoryPath) {
    if (Test-Path $directoryPath) {
        Remove-Item $directoryPath -Force -Recurse
    }
}

function Clean() {
    & dotnet clean $SolutionPath --configuration Release

    DeleteDirectories "bin"
    DeleteDirectories "obj"

    DeleteDirectory $TestSummaryPath
    DeleteDirectory $TestCoveragePath
}

function CreateCoverageReport() {
    $isReportGeneratorInstalled = $false
    if (Test-Path $ToolsPath) {
        $tools = dotnet tool list --tool-path $ToolsPath
        $isReportGeneratorInstalled = $tools -Like '*dotnet-reportgenerator-globaltool*'
    }

    if (!$isReportGeneratorInstalled) {
        dotnet tool install --tool-path $ToolsPath dotnet-reportgenerator-globaltool
    }

    & "$ToolsPath\reportgenerator.exe" `
        -reports:"$TestCoverageResultsPath\**\*.xml" `
        -targetdir:"$TestCoveragePath\report" `
        -reporttypes:"Badges;Html;MarkdownSummary" `
        -title:"Build Log Reporter"
}

function DisplayHelp() {
    Write-Host
    Write-Host "Usage: .\Make.ps1 [action]"
    Write-Host
    Write-Host "Available actions:"
    Write-Host "build`tbuilds the solution"
    Write-Host "clean`tcleans the solution output"
    Write-Host "help`tdisplays this content"
    Write-Host
}

switch -wildcard ($action) {
    "clean" {
        Clean
        break
    }

    "build" {
        & dotnet build $SolutionPath --configuration Release
        break
    }

    "unit-test" {
        & dotnet test --configuration Release `
            --no-build `
            --collect:"XPlat Code Coverage" `
            --logger "junit;LogFilePath=$TestSummaryPath/{assembly}.xml" `
            --results-directory "$TestCoverageResultsPath"
        break
    }

    "coverage" {
        CreateCoverageReport
        break
    }

    "pack" {
        & dotnet pack $SolutionPath --configuration Release --no-build
        break
    }

    "help" {
        DisplayHelp
        break
    }

    default {
        Write-Error "Could not find action '$action'"
        exit -1
    }
}
