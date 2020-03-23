using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using static Log;

public class CertificateRequest
{
    [JsonProperty ("resource")]
    public string Resource => "new-cert";

    [JsonProperty ("csr")]
    public string Csr { get; set; }
}

public class CertificateResponse
{
    public string Location { get; set; }
    public byte [] Certificate { get; set; }
}

class AcmeClient
{
    /*
        WebClient client;

        private AcmeDirectory directory;

        private string nonce;

        //private readonly JWS jws;

        internal CertificateResponse NewCertificateRequest (byte [] csr)
        {
            EnsureDirectory ();

            Info ("requesting certificate");

            var request = new CertificateRequest { Csr = csr.Base64UrlEncoded () };
            var response = Post<CertificateResponse> (directory.NewCertificate, request);

            Verbose ($"location: {response.Location}");
            return response;
        }

        public void EnsureDirectory ()
        {
            if (directory == null || nonce == null) {
                directory = Discover ();
            }
        }

        public AcmeDirectory Discover ()
        {
            Verbose ($"Querying directory information from {client.BaseAddress}");
            var dir = Get<AcmeDirectory> (new Uri ("directory", UriKind.Relative));
            return dir;
        }
    */
}
