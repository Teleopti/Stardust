using System;
using System.Globalization;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core
{
	public class DateOnlyModelBinder : IModelBinder
	{
		public DateOnlyModelBinder()
		{
			Year = "year";
			Month = "month";
			Day = "day";
		}

		public string Year { get; set; }
		public string Month { get; set; }
		public string Day { get; set; }

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return bindFromModelName(bindingContext) ?? bindFromParts(bindingContext);
		}

		private static object bindFromModelName(ModelBindingContext bindingContext)
		{
			var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			DateTime date;
			if (result == null  || result.AttemptedValue == null || !DateTime.TryParse(result.AttemptedValue, out date))
			{
				return null;
			}
			return new DateOnly(date);
		}
	

		private object bindFromParts(ModelBindingContext bindingContext)
		{
			var year = getValue<int>(bindingContext, Year);
			var month = getValue<int>(bindingContext, Month);
			var day = getValue<int>(bindingContext, Day);

			if (!year.HasValue || !month.HasValue || !day.HasValue)
			{
				return null;
			}
			var calendar = CultureInfo.CurrentCulture.Calendar;
			var dateTime = new DateTime(year.Value, month.Value, 1, calendar).AddDays(day.Value - 1);
			return new DateOnly(dateTime);
		}

		private static T? getValue<T>(ModelBindingContext bindingContext, string key) where T : struct
		{
			if (string.IsNullOrEmpty(key))
			{
				return null;
			}

			var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + "." + key);

			if (result == null && bindingContext.FallbackToEmptyPrefix)
			{
				result = bindingContext.ValueProvider.GetValue(key);
			}

			if (result == null)
			{
				return null;
			}


			return (T?) result.ConvertTo(typeof (T));
		}
	}
}