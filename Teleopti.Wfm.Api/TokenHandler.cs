using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Teleopti.Wfm.Api
{
	public class TokenHandler : OwinMiddleware
	{
		private readonly ITokenVerifier hash;

		public TokenHandler(OwinMiddleware next, ITokenVerifier hash) : base(next)
		{
			this.hash = hash;
		}

		public override Task Invoke(IOwinContext context)
		{
			var auth = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(auth))
			{
				context.Response.StatusCode = 401;
				return Task.FromResult(false);
			}

			var token = auth.Replace("bearer", "").Replace("Bearer", "").Replace("BEARER", "").Replace(":", "").Trim();
			if (!hash.TryGetUser(token, out var user))
			{
				context.Response.StatusCode = 401;
				return Task.FromResult(false);
			}

			context.Request.User = new ClaimsPrincipal(new ClaimsIdentity("token", "nameidentifier", user));

			// Call the next delegate/middleware in the pipeline
			return Next.Invoke(context);
		}
	}
}