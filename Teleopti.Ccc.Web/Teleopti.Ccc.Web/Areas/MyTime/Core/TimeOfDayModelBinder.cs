using System;
using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class TimeOfDayModelBinder : IModelBinder
	{
		private readonly bool _nullable;

		public TimeOfDayModelBinder(bool nullable = false)
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
			if (TimeHelper.TryParse(userValue, out converterFromClient))
				if ((int) converterFromClient.TotalDays == 0)
					return new TimeOfDay(converterFromClient);

			bindingContext.ModelState.AddModelError("timeError", string.Format(CultureInfo.CurrentUICulture, Resources.InvalidTimeValue, userValue));
			return new TimeOfDay();
		}

		private bool shouldReturnNull(string userValue)
		{
			return _nullable && string.IsNullOrWhiteSpace(userValue);
		}
	}
}