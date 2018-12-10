using System;
using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class TimeSpanModelBinder : IModelBinder
	{
		private readonly bool _nullable;

		public TimeSpanModelBinder(bool nullable = false)
		{
			_nullable = nullable;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return bindFromModelName(bindingContext);
		}

		private object bindFromModelName(ModelBindingContext bindingContext)
		{
			var userValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).AttemptedValue;
			if (shouldReturnNull(userValue))
			{
				return null;
			}

			TimeSpan converterFromClient;

			userValue = userValue.Trim();
			if (TimeHelper.TryParseLongHourStringDefaultInterpretation(userValue, TimeSpan.FromDays(365), out converterFromClient, TimeFormatsType.HoursMinutes, false))
				return converterFromClient;

			bindingContext.ModelState.AddModelError("timeError", string.Format(CultureInfo.CurrentUICulture, Resources.InvalidTimeValue, userValue));
			return TimeSpan.Zero;
		}

		private bool shouldReturnNull(string userValue)
		{
			return _nullable && string.IsNullOrWhiteSpace(userValue);
		}
	}
}