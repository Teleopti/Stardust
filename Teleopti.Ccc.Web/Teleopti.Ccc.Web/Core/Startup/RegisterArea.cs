using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class RegisterArea : IRegisterArea
	{
		public void Register(AreaRegistration registration)
		{
			var context = new AreaRegistrationContext(registration.AreaName, RouteTable.Routes);
			string ns = registration.GetType().Namespace;

			if (ns != null) context.Namespaces.Add($"{ns}.*");

			registration.RegisterArea(context);
		}
	}
}