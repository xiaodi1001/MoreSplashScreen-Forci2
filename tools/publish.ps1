$ToolsDir = $PSScriptRoot
$ProjectDir = Split-Path $ToolsDir -Parent
Set-Location $ProjectDir
dotnet publish -c Release -p:CreateCipx=true -p:CipxPackageOutputDirectory=../out
Set-Location ..