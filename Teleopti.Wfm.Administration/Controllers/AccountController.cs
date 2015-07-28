using System;
using System.Security.Cryptography;
using System.Web.Http;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class AccountController : ApiController
	{

		[HttpPost]
		[Route("Login")]
		public LoginResult Login(LoginModel model)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(model.UserName);
			SHA1 sha = new SHA1CryptoServiceProvider();
			var hashed = sha.ComputeHash(bytes);

			return new LoginResult {UserName = model.UserName, AccessToken = hashed};
		}
	}

	public class LoginModel
	{
		public string GrantType { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class LoginResult
	{
		public byte[] AccessToken { get; set; }
		public string UserName { get; set; }
	}
}