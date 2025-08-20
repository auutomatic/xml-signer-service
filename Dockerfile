# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Create a minimal .csproj file with explicit AssemblyName and required packages
RUN echo '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType><AssemblyName>SignXmlApi</AssemblyName></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App" /><PackageReference Include="System.Security.Cryptography.Xml" Version="8.0.0" /><PackageReference Include="System.Security.Cryptography.Pkcs" Version="8.0.0" /></ItemGroup><ItemGroup><None Update="SignXmlApi.cs"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None></ItemGroup></Project>' > SignXmlApi.csproj

# Copy the source file with explicit case sensitivity
COPY ./SignXmlApi.cs .

# Verify file presence
RUN ls -la && test -f SignXmlApi.cs || { echo "SignXmlApi.cs not found"; exit 1; }

# Restore with fallback NuGet source and build with verbose output
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org && \
    dotnet restore SignXmlApi.csproj -v detailed || { echo "Restore failed"; exit 1; } && \
    dotnet publish SignXmlApi.csproj -c Release -o out --no-restore -v detailed || { echo "Publish failed"; exit 1; }

# Use the .NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8085
ENV ASPNETCORE_URLS=http://+:8085
ENTRYPOINT ["dotnet", "SignXmlApi.dll"]