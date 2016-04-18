using System;
using System.Net;

namespace Dogstar
{
	public class AquaHttpClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address) as HttpWebRequest;
			request.UserAgent = "AQUA_HTTP";
			return request;
		}
	}
}
