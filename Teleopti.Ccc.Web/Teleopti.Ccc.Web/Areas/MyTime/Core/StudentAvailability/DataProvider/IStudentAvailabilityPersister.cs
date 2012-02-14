using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public interface IStudentAvailabilityPersister
	{
		StudentAvailabilityDayFormResult Persist(StudentAvailabilityDayForm form);
		StudentAvailabilityDayFormResult Delete(DateOnly date);
	}
}