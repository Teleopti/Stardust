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
			binders[typeof (DateOnly?)] = dateOnlyModelBinder;
			binders[typeof (DateOnly)] = dateOnlyModelBinder;
			binders[typeof (TimeOfDay)] = timeOfDayModelBinder;
			binders[typeof(TimeOfDay?)] = timeOfDayModelBinder;
		}
	}
}