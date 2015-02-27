using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class SsoAuthenticationModelBinder : IModelBinder
	{
		private readonly IApplicationAuthenticationType _type;

		public SsoAuthenticationModelBinder(IApplicationAuthenticationType type)
		{
			_type = type;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return _type.BindModel(bindingContext);
		}
	}
}