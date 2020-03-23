using System.Collections.Generic;

public enum LogLevel
{
    Disabled,
    Error,
    Warning,
    Info,
    Verbose
}

public class Options
{
    public Options ()
    {
        AcmeServer = "https://acme-v01.api.letsencrypt.org";
        Domains = new string [] { "xmpp.bbs.io" };
        AcceptTermsOfService = true;
        TermsOfServiceUri = "https://letsencrypt.org/documents/LE-SA-v1.2-November-15-2017.pdf";
        IgnoreSSLCertificateErrors = true;
        Verbosity = LogLevel.Info;
        PfxPassword = null;
        AccountKeyContainerLocation = null;
        AccountKeyName = "acme-key";
        ChallengeProvider = "manual-http-01";
        ServerConfigurationProvider = "iis";
        IpBinding = "*:443:xmpp.bbs.io";
        Contact = "admin@xmpp.bbs.io";
        AcceptInstructions = true;
        IISWebSite = null;
    }

    public string AcmeServer { get; set; }

    public IEnumerable<string> Domains { get; set; }

    public bool AcceptTermsOfService { get; set; }

    public string TermsOfServiceUri { get; set; }

    public bool IgnoreSSLCertificateErrors { get; set; }

    public LogLevel Verbosity { get; set; }

    public string PfxPassword { get; set; }

    public string AccountKeyContainerLocation { get; set; }

    public string AccountKeyName { get; set; }

    public string ChallengeProvider { get; set; }

    public string ServerConfigurationProvider { get; set; }

    public string IpBinding { get; set; }

    public string Contact { get; set; }

    public bool AcceptInstructions { get; set; }

    public string IISWebSite { get; set; }
}

