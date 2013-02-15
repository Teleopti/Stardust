//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Configuration;
using System.Web;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Provider;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;

namespace Teleopti.Ccc.Web.IdentityProvider.Code
{
	public class Util
	{
		/// <summary>
		/// Gets a value indicating whether the authentication system used by the OP requires
		/// no user interaction (an HTTP header based authentication protocol).
		/// </summary>
		internal static bool ImplicitAuth
		{
			get
			{
				// This should return false if using FormsAuthentication.
				return bool.Parse(ConfigurationManager.AppSettings["ImplicitAuth"]);
			}
		}

		private static string extractUserName(Uri url)
		{
			return url.Segments[url.Segments.Length - 1];
		}

		private static string extractUserName(Identifier identifier)
		{
			return extractUserName(new Uri(identifier.ToString()));
		}

		private static Identifier buildIdentityUrl()
		{
			string username = HttpContext.Current.User.Identity.Name;
			int slash = username.IndexOf('\\');
			if (slash >= 0)
			{
				username = username.Substring(slash + 1);
			}
			return buildIdentityUrl(username);
		}

		private static Identifier buildIdentityUrl(string username)
		{
			// This sample Provider has a custom policy for normalizing URIs, which is that the whole
			// path of the URI be lowercase except for the first letter of the username.
			username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1).ToLowerInvariant();
			return new Uri(HttpContext.Current.Request.Url, HttpContext.Current.Response.ApplyAppPathModifier("~/OpenId/AskUser/" + username));
		}

		public static ActionResult ProcessAuthenticationChallenge(IAuthenticationRequest idrequest)
		{
			// Verify that RP discovery is successful.
			if (idrequest.IsReturnUrlDiscoverable(ProviderEndpoint.Provider.Channel.WebRequestHandler) != RelyingPartyDiscoveryResult.Success)
			{
				idrequest.IsAuthenticated = false;
				ProviderEndpoint.Provider.PrepareResponse(idrequest).AsActionResult();
			}

			// Verify that the RP is on the whitelist.  Realms are case sensitive.
			string[] whitelist = ConfigurationManager.AppSettings["whitelistedRealms"].Split(';');
			if (Array.IndexOf(whitelist, idrequest.Realm.ToString()) < 0)
			{
				idrequest.IsAuthenticated = false;
				ProviderEndpoint.Provider.PrepareResponse(idrequest).AsActionResult();
			}

			if (idrequest.IsDirectedIdentity)
			{
				if (HttpContext.Current.User.Identity.IsAuthenticated)
				{
					idrequest.LocalIdentifier = buildIdentityUrl();
					idrequest.IsAuthenticated = true;
				}
				else
				{
					// If the RP demands an immediate answer, or if we're using implicit authentication
					// and therefore have nothing further to ask the user, just reject the authentication.
					if (idrequest.Immediate || ImplicitAuth)
					{
						idrequest.IsAuthenticated = false;
					}
					else
					{
						// Send the user to a page to actually log into the OP.
						if (!HttpContext.Current.Request.Path.EndsWith("Account/Logon", StringComparison.OrdinalIgnoreCase)
							|| !HttpContext.Current.Request.Path.EndsWith("Account/Logon/", StringComparison.OrdinalIgnoreCase))
						{
							return new RedirectResult("~/Account/Logon");
						}
					}
				}
			}
			else
			{
				string userOwningOpenIdUrl = extractUserName(idrequest.LocalIdentifier);

				// NOTE: in a production provider site, you may want to only 
				// respond affirmatively if the user has already authorized this consumer
				// to know the answer.
				idrequest.IsAuthenticated = userOwningOpenIdUrl == HttpContext.Current.User.Identity.Name;

				if (!idrequest.IsAuthenticated.Value && !ImplicitAuth && !idrequest.Immediate)
				{
					// Send the user to a page to actually log into the OP.
					if (!HttpContext.Current.Request.Path.EndsWith("Account/Logon", StringComparison.OrdinalIgnoreCase)
							|| !HttpContext.Current.Request.Path.EndsWith("Account/Logon/", StringComparison.OrdinalIgnoreCase))
					{
						return new RedirectResult("~/Account/Logon");
					}
				}
			}

			if (idrequest.IsAuthenticated != null && idrequest.IsAuthenticated.Value)
			{
				// add extension responses here.
				var fetchRequest = idrequest.GetExtension<FetchRequest>();
				if (fetchRequest != null)
				{
					var fetchResponse = new FetchResponse();
					//if (fetchRequest.Attributes.Contains(RolesAttribute))
					//{
					//    // Inform the RP what roles this user should fill
					//    // These roles would normally come out of the user database
					//    // or Windows security groups.
					//    fetchResponse.Attributes.Add(RolesAttribute, "Member", "Admin");
					//}
					idrequest.AddResponseExtension(fetchResponse);
				}
			}

			return new EmptyResult();
		}
	}
}