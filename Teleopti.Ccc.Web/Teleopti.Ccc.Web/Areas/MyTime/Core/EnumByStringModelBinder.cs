using System;
using System.Globalization;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class EnumByStringModelBinder<T> : IModelBinder where T : struct
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return bindFromModelName(bindingContext);
		}

		private object bindFromModelName(ModelBindingContext bindingContext)
		{
			var enumValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;

			T value;
			if (!Enum.TryParse(enumValue.Trim().Replace("-", ""), true, out value))
			{
				//bindingContext.ModelState.AddModelError("Argument Error",
				//	string.Format(CultureInfo.CurrentUICulture, "Invalid value: {0}", enumValue));
				throw new ArgumentException(string.Format("The enum given ({0}) must have any of the values {1}",
					string.Join(", ", Enum.GetNames(typeof (T))), enumValue));
			}
			return value;
		}
	}
}
