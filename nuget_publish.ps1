param(
    [string]$ApiKey,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$isDryRun = $true
if ($PSBoundParameters.ContainsKey("DryRun")) {
    $isDryRun = [bool]$DryRun
}

$csprojPath = Join-Path $PSScriptRoot "txt2obj\txt2obj.csproj"
$nugetVersionsUrl = "https://api.nuget.org/v3-flatcontainer/txt2obj/index.json"

Write-Host "Checking current version on NuGet..."
try {
    $versionsResponse = Invoke-WebRequest -Uri $nugetVersionsUrl -UseBasicParsing
    $versionsJson = $versionsResponse.Content | ConvertFrom-Json
    $versionCandidates = @()
    foreach ($version in $versionsJson.versions) {
        try {
            $versionCandidates += [version]$version
        } catch {
            # Skip prerelease or non-standard versions.
        }
    }
    if ($versionCandidates.Count -eq 0) {
        throw "No parseable versions found."
    }
    $nugetVersion = ($versionCandidates | Sort-Object -Descending | Select-Object -First 1).ToString()
} catch {
    Write-Warning "Could not detect current version from $nugetVersionsUrl. Skipping auto-bump check."
    $nugetVersion = $null
}

if ($nugetVersion) {
    $csprojContent = Get-Content $csprojPath -Raw
    $csprojVersionMatch = [regex]::Match($csprojContent, "<Version>(\d+\.\d+\.\d+(\.\d+)?)</Version>", "IgnoreCase")
    if (-not $csprojVersionMatch.Success) {
        Write-Warning "Could not detect <Version> in $csprojPath. Skipping auto-bump check."
    } else {
        $csprojVersion = $csprojVersionMatch.Groups[1].Value
        Write-Host "NuGet current version: $nugetVersion"
        Write-Host "Project version: $csprojVersion"
        $nugetVerObj = $null
        $projVerObj = $null
        try {
            $nugetVerObj = [version]$nugetVersion
            $projVerObj = [version]$csprojVersion
        } catch {
            Write-Warning "Version parsing failed. Skipping auto-bump check."
        }

        if ($nugetVerObj -and $projVerObj -and $nugetVerObj -ge $projVerObj) {
            $versionParts = $csprojVersion.Split(".")
            if ($versionParts.Length -lt 3) {
                $versionParts = @($versionParts + "0")
            }
            $major = [int]$versionParts[0]
            $minor = [int]$versionParts[1]
            $patch = [int]$versionParts[2]
            $suggestedVersion = [string]::Format("{0}.{1}.{2}", $major, $minor, ($patch + 1))
            if ([string]::IsNullOrWhiteSpace($suggestedVersion)) {
                Write-Warning "Suggested version could not be computed. Skipping auto-bump."
            } else {
                Write-Host ("Suggested version: {0}" -f $suggestedVersion)
                $prompt = "Bump version in {0} to {1}? (y/n)" -f $csprojPath, $suggestedVersion
                $answer = Read-Host $prompt
                if ($answer -match '^(y|yes)$') {
                    $updated = $csprojContent -replace "<Version>$([regex]::Escape($csprojVersion))</Version>", "<Version>$suggestedVersion</Version>"
                    Set-Content -Path $csprojPath -Value $updated
                    Write-Host "Updated version to $suggestedVersion."
                } else {
                    Write-Host "Keeping version $csprojVersion."
                }
            }
        }
    }
}

$projectPath = Join-Path $PSScriptRoot "txt2obj\txt2obj.csproj"
$projectDir = Split-Path -Parent $projectPath
$packRoot = Join-Path $PSScriptRoot "nuget_packaged"
$outputDir = Join-Path $packRoot "nuget"
$intermediateDir = Join-Path $packRoot "obj"

Write-Host "Packing $projectPath..."
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}
if (-not (Test-Path $intermediateDir)) {
    New-Item -ItemType Directory -Path $intermediateDir | Out-Null
}
Write-Host "Building $projectPath..."
$intermediateWithSlash = $intermediateDir
if (-not $intermediateWithSlash.EndsWith("\") -and -not $intermediateWithSlash.EndsWith("/")) {
    $intermediateWithSlash = "$intermediateWithSlash\"
}
Write-Host "Cleaning $projectPath (Debug/Release)..."
dotnet clean $projectPath -c Debug
dotnet clean $projectPath -c Release
dotnet build $projectPath -c Release /p:BaseIntermediateOutputPath=$intermediateWithSlash
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}
dotnet pack $projectPath -c Release -o $outputDir /p:BaseIntermediateOutputPath=$intermediateWithSlash
if ($LASTEXITCODE -ne 0) {
    throw "dotnet pack failed with exit code $LASTEXITCODE."
}

$package = Get-ChildItem -Path $outputDir -Filter "txt2obj.*.nupkg" |
    Where-Object { $_.Name -notlike "*.snupkg" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $package) {
    $fallbackDirs = @(
        (Join-Path $PSScriptRoot "txt2obj\bin\Release"),
        (Join-Path $PSScriptRoot "txt2obj\obj\Release")
    )
    foreach ($dir in $fallbackDirs) {
        if (Test-Path $dir) {
            $package = Get-ChildItem -Path $dir -Filter "txt2obj.*.nupkg" -Recurse |
                Where-Object { $_.Name -notlike "*.snupkg" } |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1
            if ($package) { break }
        }
    }
}

if (-not $package) {
    Write-Host "No .nupkg found. Contents of ${outputDir}:"
    Get-ChildItem -Path $outputDir | ForEach-Object { Write-Host " - $($_.Name)" }
    throw "No .nupkg found in $outputDir. Ensure the build succeeded."
}

Write-Host "Pushing $($package.FullName)..."
if ($isDryRun) {
    Write-Host "Dry run enabled. Skipping push."
    Write-Host "Would run: dotnet nuget push `"$($package.FullName)`" -k <API_KEY> -s https://api.nuget.org/v3/index.json"
} else {
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        throw "ApiKey is required when DryRun is disabled."
    }
    dotnet nuget push $package.FullName -k $ApiKey -s https://api.nuget.org/v3/index.json
}
