#Set variables
Push-Location -Path "."
$RepoRoot = Get-Location

Write-Host $RepoRoot

$Src = "$RepoRoot\src"
$Project = "$Src\Gaspra.Roulette.sln"
$Output = "$RepoRoot\.publish"
$Runtime = "win-x64"
$Configuration = "Release"
$Framework = "net5.0"

#Ensure paket is available
dotnet tool restore

#Push to temp folder
dotnet publish $Project -o $Output -r $Runtime -c $Configuration -f $Framework --self-contained

Pop-Location