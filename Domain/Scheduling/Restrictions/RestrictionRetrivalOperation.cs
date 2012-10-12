using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IRestrictionRetrivalOperation
	{
		IEnumerable<IRotationRestriction> GetRotationRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IAvailabilityRestriction> GetAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IPreferenceRestriction> GetPreferenceRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDays(IScheduleDay scheduleDay);
	}

	public class RestrictionRetrivalOperation : IRestrictionRetrivalOperation
	{
		public IEnumerable<IRotationRestriction> GetRotationRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Rotation).Cast<IRotationRestriction>().ToArray();
		}

		public IEnumerable<IAvailabilityRestriction> GetAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Availability).Cast<IAvailabilityRestriction>().ToArray();
		}

		public IEnumerable<IPreferenceRestriction> GetPreferenceRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Preference).Cast<IPreferenceRestriction>().ToArray();
		}

		public IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDays(IScheduleDay scheduleDay)
		{
			return scheduleDay.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().ToArray();
		}

	}
}