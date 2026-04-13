param([string]$Versao)
$VersaoLimpa = $Versao -replace 'versao', ''
dotnet pack -c Release -p:PackageVersion=$VersaoLimpa -o ./nupkgs
Write-Host "Pacote gerado com versão $VersaoLimpa"