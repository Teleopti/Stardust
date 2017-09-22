using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferenceOptionsProvider : IPreferenceOptionsProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public PreferenceOptionsProvider(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IShiftCategory> RetrieveShiftCategoryOptions()
		{
            return GetWorkflowControlSetData(w => w.AllowedPreferenceShiftCategories).Where(a => ((ShiftCategory)a).IsDeleted == false);
		}

		public IEnumerable<IDayOffTemplate> RetrieveDayOffOptions()
		{
            return GetWorkflowControlSetData(w => w.AllowedPreferenceDayOffs).Where(a => ((DayOffTemplate)a).IsDeleted == false);
		}

		public IEnumerable<IAbsence> RetrieveAbsenceOptions()
		{
            return GetWorkflowControlSetData(w => w.AllowedPreferenceAbsences).Where(a => ((Absence)a).IsDeleted == false);
		}

		public IEnumerable<IActivity> RetrieveActivityOptions()
		{
			var activity = GetWorkflowControlSetData(w => w.AllowedPreferenceActivity, () => null);
			if (activity == null)
				return new IActivity[] {};
			return new[] {activity};
		}

		private IEnumerable<T> GetWorkflowControlSetData<T>(Func<IWorkflowControlSet, IEnumerable<T>> getData)
		{
			return GetWorkflowControlSetData(getData, () => new T[] {});
		}

		private T GetWorkflowControlSetData<T>(Func<IWorkflowControlSet, T> getData, Func<T> @default)
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet == null)
				return @default.Invoke();
			return getData.Invoke(person.WorkflowControlSet);
		}


	}
}