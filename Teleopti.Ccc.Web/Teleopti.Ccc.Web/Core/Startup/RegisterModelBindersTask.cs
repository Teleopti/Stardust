using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(4)]
	public class RegisterModelBindersTask : IBootstrapperTask
	{
		public Func<ModelBinderDictionary> BindersGetter = () => ModelBinders.Binders;

		public Task Execute(IAppBuilder application)
		{
			RegisterModelBinders(BindersGetter());
			return null;
		}

		public void RegisterModelBinders(ModelBinderDictionary binders)
		{
			var dateOnlyModelBinder = new DateOnlyModelBinder();
			var timeOfDayModelBinder = new TimeOfDayModelBinder();
			var nullableTimeOfDayModelBinder = new TimeOfDayModelBinder(nullable: true);
			var timeSpanModelBinder = new TimeSpanModelBinder();
			var nullableTimeSpanModelBinder = new TimeSpanModelBinder(nullable: true);
			var shiftExchangeLookingForDayModelBinder = new EnumByStringModelBinder<ShiftExchangeLookingForDay>();

			binders[typeof (DateOnly?)] = dateOnlyModelBinder;
			binders[typeof (DateOnly)] = dateOnlyModelBinder;
			binders[typeof (TimeOfDay)] = timeOfDayModelBinder;
			binders[typeof (TimeOfDay?)] = nullableTimeOfDayModelBinder;
			binders[typeof (TimeSpan)] = timeSpanModelBinder;
			binders[typeof (TimeSpan?)] = nullableTimeSpanModelBinder;
			binders[typeof (ShiftExchangeLookingForDay)] = shiftExchangeLookingForDayModelBinder;
		}
	}
}