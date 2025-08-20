# Use the .NET runtime image for linux-x64
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy all files from the bin directory
COPY bin/ .

# Verify presence of SignXmlService.dll
RUN ls -la && test -f SignXmlService.dll || { echo "SignXmlService.dll not found"; exit 1; }

# Expose port 8085
EXPOSE 8085
ENV ASPNETCORE_URLS=http://+:8085

# Run the precompiled DLL
ENTRYPOINT ["dotnet", "SignXmlService.dll"]
