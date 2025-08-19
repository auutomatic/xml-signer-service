# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the source file
COPY SignXmlApi.cs .

# Restore and build the application
RUN dotnet new console -o . && \
    mv SignXmlApi.cs Program.cs && \
    dotnet add package Microsoft.AspNetCore.App && \
    dotnet publish -c Release -o out

# Use the .NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8085
ENV ASPNETCORE_URLS=http://+:8085
ENTRYPOINT ["dotnet", "SignXmlApi.dll"]