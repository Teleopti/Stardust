using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class LandingPageController : ApiController
	{
		private readonly IToggleManager _toggleManager;
		private readonly IPermissionProvider _permissionProvider;

		public LandingPageController(IToggleManager toggleManager, IPermissionProvider permissionProvider)
		{
			_toggleManager = toggleManager;
			_permissionProvider = permissionProvider;
		}


		[HttpGet, Route("api/Anywhere/GetLandingPage"), UnitOfWork]
		public virtual string GetLandingPage()
		{
			var linkForLandingPage = "";
			var loginId = Guid.NewGuid();
			var loginData = new Hashtable() { { "id", loginId } };
			var haspermission = _permissionProvider.HasApplicationFunctionPermission(
				DefinedRaptorApplicationFunctionPaths.ViewCustomerCenter);

			if (haspermission && _toggleManager.IsEnabled(Toggles.WFM_Connect_NewLandingPage_WEB_78578))
			{
				var response = authenticateUser(loginData);
				if (response.StatusCode == HttpStatusCode.OK)
					linkForLandingPage = "https://www.teleopti.com/alogin.aspx?id=" + loginId;
			}
			return linkForLandingPage;
		}

		private HttpResponseMessage authenticateUser(Hashtable loginData)
		{
			StringContent stringContent = new StringContent(JsonConvert.SerializeObject(encrypt(loginData)),
				Encoding.UTF8, "application/json");
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				var response = client
					.PostAsync("https://www.teleopti.com/api/v1/autologin/init", stringContent)
					.GetAwaiter().GetResult();
				return response;
			}
		}

		private string encrypt(Hashtable DataToEncrypt)
		{
			string keyString = "12345678";
			string ivString = "qwertyui";

			DESCryptoServiceProvider DES = new DESCryptoServiceProvider() { IV = Encoding.UTF8.GetBytes(ivString), Key = Encoding.UTF8.GetBytes(keyString) };
			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, DES.CreateEncryptor(), CryptoStreamMode.Write))
				{
					SoapFormatter formatter = new SoapFormatter();
					formatter.Serialize(cs, DataToEncrypt);
					cs.FlushFinalBlock();
				}

				return Convert.ToBase64String(ms.ToArray());
			}
		}
	}
}