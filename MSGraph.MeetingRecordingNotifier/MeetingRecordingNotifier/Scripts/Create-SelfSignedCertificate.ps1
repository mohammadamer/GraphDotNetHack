#Requires -RunAsAdministrator
<#
.SYNOPSIS
Creates a Self Signed Certificate
.DESCRIPTION
.EXAMPLE
.\Create-SelfSignedCertificate.ps1
#>

param(
   [Parameter(Mandatory=$true)][System.String]$CertificateName
)

$cert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
   -Subject "CN=$($CertificateName)-Root" -KeyExportPolicy Exportable `
   -HashAlgorithm sha256 -KeyLength 2048 `
   -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsageProperty Sign -KeyUsage CertSign

New-SelfSignedCertificate -Type Custom -DnsName $($CertificateName) -KeySpec Signature `
   -Subject "CN=$($CertificateName)" -KeyExportPolicy Exportable `
   -HashAlgorithm sha256 -KeyLength 2048 `
   -CertStoreLocation "Cert:\CurrentUser\My" `
   -Signer $cert -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")