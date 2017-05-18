﻿namespace Oocx.ACME.Xsp
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Security.AccessControl;
	using System.Security.Principal;
	using System.Threading.Tasks;
	using Microsoft.Web.Administration;
	using Oocx.ACME.Client;
	using Oocx.ACME.Protocol;
	using static Oocx.ACME.Common.Log;
	using Directory = System.IO.Directory;
	using System.Reflection;

	// This project can output the Class library as a NuGet Package.
	// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
	public class XspChallengeProvider : IChallengeProvider
	{
		private readonly IAcmeClient client;
		private readonly ServerManager manager;

		public XspChallengeProvider(IAcmeClient client)
		{
			this.client = client;
			manager = new ServerManager();
		}

		public async Task<PendingChallenge> AcceptChallengeAsync(string domain, string siteName, AuthorizationResponse authorization)
		{
			var challenge = authorization?.Challenges.FirstOrDefault(c => c.Type == "http-01");
			if (challenge == null)
			{
				Error("the server does not accept challenge type http-01");
				return null;
			}
			Info($"accepting challenge {challenge.Type}");

			var keyAuthorization = client.GetKeyAuthorization(challenge.Token);

			if (siteName == null)
			{
				await AcceptChallengeForDomainAsync(domain, challenge.Token, keyAuthorization);
			}
			else
			{
				await AcceptChallengeForSiteAsync(siteName, challenge.Token, keyAuthorization);
			}

			return new PendingChallenge()
			{
				Instructions = $"using Xsp integration to complete the challenge.",
				Complete = () => client.CompleteChallengeAsync(challenge)
			};
		}

		/*public bool CanAcceptChallengeForDomain(string domain)
        {            
            return manager.GetSiteForDomain(domain) != null;
        }*/


		public async Task AcceptChallengeForDomainAsync(string domain, string token, string challengeJson)
		{
			Info($"XspChallengeService is accepting challenge with token {token} for domain {domain}");
			var root = GetXspRoot(domain);
			await CreateWellKnownDirectoryWithChallengeFileAsync(root, token, challengeJson);
		}

		public async Task AcceptChallengeForSiteAsync(string siteName, string token, string challengeJson)
		{
			Info($"XspChallengeService is accepting challenge with token {token} for Xsp web site '{siteName}'");
			SiteCollection sc = manager.Sites;
			Site site = (Site)sc[siteName];
			if (site == null)
			{
				Error($"Xsp web site '{siteName}' not found, cannot process challenge.");
				return;
			}

			Application a = (Application)site.Applications["/"];
			VirtualDirectory vd = (VirtualDirectory)a.VirtualDirectories["/"];
			string root = vd.PhysicalPath;
			await CreateWellKnownDirectoryWithChallengeFileAsync(root, token, challengeJson);
		}

		private string GetXspRoot(string domain)
		{
			Site site = manager.GetSiteForDomain(domain);
			Application app = site.Applications["/"];
			return app.VirtualDirectories["/"].PhysicalPath;
		}

		private static async Task CreateWellKnownDirectoryWithChallengeFileAsync(string root, string token, string keyAuthorization)
		{
			root = Environment.ExpandEnvironmentVariables(root);
			var wellKnownPath = Path.Combine(root, ".well-known");
			CreateDirectory(wellKnownPath);
			var acmePath = Path.Combine(wellKnownPath, "acme-challenge");
			CreateDirectory(acmePath);

			CreateWebConfig(acmePath);

			var challengeFilePath = Path.Combine(acmePath, token);

			Verbose($"writing challenge to {challengeFilePath}");
			using (var fs = new FileStream(challengeFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				var data = System.Text.Encoding.ASCII.GetBytes(keyAuthorization);
				await fs.WriteAsync(data, 0, data.Length);
			}
		}

		private static void CreateWebConfig(string acmePath)
		{
			var webConfigPath = Path.Combine(acmePath, "web.config");
			if (File.Exists(webConfigPath))
			{
				return;
			}

			Type iisChallengeProviderType = typeof(XspChallengeProvider);
			string resourceName = iisChallengeProviderType.ToString().Substring(0, iisChallengeProviderType.ToString().LastIndexOf(".", StringComparison.Ordinal)) + ".web.config";
			Verbose($"Creating file '{webConfigPath}' from internal resource '{resourceName}'");

			var webConfigStream = iisChallengeProviderType.GetTypeInfo().Assembly.GetManifestResourceStream(resourceName);
			using (var fileStream = new FileStream(webConfigPath, FileMode.CreateNew, FileAccess.Write))
			{
				webConfigStream.CopyTo(fileStream);
				webConfigStream.Flush();
				webConfigStream.Close();
			}
		}

		private static void CreateDirectory(string challengePath)
		{
			if (Directory.Exists(challengePath)) return;

			Verbose($"creating directory {challengePath}");
			System.IO.Directory.CreateDirectory(challengePath);
		}
	}
}