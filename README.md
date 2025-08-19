# XML Signing Web Service

A .NET Core Web API to sign XML documents using a P12 certificate, running in Docker on port 8085.

## Prerequisites
- .NET 8 SDK (for local testing)
- Docker
- Portainer (for deployment)
- Visual Studio (optional, for local testing)

## Local Testing in Visual Studio
1. Open the solution:
   - Create a new .NET Core Console Application in Visual Studio.
   - Replace the generated `Program.cs` with `SignXmlApi.cs` from this repository.
   - Add the NuGet package `Microsoft.AspNetCore.App` (no specific version needed; it pulls the framework reference).
2. Run the project:
   - Press F5 or run `dotnet run` in the project directory.
   - The API will start on `http://localhost:8085`.
3. Test the endpoint:
   - Use a tool like Postman or curl to send a POST request to `http://localhost:8085/signxml` with a JSON body:
     ```json
     {
         "XmlContent": "<your_xml_string>",
         "P12Base64": "<base64_encoded_p12_certificate>",
         "P12Password": "<certificate_password>"
     }
     ```
   - Example using curl:
     ```bash
     curl -X POST http://localhost:8085/signxml -H "Content-Type: application/json" -d '{"XmlContent":"<root>Test</root>","P12Base64":"<base64_string>","P12Password":"password"}'
     ```

## Deploying with Portainer
1. Create a GitHub repository and push all files (`SignXmlApi.cs`, `Dockerfile`, `.dockerignore`).
2. In Portainer:
   - Go to **Stacks** > **Add stack**.
   - Choose **Git Repository** as the build method.
   - Enter the repository URL and set the **Compose file path** to `Dockerfile` (Portainer will treat it as a single Dockerfile stack).
   - Set the **Port mapping** to `8085:8085` (host:container).
   - Deploy the stack.
3. Access the service at `http://<your-server-ip>:8085/signxml`.

## API Endpoint
- **POST /signxml**
  - **Request Body**:
    ```json
    {
        "XmlContent": "string (XML content to sign)",
        "P12Base64": "string (base64-encoded P12 certificate)",
        "P12Password": "string (certificate password)"
    }
    ```
  - **Response**:
    - Success: Signed XML as a string (200 OK)
    - Error: Error message (400 Bad Request or 500 Internal Server Error)

## Notes
- Ensure the P12 certificate is RSA-based and includes a private key.
- Adjust CORS settings in `SignXmlApi.cs` for production to restrict origins.
- The service runs on port 8085 to avoid conflicts with common ports.