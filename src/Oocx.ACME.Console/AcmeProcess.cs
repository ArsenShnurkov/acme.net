using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Oocx.ACME.Client;
using Oocx.ACME.IIS;
using Oocx.ACME.Protocol;
using Oocx.ACME.Services;
using Oocx.Asn1PKCS.PKCS10;
using Oocx.Asn1PKCS.PKCS12;
using static Oocx.ACME.Common.Log;

namespace Oocx.ACME.Console
{
    public class AcmeProcess : IAcmeProcess
    {
        private readonly Options options;
        private readonly IChallengeProvider challengeProvider;
        private readonly IServerConfigurationProvider serverConfiguration;
        private readonly IAcmeClient client;
        private readonly IPkcs12 pkcs12;

        private readonly ICertificateRequestAsn1DEREncoder certificateRequestEncoder;

        public AcmeProcess (Options options, IChallengeProvider challengeProvider, IServerConfigurationProvider serverConfiguration, IAcmeClient client, IPkcs12 pkcs12, ICertificateRequestAsn1DEREncoder certificateRequestEncoder)
        {
            this.options = options;
            this.challengeProvider = challengeProvider;
            this.serverConfiguration = serverConfiguration;
            this.client = client;
            this.pkcs12 = pkcs12;
            this.certificateRequestEncoder = certificateRequestEncoder;
        }

        public async Task StartAsync ()
        {
            IgnoreSslErrors ();

            await RegisterWithServer ();

            foreach (var domain in options.Domains) {
                bool isAuthorized = await AuthorizeForDomain (domain);
                if (!isAuthorized) {
                    Error ($"authorization for domain {domain} failed");
                    continue;
                }

                var keyPair = GetNewKeyPair ();

                var certificateResponse = await RequestCertificateForDomain (domain, keyPair);

                var certificatePath = SaveCertificateReturnedByServer (domain, certificateResponse);

                SaveCertificateWithPrivateKey (domain, keyPair, certificatePath);

                ConfigureServer (domain, certificatePath, keyPair, options.IISWebSite, options.IpBinding);
            }
        }

        private void ConfigureServer (string domain, string certificatePath, RSAParameters key, string siteName, string binding)
        {
            var certificateHash = serverConfiguration.InstallCertificateWithPrivateKey (certificatePath, "my", key);
            serverConfiguration.ConfigureServer (domain, certificateHash, "my", siteName, binding);
        }

        private async Task<CertificateResponse> RequestCertificateForDomain (string domain, RSAParameters key)
        {
            var csr = CreateCertificateRequest (domain, key);
            return await client.NewCertificateRequestAsync (csr);
        }

        private static RSAParameters GetNewKeyPair ()
        {
            var rsa = new RSACryptoServiceProvider (2048);
            using (FileStream file = new FileStream ("private.key", FileMode.Create))
            using (TextWriter stream = new StreamWriter (file)) {
                ExportPrivateKey (rsa, stream);
            }
            var key = rsa.ExportParameters (true);
            return key;
        }

        private void SaveCertificateWithPrivateKey (string domain, RSAParameters key, string certificatePath)
        {
            Info ("generating pfx file with certificate and private key");
            GetPfxPasswordFromUser ();

            try {
                var pfxPath = Path.Combine (Environment.CurrentDirectory, $"{domain}.pfx");
                pkcs12.CreatePfxFile (key, certificatePath, options.PfxPassword, pfxPath);
                Info ($"pfx file saved to {pfxPath}");
            } catch (Exception ex) {
                Error ("could not create pfx file: " + ex);
            }
        }

        private byte [] CreateCertificateRequest (string domain, RSAParameters key)
        {
            var data = new CertificateRequestData (domain, key);
            var csr = certificateRequestEncoder.EncodeAsDER (data);
            return csr;
        }

        private void GetPfxPasswordFromUser ()
        {
            System.Console.CursorVisible = false;

            while (null == options.PfxPassword) {
                System.Console.Write ("Enter password for pfx file: ");
                var color = System.Console.ForegroundColor;
                System.Console.ForegroundColor = System.Console.BackgroundColor;

                string pass1 = System.Console.ReadLine ();
                System.Console.ForegroundColor = color;

                System.Console.Write ("Repeat the password: ");
                System.Console.ForegroundColor = System.Console.BackgroundColor;

                string pass2 = System.Console.ReadLine ();
                System.Console.ForegroundColor = color;

                if (pass1 == pass2) {
                    options.PfxPassword = pass1;
                } else {
                    System.Console.WriteLine ("The passwords do not match.");
                }
            }
            System.Console.CursorVisible = true;
        }

        private static string SaveCertificateReturnedByServer (string domain, CertificateResponse response)
        {
            var certificatePath = Path.Combine (Environment.CurrentDirectory, $"{domain}.cer");
            Info ($"saving certificate returned by ACME server to {certificatePath}");
            File.WriteAllBytes (certificatePath, response.Certificate);
            return certificatePath;
        }

        private async Task<bool> AuthorizeForDomain (string domain)
        {
            var authorization = await client.NewDnsAuthorizationAsync (domain);

            var challenge = await challengeProvider.AcceptChallengeAsync (domain, options.IISWebSite, authorization);
            if (challenge == null) {
                return false;
            }

            System.Console.WriteLine (challenge.Instructions);
            if (!options.AcceptInstructions) {
                System.Console.WriteLine ("Press ENTER to continue");
                System.Console.ReadLine ();
            } else {
                System.Console.WriteLine ("Automatically accepting instructions.");
            }
            var challengeResult = await challenge.Complete ();
            return "valid".Equals (challengeResult?.Status, StringComparison.OrdinalIgnoreCase);
        }

        private async Task RegisterWithServer ()
        {
            Verbose ($"Contact: {string.Join (", ", options.Contact)}");
            string termsOfServiceUri = options.AcceptTermsOfService ? options.TermsOfServiceUri : null;
            string [] contacts = new [] { options.Contact };
            var registration = await client.RegisterAsync (termsOfServiceUri, contacts);
            Info ($"Terms of service: {registration.Agreement}");
            Verbose ($"Created at: {registration.CreatedAt}");
            Verbose ($"Id: {registration.Id}");
            Verbose ($"Contact: {string.Join (", ", registration.Contact)}");
            Verbose ($"Initial Ip: {registration.InitialIp}");

            if (!string.IsNullOrWhiteSpace (registration.Location) && options.AcceptTermsOfService) {
                Info ("accepting terms of service");
                if (!string.Equals (registration.Agreement, options.TermsOfServiceUri)) {
                    Error ($"Cannot accept terms of service. The terms of service uri is '{registration.Agreement}', expected it to be '{options.TermsOfServiceUri}'.");
                    return;
                }
                await client.UpdateRegistrationAsync (registration.Location, registration.Agreement, new [] { options.Contact });
            }
        }

        private void IgnoreSslErrors ()
        {
            if (options.IgnoreSSLCertificateErrors) {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => {
                    if (sslPolicyErrors != SslPolicyErrors.None)
                        Verbose ($"ignoring SSL certificate error: {sslPolicyErrors}");
                    return true;
                };
            }
        }

        private static void ExportPrivateKey (RSACryptoServiceProvider csp, TextWriter outputStream)
        {
            if (csp.PublicOnly) throw new ArgumentException ("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters (true);
            using (var stream = new MemoryStream ()) {
                var writer = new BinaryWriter (stream);
                writer.Write ((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream ()) {
                    var innerWriter = new BinaryWriter (innerStream);
                    EncodeIntegerBigEndian (innerWriter, new byte [] { 0x00 }); // Version
                    EncodeIntegerBigEndian (innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian (innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian (innerWriter, parameters.D);
                    EncodeIntegerBigEndian (innerWriter, parameters.P);
                    EncodeIntegerBigEndian (innerWriter, parameters.Q);
                    EncodeIntegerBigEndian (innerWriter, parameters.DP);
                    EncodeIntegerBigEndian (innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian (innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength (writer, length);
                    writer.Write (innerStream.GetBuffer (), 0, length);
                }

                var base64 = Convert.ToBase64String (stream.GetBuffer (), 0, (int)stream.Length).ToCharArray ();
                outputStream.WriteLine ("-----BEGIN RSA PRIVATE KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64) {
                    outputStream.WriteLine (base64, i, Math.Min (64, base64.Length - i));
                }
                outputStream.WriteLine ("-----END RSA PRIVATE KEY-----");
            }
        }

        private static void EncodeLength (BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException ("length", "Length must be non-negative");
            if (length < 0x80) {
                // Short form
                stream.Write ((byte)length);
            } else {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0) {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write ((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--) {
                    stream.Write ((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private static void EncodeIntegerBigEndian (BinaryWriter stream, byte [] value, bool forceUnsigned = true)
        {
            stream.Write ((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++) {
                if (value [i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0) {
                EncodeLength (stream, 1);
                stream.Write ((byte)0);
            } else {
                if (forceUnsigned && value [prefixZeros] > 0x7f) {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength (stream, value.Length - prefixZeros + 1);
                    stream.Write ((byte)0);
                } else {
                    EncodeLength (stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++) {
                    stream.Write (value [i]);
                }
            }
        }
    }
}

