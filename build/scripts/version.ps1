[CmdLetBinding()]
param (
    [switch] $Major,
    [switch] $Minor,
    [switch] $Patch
)

Set-StrictMode -Version "Latest"
$ErrorActionPreference = "Stop"

$root = Split-Path -Path $PSCommandPath -Parent | Join-Path -ChildPath "..\.." | Resolve-Path

# Find the current version.
$assemblyInfo = $root | Join-Path -ChildPath "source\GitWebLinks\My Project\AssemblyInfo.vb"
$content = Get-Content -Path $assemblyInfo -Raw

if (-not ($content -match "\<Assembly\: AssemblyVersion\(`"([\d\.]+)`"\)\>")) {
    throw "Could not determine the current version."
}

# Determine the new version.
$previous = $matches[1]
$version = New-Object System.Version -ArgumentList $previous

if ($Major) {
    $version = New-Object System.Version -ArgumentList ($version.Major + 1), 0, 0, 0

} elseif ($Minor) {
    $version = New-Object System.Version -ArgumentList $version.Major, ($version.Minor + 1), 0, 0

} elseif ($Patch) {
    $version = New-Object System.Version -ArgumentList $version.Major, $version.Minor, ($version.Build + 1), 0
}

$assemblyVersion = "$($version.Major).$($version.Minor).$($version.Build).$($version.Revision)"
$vsixVersion = "$($version.Major).$($version.Minor).$($version.Build)"

# Update the assembly info files.
Get-ChildItem -Path $root -Recurse -Filter "AssemblyInfo.vb" | ForEach-Object {
    $s = Get-Content -Path $_.FullName -Raw -Encoding UTF8
    $s = $s -creplace "Version\(`"([\d\.]+)`"\)", "Version(`"$assemblyVersion`")"
    [System.IO.File]::WriteAllText($_.FullName, $s, [System.Text.Encoding]::UTF8)
}

# Update the VSIX manifest files.
Get-ChildItem -Path $root -Recurse -Filter "*.vsixmanifest" | ForEach-Object {
    $doc = [xml](Get-Content -Path $_.FullName)

    if ($doc) {
        $ns = New-Object System.Xml.XmlNamespaceManager -ArgumentList $doc.NameTable
        $ns.AddNamespace("x", $doc.DocumentElement.NamespaceURI)

        $identity = $doc.SelectSingleNode("x:PackageManifest/x:Metadata/x:Identity", $ns)
        $identity.SetAttribute("Version", $vsixVersion)

        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.IndentChars = "    "
        $settings.NewLineChars = "`r`n"
        $settings.NewLineHandling = "Replace"

        $streamWriter = [System.IO.StreamWriter]::new($_.FullName, $false, [System.Text.Encoding]::UTF8)
        $xmlWriter = [System.Xml.XmlWriter]::Create($streamWriter, $settings)
        
        try {
            $doc.Save($xmlWriter)
            $streamWriter.WriteLine("")
            
        } finally {
            $xmlWriter.Close()
            $streamWriter.Close()
        }
    }
}

# Stage the changes and commit.
git add .
git commit -m "v$vsixVersion"
