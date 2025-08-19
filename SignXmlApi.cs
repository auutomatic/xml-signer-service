using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; // Added for Results
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace XmlSigningService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add CORS to allow requests from any origin (adjust for production if needed)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });

            // Configure Kestrel to listen on port 8085
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(8085);
            });

            var app = builder.Build();

            // Enable CORS
            app.UseCors("AllowAll");

            app.MapPost("/signxml", async ([FromBody] SignXmlRequest request) =>
            {
                try
                {
                    // Validate input
                    if (string.IsNullOrEmpty(request.XmlContent) || string.IsNullOrEmpty(request.P12Base64) || string.IsNullOrEmpty(request.P12Password))
                    {
                        return Results.BadRequest("XML content, P12 certificate, and password are required.");
                    }

                    // Load XML
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(request.XmlContent);

                    // Decode P12 certificate
                    byte[] certBytes = Convert.FromBase64String(request.P12Base64);
                    var cert = new X509Certificate2(certBytes, request.P12Password, X509KeyStorageFlags.MachineKeySet);

                    // Sign the XML
                    var signedXml = SignXmlWithP12(xmlDoc, cert);

                    // Return the signed XML with content type application/xml
                    return Results.Content(signedXml.OuterXml, "application/xml");
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error signing XML: {ex.Message}");
                }
            });

            app.Run();
        }

        private static XmlDocument SignXmlWithP12(XmlDocument xmlDoc, X509Certificate2 cert)
        {
            // Ensure the certificate has a private key
            if (!cert.HasPrivateKey)
                throw new InvalidOperationException("Certificate does not contain a private key.");

            // Use RSACryptoServiceProvider or similar for compatibility with SignedXml
            RSA privateKey;
            try
            {
                privateKey = cert.GetRSAPrivateKey(); // Attempts to get RSA key, works with CNG
                if (privateKey == null)
                    throw new InvalidOperationException("Certificate does not contain an RSA private key.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve RSA private key. Ensure the certificate uses RSA.", ex);
            }

            // Create a SignedXml object
            SignedXml xmlFirmado = new SignedXml(xmlDoc);

            // Attach the private key to the SignedXml document
            xmlFirmado.SigningKey = privateKey;

            // Create a reference to the document to be signed
            Reference reference = new Reference();
            reference.Uri = ""; // Entire document

            // Add an enveloped signature transform
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object
            xmlFirmado.AddReference(reference);

            // Create a KeyInfo object
            KeyInfo keyInfo = new KeyInfo();

            // Add certificate data to KeyInfo
            KeyInfoX509Data clause = new KeyInfoX509Data();
            clause.AddSubjectName(cert.Subject);
            clause.AddCertificate(cert);
            keyInfo.AddClause(clause);

            // Assign the KeyInfo to the SignedXml object
            xmlFirmado.KeyInfo = keyInfo;

            // Compute the signature
            xmlFirmado.ComputeSignature();

            // Get the XML representation of the signature
            XmlElement xmlFirmaDigital = xmlFirmado.GetXml();

            // Append the signature element to the XML document
            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlFirmaDigital, true));

            return xmlDoc;
        }
    }

    public class SignXmlRequest
    {
        public string XmlContent { get; set; }
        public string P12Base64 { get; set; }
        public string P12Password { get; set; }
    }
}