using System.Globalization;
using System.Linq;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class PreferenceOpenWithAllowedPreferencesWorkflowControlSet :  IUserSetup
	{
		public IShiftCategory ShiftCategory;
		public IDayOffTemplate DayOffTemplate;
		public IAbsence Absence;

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			ShiftCategory = TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AllowedPreferenceShiftCategories.First();
			DayOffTemplate = TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AllowedPreferenceDayOffs.First();
			Absence = TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AllowedPreferenceAbsences.First();
			user.WorkflowControlSet = TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences;
		}
	}
}