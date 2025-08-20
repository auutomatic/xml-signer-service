# Use the .NET runtime image directly (no SDK needed)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the precompiled DLL and related files from the bin directory
COPY bin/ .

# Expose port 8085
EXPOSE 8085
ENV ASPNETCORE_URLS=http://+:8085

# Run the precompiled DLL
ENTRYPOINT ["dotnet", "SignXmlApi.dll"]