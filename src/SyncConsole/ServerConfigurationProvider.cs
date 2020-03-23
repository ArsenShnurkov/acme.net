using System.Security.Cryptography;

public class ServerConfigurationProvider
{
    public byte [] InstallCertificateWithPrivateKey (string certificatePath, string certificateStoreName, RSAParameters privateKey)
    {
        return null;
    }

    public void ConfigureServer (string domain, byte [] certificateHash, string certificateStoreName, string siteName, string binding)
    {
    }
}
