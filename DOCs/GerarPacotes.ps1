#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Script para gerar pacotes NuGet do JMSAutoMapper
.DESCRIPTION
    Gera pacotes .nupkg do JMSAutoMapper com a versão especificada
.PARAMETER Versao
    Versão do pacote (ex: versao1.0.1, 1.0.1, 2.0.0-beta, etc)
.PARAMETER Configuracao
    Configuração do build (Release ou Debug) - padrão: Release
.PARAMETER PastaSaida
    Pasta onde os pacotes serão salvos - padrão: ./nupkgs
.EXAMPLE
    .\GerarPacotes.ps1 -Versao "versao1.0.1"
    .\GerarPacotes.ps1 -Versao "1.0.1" -Configuracao "Release"
    .\GerarPacotes.ps1 versao1.0.1
    .\GerarPacotes.ps1 2.0.0-beta
#>

param(
    [Parameter(Mandatory=$true, Position=0)]
    [string]$Versao,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Release", "Debug")]
    [string]$Configuracao = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$PastaSaida = "./nupkgs"
)

# Configurações de codificação
[Console]::OutputEncoding = [System.Text.UTF8Encoding]::new()
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

# Cores para output
$successColor = "Green"
$errorColor = "Red"
$warningColor = "Yellow"
$infoColor = "Cyan"

function Write-Success { Write-Host "✅ $($args[0])" -ForegroundColor $successColor }
function Write-Error { Write-Host "❌ $($args[0])" -ForegroundColor $errorColor }
function Write-Warning { Write-Host "⚠️ $($args[0])" -ForegroundColor $warningColor }
function Write-Info { Write-Host "ℹ️ $($args[0])" -ForegroundColor $infoColor }
function Write-Step { Write-Host "`n📌 $($args[0])" -ForegroundColor $infoColor }

# Limpar o formato da versão (remover "versao" ou "versão" se presente)
$VersaoLimpa = $Versao -replace '^vers[aã]o', '' -replace '^versão', '' -replace '^versao', ''
$VersaoLimpa = $VersaoLimpa.Trim()

# Validar formato da versão
if ($VersaoLimpa -notmatch '^\d+\.\d+\.\d+') {
    Write-Warning "Formato de versão não reconhecido. Use algo como: 1.0.1, 2.0.0-beta, versao1.0.1"
}

Write-Host "`n==================================================" -ForegroundColor $infoColor
Write-Host "🚀 JMSAutoMapper - Gerador de Pacotes NuGet" -ForegroundColor $infoColor
Write-Host "==================================================" -ForegroundColor $infoColor
Write-Host ""

Write-Info "Versão: $VersaoLimpa"
Write-Info "Configuração: $Configuracao"
Write-Info "Pasta de saída: $PastaSaida"
Write-Host ""

# Verificar se dotnet está instalado
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK $dotnetVersion encontrado"
} catch {
    Write-Error ".NET SDK não encontrado. Instale o .NET SDK primeiro."
    exit 1
}

# Verificar se estamos no diretório correto (deve conter o arquivo .csproj)
$csprojFiles = Get-ChildItem -Path . -Filter "*.csproj" -File
if ($csprojFiles.Count -eq 0) {
    Write-Error "Nenhum arquivo .csproj encontrado no diretório atual"
    Write-Info "Certifique-se de executar o script no diretório do projeto JMSAutoMapper"
    exit 1
}

$projectFile = $csprojFiles[0].FullName
Write-Success "Projeto encontrado: $(Split-Path $projectFile -Leaf)"

# Criar pasta de saída se não existir
if (!(Test-Path $PastaSaida)) {
    New-Item -ItemType Directory -Path $PastaSaida -Force | Out-Null
    Write-Success "Pasta $PastaSaida criada"
}

Write-Step "Limpando builds anteriores..."
dotnet clean -c $Configuracao -v q 2>$null
if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq 1) {
    Write-Success "Limpeza concluída"
} else {
    Write-Warning "Problema na limpeza, mas continuando..."
}

Write-Step "Restaurando pacotes NuGet..."
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Erro ao restaurar pacotes"
    exit 1
}
Write-Success "Pacotes restaurados"

Write-Step "Compilando projeto em mode $Configuracao..."
dotnet build -c $Configuracao --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Erro na compilação"
    exit 1
}
Write-Success "Compilação concluída"

Write-Step "Gerando pacote NuGet versão $VersaoLimpa..."
dotnet pack -c $Configuracao --no-build -p:PackageVersion=$VersaoLimpa -o $PastaSaida

if ($LASTEXITCODE -eq 0) {
    Write-Success "Pacote gerado com sucesso!"
    
    # Listar os pacotes gerados
    $pacotes = Get-ChildItem -Path $PastaSaida -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending
    Write-Host "`n📦 Pacotes gerados:" -ForegroundColor $infoColor
    foreach ($pacote in $pacotes) {
        $tamanho = [math]::Round($pacote.Length / 1KB, 2)
        Write-Host "   - $($pacote.Name) ($tamanho KB)" -ForegroundColor $warningColor
    }
    
    # Mostrar o caminho completo
    $caminhoAbsoluto = Resolve-Path $PastaSaida
    Write-Host "`n📁 Localização: $caminhoAbsoluto" -ForegroundColor $infoColor
    
    # Comando para publicar (opcional)
    Write-Host "`n📤 Para publicar no NuGet.org use:" -ForegroundColor $infoColor
    Write-Host "   dotnet nuget push $PastaSaida\$($pacotes[0].Name) --api-key SUA-API-KEY --source https://api.nuget.org/v3/index.json" -ForegroundColor $warningColor
} else {
    Write-Error "Erro ao gerar pacote"
    exit 1
}

Write-Host "`n==================================================" -ForegroundColor $infoColor
Write-Host "✨ Processo concluído com sucesso!" -ForegroundColor $infoColor
Write-Host "==================================================" -ForegroundColor $infoColor