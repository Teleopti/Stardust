using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public interface IStudentAvailabilityPersister
	{
		StudentAvailabilityDayViewModel Persist(StudentAvailabilityDayForm form);
		StudentAvailabilityDayViewModel Delete(DateOnly date);
	}
}