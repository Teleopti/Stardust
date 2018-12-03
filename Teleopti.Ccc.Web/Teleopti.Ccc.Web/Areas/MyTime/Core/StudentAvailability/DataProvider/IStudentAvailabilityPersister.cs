using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider
{
	public interface IStudentAvailabilityPersister
	{
		StudentAvailabilityDayViewModel Persist(StudentAvailabilityDayInput input);
		StudentAvailabilityDayViewModel Delete(DateOnly date);
	}
}