$password = "testpassword123!"
$certPath = "./cert/dev-cert.pfx"

# Create certificate directory if it doesn't exist
New-Item -ItemType Directory -Force -Path "./cert"

# Remove existing certificate if it exists
if (Test-Path $certPath) {
    Remove-Item $certPath
}

# Trust the development certificate
dotnet dev-certs https --clean
dotnet dev-certs https -ep $certPath -p $password
dotnet dev-certs https --trust

Write-Host "Development certificate generated and trusted."