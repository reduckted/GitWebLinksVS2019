Set-StrictMode -Version "Latest"
$ErrorActionPreference = "Stop"

$root = Split-Path -Path $PSCommandPath -Parent | Join-Path -ChildPath "..\.." | Resolve-Path

$msbuild = ""

foreach ($edition in @("Community", "Professional", "Enterprise")) {
    $exe = Join-Path -Path ${env:ProgramFiles(x86)} -ChildPath "Microsoft Visual Studio\2017\$edition\MSBuild\15.0\Bin\msbuild.exe"

    if (Test-Path $exe) {
        $msbuild = $exe
        break;
    }
}

if (-not $msbuild) {
    throw "Could not find MSBuild."
}

$solution = Join-Path -Path $root -ChildPath "GitWebLinks.sln"

& $msbuild $solution /t:Rebuild /p:Configuration=Release /v:m

if ($LASTEXITCODE -ne 0) {
    throw "Build failed."
}

$xunit = Join-Path -Path $root -ChildPath "packages\xunit.runner.console.2.4.1\tools\net452\xunit.console.exe"
$assembly = Join-Path -Path $root -ChildPath "tests\GitWebLinks.Tests\bin\Release\GitWebLinks.Tests.dll"

& $xunit $assembly

if ($LASTEXITCODE -ne 0) {
    throw "Tests failed."
}

$extension = Join-Path -Path $root -ChildPath "source\GitWebLinks\bin\Release\GitWebLinks.vsix"

explorer.exe "/select,`"$extension`""
