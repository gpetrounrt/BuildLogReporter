
[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet("clean", "build", "unit-test", "coverage", "pack", "help")]
    [string] $action = "")

# Establish and enforce coding rules in expressions, scripts and script blocks.
Set-StrictMode -Version Latest
$errorActionPreference = "Stop"

Set-Variable SolutionPath -Option Constant -Value "$PSScriptRoot\BuildLogReporter.sln" -Force -ErrorAction SilentlyContinue
Set-Variable ArtifactsPath -Option Constant -Value "$PSScriptRoot\artifacts" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoveragePath -Option Constant -Value "$ArtifactsPath\Coverage" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageResultsPath -Option Constant -Value "$TestCoveragePath\Results" -Force -ErrorAction SilentlyContinue
Set-Variable TestSummaryPath -Option Constant -Value "$ArtifactsPath\TestSummary" -Force -ErrorAction SilentlyContinue
Set-Variable ToolsPath -Option Constant -Value "$PSScriptRoot\tools" -Force -ErrorAction SilentlyContinue

function DeleteDirectory([string] $directoryPath) {
    if (Test-Path $directoryPath) {
        Remove-Item $directoryPath -Force -Recurse
    }
}

function Clean() {
    & dotnet clean $SolutionPath --configuration Release

    DeleteDirectory $ArtifactsPath
}

function Build() {
    $binlogPath = "$ArtifactsPath\Built\Release\BuildLogReporter\Output.binlog"

    & dotnet build $SolutionPath --configuration Release -bl:$binlogPath
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
        -targetdir:"$TestCoveragePath\Report" `
        -reporttypes:"Badges;Html;MarkdownSummary" `
        -title:"Build Log Reporter"
}

function DisplayHelp() {
    Write-Host
    Write-Host "Usage: .\Make.ps1 [action]"
    Write-Host
    Write-Host "Available actions:"
    Write-Host "clean`t`tCleans the solution artifacts"
    Write-Host "build`t`tBuilds the solution"
    Write-Host "unit-test`tRuns the unit tests"
    Write-Host "coverage`tGenerates the coverage reports"
    Write-Host "pack`t`tGenerates the NuGet package"
    Write-Host "help`t`tDisplays this content"
    Write-Host
}

switch -wildcard ($action) {
    "clean" {
        Clean
        break
    }

    "build" {
        Build
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
