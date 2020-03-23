using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using static Log;

class AcmeProcess
{
    /*
        private readonly Options options;
        private readonly ChallengeProvider challengeProvider;
        private readonly ServerConfigurationProvider serverConfiguration;
        private readonly AcmeClient client;
        private readonly Pkcs12 pkcs12;

        private readonly CertificateRequestAsn1DEREncoder certificateRequestEncoder;

        public AcmeProcess (Options options, ChallengeProvider challengeProvider, ServerConfigurationProvider serverConfiguration, AcmeClient client, Pkcs12 pkcs12, CertificateRequestAsn1DEREncoder certificateRequestEncoder)
        {
            this.options = options;
            this.challengeProvider = challengeProvider;
            this.serverConfiguration = serverConfiguration;
            this.client = client;
            this.pkcs12 = pkcs12;
            this.certificateRequestEncoder = certificateRequestEncoder;
        }

        public void StartSync ()
        {
            IgnoreSslErrors ();

            RegisterWithServer ();

            foreach (var domain in options.Domains) {
                bool isAuthorized = AuthorizeForDomain (domain);
                if (!isAuthorized) {
                    Error ($"authorization for domain {domain} failed");
                    continue;
                }

                var keyPair = GetNewKeyPair ();

                var certificateResponse = RequestCertificateForDomain (domain, keyPair);

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

        private CertificateResponse RequestCertificateForDomain (string domain, RSAParameters key)
        {
            var csr = CreateCertificateRequest (domain, key);
            return client.NewCertificateRequestAsync (csr);
        }

        private static RSAParameters GetNewKeyPair ()
        {
            var rsa = new RSACryptoServiceProvider (2048);
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

        private bool AuthorizeForDomain (string domain)
        {
            var authorization = client.NewDnsAuthorizationAsync (domain);

            var challenge = challengeProvider.AcceptChallengeAsync (domain, options.IISWebSite, authorization);
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
            var challengeResult = challenge.Complete ();
            return "valid".Equals (challengeResult?.Status, StringComparison.OrdinalIgnoreCase);
        }

        private void RegisterWithServer ()
        {
            Verbose ($"Contact: {string.Join (", ", options.Contact)}");
            string termsOfServiceUri = options.AcceptTermsOfService ? options.TermsOfServiceUri : null;
            string [] contacts = new [] { options.Contact };
            var registration = client.RegisterAsync (termsOfServiceUri, contacts);
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
                client.UpdateRegistrationAsync (registration.Location, registration.Agreement, new [] { options.Contact });
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
    */
}
