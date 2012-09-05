using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(3)]
	public class RegisterModelBindersTask : IBootstrapperTask
	{
		public void Execute()
		{
			RegisterModelBinders(ModelBinders.Binders);
		}

		public static void RegisterModelBinders(ModelBinderDictionary binders)
		{
			var dateOnlyModelBinder = new DateOnlyModelBinder();
			var timeOfDayModelBinder = new TimeOfDayModelBinder();
			var nullableTimeOfDayModelBinder = new TimeOfDayModelBinder(nullable:true);
			var timeSpanModelBinder = new TimeSpanModelBinder();
			var nullableTimeSpanModelBinder = new TimeSpanModelBinder(nullable:true);

			binders[typeof (DateOnly?)] = dateOnlyModelBinder;
			binders[typeof (DateOnly)] = dateOnlyModelBinder;
			binders[typeof (TimeOfDay)] = timeOfDayModelBinder;
			binders[typeof(TimeOfDay?)] = nullableTimeOfDayModelBinder;
			binders[typeof(TimeSpan)] = timeSpanModelBinder;
			binders[typeof(TimeSpan?)] = nullableTimeSpanModelBinder;
		}
	}
}