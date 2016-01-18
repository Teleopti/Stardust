using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace Teleopti.Ccc.Web.Areas.SSO.Models
{
	[ModelBinder(typeof(ApplicationAuthenticationConverter))]
	public class ApplicationAuthenticationModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}

	public class ApplicationAuthenticationConverter : IModelBinder
	{
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof (ApplicationAuthenticationModel))
			{
				return false;
			}

			ValueProviderResult username = bindingContext.ValueProvider.GetValue("username");
			ValueProviderResult password = bindingContext.ValueProvider.GetValue("password");
			if (username == null || password == null)
			{
				return false;
			}

			if (username.RawValue == null || password.RawValue == null)
			{
				bindingContext.ModelState.AddModelError(
					bindingContext.ModelName, "Cannot convert value to application authentication");
				return false;
			}

			bindingContext.Model = new ApplicationAuthenticationModel{UserName = username.RawValue.ToString(),Password = password.RawValue.ToString()};
			return true;
		}
	}
}