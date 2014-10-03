using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MessageBrokerTool
{
	public class MessageBrokerToolAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "MessageBrokerTool";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"MessageBrokerTool_default",
				"MessageBrokerTool/{controller}/{action}/{id}",
				new { controller = "Application" ,action = "Index", id = UrlParameter.Optional }
			);
		}
	}
}
