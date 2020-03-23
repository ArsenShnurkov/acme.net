using System;
using System.Diagnostics;
using Newtonsoft.Json;

public class AcmeDirectory
{
    [JsonProperty ("key-change")]
    public Uri KeyChange { get; set; }
}


namespace SyncConsole
{
    class MainClass
    {
        public static void Main (string [] args)
        {
            //get data as Json string 
            string json = @"
{
  ""key-change"": ""https://acme-staging.api.letsencrypt.org/acme/key-change""
}
";

            var responseContent1 = JsonConvert.DeserializeObject<AcmeDirectory> (json);

            Debug.Assert (responseContent1.KeyChange != null);
        }
    }
}
