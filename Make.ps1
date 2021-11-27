
[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet("clean", "build", "unit-test", "integration-test", "coverage", "pack", "help")]
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

function Test([string] $testType) {
    & dotnet test --configuration Release `
        --no-build `
        --filter FullyQualifiedName~$testType `
        --collect:"XPlat Code Coverage" `
        --logger "junit;LogFilePath=$TestSummaryPath/{assembly}.xml" `
        --results-directory "$TestCoverageResultsPath"
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

    $actions = @(
        @{ Name = "clean"; Description="Cleans the solution artifacts" },
        @{ Name = "build"; Description="Builds the solution" },
        @{ Name = "unit-test"; Description="Runs the unit tests" },
        @{ Name = "integration-test"; Description="Runs the integration tests" },
        @{ Name = "coverage"; Description="Generates the coverage reports" },
        @{ Name = "pack"; Description="Generates the NuGet package" }
        @{ Name = "help"; Description="Displays this content" })

    Write-Host
    Write-Host "Usage: .\Make.ps1 [action]"
    Write-Host
    Write-Host "Available actions:"
    $actions | % { [PSCustomObject]$_ } | Format-Table -HideTableHeaders
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
        Test "UnitTests"
        break
    }

    "integration-test" {
        Test "IntegrationTests"
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
