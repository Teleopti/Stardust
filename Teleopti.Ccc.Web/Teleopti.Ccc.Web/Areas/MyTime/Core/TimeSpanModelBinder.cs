using System;
using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class TimeSpanModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return bindFromModelName(bindingContext);
		}

		private object bindFromModelName(ModelBindingContext bindingContext)
		{
			var userValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;

			TimeSpan converterFromClient;
			if (TimeHelper.TryParseLongHourStringDefaultInterpretation(userValue, out converterFromClient, TimeFormatsType.HoursMinutes, false))
				return converterFromClient;

			bindingContext.ModelState.AddModelError("timeError", string.Format(CultureInfo.CurrentUICulture, Resources.InvalidTimeValue, userValue));
			return TimeSpan.Zero;
		}
	}
}