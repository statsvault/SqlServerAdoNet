SonarScanner.MSBuild.exe begin /k:"statsvault_SqlServerAdoNet" /d:sonar.organization="statsvault-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="35830c522e46faaaf7feebd098f0df3b207d18b8"
MsBuild.exe /t:Rebuild
SonarScanner.MSBuild.exe end /d:sonar.login="35830c522e46faaaf7feebd098f0df3b207d18b8"
exit
