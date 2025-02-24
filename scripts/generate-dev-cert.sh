#!/bin/bash

# Wait for certificate directory to be available
while [ ! -d "/app/cert" ]; do
    echo "Waiting for certificate directory..."
    sleep 1
done

# Check if certificate already exists
if [ ! -f "${ASPNETCORE_Kestrel__Certificates__Default__Path}" ]; then
    echo "Generating new development certificate..."
    # Generate development certificate
    dotnet dev-certs https -ep ${ASPNETCORE_Kestrel__Certificates__Default__Path} -p ${ASPNETCORE_Kestrel__Certificates__Default__Password}
    
    # Set appropriate permissions
    chmod 644 ${ASPNETCORE_Kestrel__Certificates__Default__Path}
else
    echo "Development certificate already exists."
fi

# Verify certificate exists and has correct permissions
if [ -f "${ASPNETCORE_Kestrel__Certificates__Default__Path}" ]; then
    echo "Certificate is ready at ${ASPNETCORE_Kestrel__Certificates__Default__Path}"
else
    echo "Certificate generation failed!"
    exit 1
fi