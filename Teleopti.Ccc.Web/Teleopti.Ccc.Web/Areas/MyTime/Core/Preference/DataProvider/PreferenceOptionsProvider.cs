using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

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
			return GetWorkflowControlSetData(w => w.AllowedPreferenceShiftCategories);
		}

		public IEnumerable<IDayOffTemplate> RetrieveDayOffOptions()
		{
			return GetWorkflowControlSetData(w => w.AllowedPreferenceDayOffs);
		}

		public IEnumerable<IAbsence> RetrieveAbsenceOptions()
		{
			return GetWorkflowControlSetData(w => w.AllowedPreferenceAbsences);
		}

		private IEnumerable<T> GetWorkflowControlSetData<T>(Func<IWorkflowControlSet, IEnumerable<T>> getData)
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet == null)
				return new T[] {};
			return getData.Invoke(person.WorkflowControlSet);
		}

	}
}