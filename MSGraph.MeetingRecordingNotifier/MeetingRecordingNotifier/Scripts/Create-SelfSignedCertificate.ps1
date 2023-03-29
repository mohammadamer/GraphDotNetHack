#Requires -RunAsAdministrator
<#
.SYNOPSIS
Creates a Certificate
.DESCRIPTION
.EXAMPLE
.\Create-SelfSignedCertificate.ps1
#>

$password = "<YourPasswordForTheCertificate>"
$cert = New-SelfSignedCertificate -DnsName "<Name>" -CertStoreLocation "cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(1) -KeyAlgorithm RSA -KeyLength 2048
$cert | Export-PfxCertificate -FilePath ".\Certs\<Name>.pfx"  -Password $(ConvertTo-SecureString -String $password -AsPlainText -Force)
$cert | Export-Certificate -FilePath ".\Certs\<Name>.cer"