Set-StrictMode -Version "Latest"
$ErrorActionPreference = "Stop"

$root = Split-Path -Path $PSCommandPath -Parent | Join-Path -ChildPath "..\.." | Resolve-Path

$vsPath = ""

foreach ($edition in @("Community", "Professional", "Enterprise")) {
    $path = Join-Path -Path ${env:ProgramFiles} -ChildPath "Microsoft Visual Studio\2022\$edition"

    if (Test-Path $path) {
        $vsPath = $path
        break;
    }
}

if (-not $vsPath) {
    throw "Could not find Visual Studio installation."
}

$msbuild = Join-Path -Path $vsPath -ChildPath "MSBuild\Current\Bin\msbuild.exe"

if (-not (Test-Path $msbuild)) {
    throw "Could not find MSBuild."
}

$solution = Join-Path -Path $root -ChildPath "GitWebLinks.sln"

& $msbuild $solution /t:Rebuild /p:Configuration=Release /v:m

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

$vstest = Join-Path -Path $vsPath -ChildPath "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"

if (-not (Test-Path $vstest)) {
    throw "Could not find VSTest"
}

$assembly = Join-Path -Path $root -ChildPath "tests\GitWebLinks.Tests\bin\Release\GitWebLinks.Tests.dll"

& $vstest $assembly

if ($LASTEXITCODE -ne 0) {
    throw "Tests failed."
}

$extension = Join-Path -Path $root -ChildPath "source\GitWebLinks\bin\Release\GitWebLinks.vsix"

explorer.exe "/select,`"$extension`""

$extension2022 = Join-Path -Path $root -ChildPath "source\GitWebLinks2022\bin\Release\GitWebLinks2022.vsix"

explorer.exe "/select,`"$extension2022`""
