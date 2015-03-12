using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class ConfigController : Controller
	{
		[HttpGet]
		public virtual JsonResult SharedSettings()
		{
			var res = new SharedSettings
			{
				MessageBroker = ConfigurationManager.AppSettings.AllKeys.Contains("MessageBroker")  ? ConfigurationManager.AppSettings["MessageBroker"]: "",
				MessageBrokerLongPolling = ConfigurationManager.AppSettings.AllKeys.Contains("MessageBrokerLongPolling")  ? ConfigurationManager.AppSettings["MessageBrokerLongPolling"]: "",
				RtaPollingInterval = ConfigurationManager.AppSettings.AllKeys.Contains("RtaPollingInterval")  ? ConfigurationManager.AppSettings["RtaPollingInterval"]: "",
				Queue = ConfigurationManager.ConnectionStrings["Queue"] != null  ? Encryption.EncryptStringToBase64(ConfigurationManager.ConnectionStrings["Queue"].ToString(), EncryptionConstants.Image1,
				EncryptionConstants.Image2) : ""
			};
			return Json(res, JsonRequestBehavior.AllowGet);

		}
	}

	public class SharedSettings
	{
		public string Queue { get; set; }
		public string MessageBroker { get; set; }
		public string MessageBrokerLongPolling { get; set; }
		public string RtaPollingInterval { get; set; }
	}
}