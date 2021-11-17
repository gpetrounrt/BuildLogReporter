
[CmdletBinding()]
Param(
    [Parameter(Position = 1)]
    [ValidateSet("clean", "build", "unit-test", "help")]
    [string] $action = "")

# Establish and enforce coding rules in expressions, scripts and script blocks.
Set-StrictMode -Version Latest
$errorActionPreference = "Stop"

Set-Variable SolutionPath -Option Constant -Value "$PSScriptRoot\BuildLogReporter.sln" -Force -ErrorAction SilentlyContinue

function DeleteDirectories([string] $directoryName) {
    $directories = Get-ChildItem $PSScriptRoot $directoryName -Recurse -Directory
    foreach ($directory in $directories) {
        $directoryPath = $directory.FullName
        Remove-Item $directoryPath -Force -Recurse
    }
}

function Clean() {
    & dotnet clean $SolutionPath --configuration Release

    DeleteDirectories "bin"
    DeleteDirectories "obj"
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
        & dotnet test $SolutionPath --configuration Release --no-build
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
