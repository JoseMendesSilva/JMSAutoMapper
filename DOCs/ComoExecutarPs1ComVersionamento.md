Executar de diferentes formas
powershell
# Forma 1: Com parâmetro nomeado
# .\GerarPacotesNupkg.ps1 -Versao "1.0.12"

# Forma 2: Com "versao" no início (seu exemplo)
# .\GerarPacotesNupkg.ps1 versao1.0.12

# Forma 3: Apenas o número
.\GerarPacotesNupkg.ps1 1.0.12

# Forma 4: Com versão beta
.\GerarPacotesNupkg.ps1 1.0.12-beta

# Forma 5: Especificando configuração
.\GerarPacotesNupkg.ps1 -Versao "1.0.12" -Configuracao "Debug"

# Forma 6: Especificando pasta de saída diferente
.\GerarPacotesNupkg.ps1 -Versao "1.0.12" -PastaSaida "./D:\NuGetLocal/DeployParaTeste"