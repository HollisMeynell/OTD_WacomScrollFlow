param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$projectFile = Join-Path $repositoryRoot 'src/WacomScrollFlow/WacomScrollFlow.csproj'
[xml]$project = Get-Content -LiteralPath $projectFile -Raw
if ($Version -ne $project.Project.PropertyGroup.Version) {
    throw "Package version $Version does not match the project version."
}

$dll = Join-Path $repositoryRoot 'src/WacomScrollFlow/bin/Release/net8.0/WacomScrollFlow.dll'
if (!(Test-Path -LiteralPath $dll)) { throw 'Build the Release configuration before packaging.' }
$assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($dll).Version
if ($assemblyVersion -ne [version]"$Version.0") { throw 'The built DLL version does not match the package version. Rebuild first.' }

$outputDirectory = Join-Path $repositoryRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$archive = Join-Path $outputDirectory "WacomScrollFlow-$Version-win.zip"
$files = @($dll, (Join-Path $repositoryRoot 'README.md'), (Join-Path $repositoryRoot 'README-zh.md'))
Compress-Archive -LiteralPath $files -DestinationPath $archive -Force
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$archive.sha256", "$hash  $([IO.Path]::GetFileName($archive))`n", [Text.UTF8Encoding]::new($false))
Write-Output "Created $archive"
Write-Output "SHA256: $hash"
