using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace StaffHubPoC
{
	public static class OAuthFlow
	{
		private static string code;
		private static Form form;
		private static string url;

		public async static Task<string> AdalOAuthFlow(string clientId, string resourceId, Uri redirectURI, string authority, string authorizeUri, bool consent, string secret)
		{
			string query = (consent ? "&prompt=consent" : string.Empty) + (consent ? "&prompt=consent" : string.Empty);
			query = query.TrimStart('&');

			var authContext = new AuthenticationContext(authority);
			AuthenticationResult authToken;
			if (string.IsNullOrEmpty(secret))
			{
				authToken = await authContext.AcquireTokenAsync(resourceId, clientId, redirectURI, new PlatformParameters(PromptBehavior.Auto), UserIdentifier.AnyUser, query);
			}
			else
			{
				throw new NotImplementedException("ADAL expected confidential clients to obtain the authcode using their own stack as written here: http://www.cloudidentity.com/blog/2013/09/12/active-directory-authentication-library-adal-v1-for-net-general-availability/");
			}

			return authToken.AccessToken;
		}


		[STAThread]
		public static string Flow(string clientId, string resourceId, Uri redirectURI, string authority,
			string authorizeUri, bool consent, string secret)
		{
			string state = Guid.NewGuid().ToString("N");
			var browser = new WebBrowser
			{
				Dock = DockStyle.Fill,
				Name = "browser"
			};
			var uri = new Uri(
				$"{authorizeUri}?client_id={clientId}&response_type=code&redirect_uri={HttpUtility.UrlEncode(redirectURI.AbsoluteUri)}&response_mode=query&resource={resourceId}&state={state}" +
				(consent ? "&prompt=consent" : string.Empty));
			browser.Url = uri;
			browser.Navigated += Browser_Navigated;
			form = new Form
			{
				WindowState = FormWindowState.Normal,
				Height = 800,
				Width = 600
			};
			form.Controls.Add(browser);
			form.Name = "browserForm";

			Application.Run(form);

			var request = (HttpWebRequest)HttpWebRequest.Create(authority);
			request.Method = "POST";

			if (!string.IsNullOrEmpty(secret))
			{
				secret = $"&client_secret={HttpUtility.UrlEncode(secret)}";
			}

			var data = Encoding.ASCII.GetBytes(
				$"grant_type=authorization_code&client_id={clientId}&code={code}&redirect_uri={HttpUtility.UrlEncode(redirectURI.AbsoluteUri)}&resource={resourceId}{secret}");
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			using (var stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}

			try
			{
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					string body;
					using (Stream receiveStream = response.GetResponseStream())
					{
						using (StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("utf-8")))
						{
							body = readStream.ReadToEnd();
						}
					}

					Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
					return result["access_token"];
				}
			}
			catch (WebException ex)
			{
				string body;
				using (Stream receiveStream = ex.Response.GetResponseStream())
				{
					using (StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("utf-8")))
					{
						body = readStream.ReadToEnd();
					}
				}

				Console.WriteLine(body);

				return string.Empty;
			}
		}

		private static void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			url = e.Url.AbsoluteUri;
			if (e.Url.AbsoluteUri.Contains("error"))
			{
				Console.WriteLine(HttpUtility.UrlDecode(e.Url.AbsoluteUri));
				form.Close();
				return;
			}

			if (e.Url.Query.Contains("code="))
			{
				string codeParam = e.Url.Query.Split(new char[] { '&' }).First(s => s.StartsWith("code=") || s.StartsWith("?code="));
				codeParam = codeParam.TrimStart(new char[] { '?' });
				code = codeParam.Substring("code=".Length);
				form.Close();
			}
		}

	}
}