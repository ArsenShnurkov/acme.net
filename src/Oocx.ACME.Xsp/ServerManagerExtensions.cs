namespace Oocx.ACME.Xsp
{
	using System;
	using System.Linq;
	using ApacheModmono.Web.Administration;

	public static class ServerManagerExtensions
	{
		public static Site GetSiteForDomain(this ServerManager manager, string domain)
		{
			var sites = manager.Sites;
			var res = sites.SingleOrDefault(s => s.Bindings.Any(b => string.Equals(domain, b.Host, StringComparison.OrdinalIgnoreCase)));
			return res;
		}
	}
}