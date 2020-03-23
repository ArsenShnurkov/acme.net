using System;
using Newtonsoft.Json;

namespace Oocx.ACME.Protocol
{
    public class Directory
    {
        [JsonProperty ("key-change")]
        public string KeyChange { get; set; }

        [JsonProperty ("meta")]
        public DirectoryMeta Meta { get; set; }

        [JsonProperty ("new-authz")]
        public string NewAuthorization { get; set; }

        [JsonProperty ("new-cert")]
        public string NewCertificate { get; set; }

        [JsonProperty ("new-reg")]
        public string NewRegistration { get; set; }

        [JsonProperty ("revoke-cert")]
        public string RevokeCertificate { get; set; }

        [JsonProperty ("x988GrudFSA")]
        public string x988GrudFSA { get; set; }
    }

    public class DirectoryMeta
    {
        [JsonProperty ("caa-identities")]
        public string [] CaaIdentities { get; set; }

        [JsonProperty ("terms-of-service")]
        public string TermsOfService { get; set; }

        [JsonProperty ("website")]
        public string Website { get; set; }
    }
}
