
[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet(
        "clean",
        "build",
        "unit-test",
        "integration-test",
        "coverage",
        "pack",
        "install",
        "report",
        "all",
        "upload-badges",
        "help")]
    [string] $action = "")

# Establish and enforce coding rules in expressions, scripts and script blocks.
Set-StrictMode -Version Latest
$errorActionPreference = "Stop"

$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1

Set-Variable SolutionPath -Option Constant -Value "$PSScriptRoot\BuildLogReporter.sln" -Force -ErrorAction SilentlyContinue
Set-Variable ArtifactsPath -Option Constant -Value "$PSScriptRoot\artifacts" -Force -ErrorAction SilentlyContinue
Set-Variable ReportPath -Option Constant -Value "$ArtifactsPath\Report" -Force -ErrorAction SilentlyContinue
Set-Variable BinlogPath -Option Constant -Value "$ArtifactsPath\Built\Release\BuildLogReporter\Output.binlog" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoveragePath -Option Constant -Value "$ArtifactsPath\Coverage" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageReportPath -Option Constant -Value "$TestCoveragePath\Report" -Force -ErrorAction SilentlyContinue
Set-Variable TestCoverageResultsPath -Option Constant -Value "$TestCoveragePath\Results" -Force -ErrorAction SilentlyContinue
Set-Variable TestSummaryPath -Option Constant -Value "$ArtifactsPath\TestSummary" -Force -ErrorAction SilentlyContinue
Set-Variable ToolsPath -Option Constant -Value "$PSScriptRoot\tools" -Force -ErrorAction SilentlyContinue
Set-Variable BadgesGistPath -Option Constant -Value "$PSScriptRoot\repos\BadgesGist" -Force -ErrorAction SilentlyContinue

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
    & dotnet build $SolutionPath --configuration Release -bl:$BinlogPath
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
        -targetdir:"$TestCoverageReportPath" `
        -reporttypes:"Badges;Html;MarkdownSummary" `
        -title:"Build Log Reporter"
}

function Pack() {
    & dotnet pack $SolutionPath --configuration Release --no-build
}

function Install() {
    $isBuildLogReporterInstalled = $false
    if (Test-Path $ToolsPath) {
        $tools = dotnet tool list --tool-path $ToolsPath
        $isBuildLogReporterInstalled = $tools -Like '*build-log-reporter*'
    }

    if ($isBuildLogReporterInstalled) {
        & dotnet tool uninstall --tool-path $ToolsPath BuildLogReporter
    }

    & dotnet tool install --add-source "$ArtifactsPath\Package" `
        --tool-path $ToolsPath `
        --prerelease `
        BuildLogReporter
}

function Report() {
    & "$ToolsPath\build-log-reporter.exe" "--report-types" "Badge;Html;Json;Markdown;Xml" $BinlogPath $ReportPath

    $report = Get-Content "$ReportPath\BuildLogReport.json" | ConvertFrom-Json
    if ($report.error_count -gt 0 -or $report.warning_count -gt 0) {

        # TODO: Add GitHub PR comment.
    } else {
        Write-Host "The build completed without errors or warnings."
    }
}

function UploadBadges() {
    $branchName = git rev-parse --abbrev-ref HEAD
    if ($branchName -ne "main") {
        Write-Host "Badges are only uploaded when building the main branch."
        return
    }

    if ($null -eq $env:GITHUB_GIST_TOKEN) {
        Write-Host "GITHUB_GIST_TOKEN environment variable is not defined."
        return
    }

    $gistUrl = "https://" + $env:GITHUB_GIST_TOKEN + "@gist.github.com/12e53399727fc04da47e22494e6e2681.git"

    if (!(Test-Path "$BadgesGistPath")) {
        & git clone $gistUrl repos/BadgesGist
    }

    Copy-Item "$TestCoverageReportPath\badge_combined.svg" "$BadgesGistPath\CoverageBadge.svg"
    Copy-Item "$ReportPath\BuildLogReport.svg" "$BadgesGistPath\BuildLogBadge.svg"

    & git -C "$BadgesGistPath" add .
    & git -C "$BadgesGistPath" commit --amend --no-edit
    & git -C "$BadgesGistPath" push $gistUrl master -f
}

function DisplayHelp() {

    $availableActions = @(
        @{ Name = "clean"; Description = "Cleans the solution artifacts" },
        @{ Name = "build"; Description = "Builds the solution" },
        @{ Name = "unit-test"; Description = "Runs the unit tests" },
        @{ Name = "integration-test"; Description = "Runs the integration tests" },
        @{ Name = "coverage"; Description = "Generates the coverage reports" },
        @{ Name = "pack"; Description = "Generates the NuGet package" },
        @{ Name = "install"; Description = "Installs the NuGet package" },
        @{ Name = "report"; Description = "Generates the build log reports" },
        @{ Name = "all"; Description = "Runs all the above actions" },
        @{ Name = "upload-badges"; Description = "Uploads badges to Gist" },
        @{ Name = "help"; Description = "Displays this content" })

    Write-Host
    Write-Host "Usage: .\Make.ps1 [action]"
    Write-Host
    Write-Host "Available actions:"
    $availableActions | % { [PSCustomObject]$_ } | Format-Table -HideTableHeaders
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
        Pack
        break
    }

    "install" {
        Install
        break
    }

    "report" {
        Report
        break
    }

    "all" {
        Clean
        Build
        Test "UnitTests"
        Test "IntegrationTests"
        CreateCoverageReport
        Pack
        Install
        Report
        break
    }

    "upload-badges" {
        UploadBadges
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
