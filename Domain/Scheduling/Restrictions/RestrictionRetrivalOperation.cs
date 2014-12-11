using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IRestrictionRetrievalOperation
	{
		IEnumerable<IRotationRestriction> GetRotationRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IAvailabilityRestriction> GetAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IStudentAvailabilityRestriction> GetStudentAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IPreferenceRestriction> GetPreferenceRestrictions(IEnumerable<IRestrictionBase> restrictions);
		IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDays(IScheduleDay scheduleDay);
	}

	public class RestrictionRetrievalOperation : IRestrictionRetrievalOperation
	{
		public IEnumerable<IRotationRestriction> GetRotationRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Rotation).Cast<IRotationRestriction>().ToArray();
		}

		public IEnumerable<IAvailabilityRestriction> GetAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Availability).Cast<IAvailabilityRestriction>().ToArray();
		}

		public IEnumerable<IStudentAvailabilityRestriction> GetStudentAvailabilityRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.StudentAvailability).Cast<IStudentAvailabilityRestriction>().ToArray();
		}

		public IEnumerable<IPreferenceRestriction> GetPreferenceRestrictions(IEnumerable<IRestrictionBase> restrictions)
		{
			return restrictions.FilterBySpecification(RestrictionMustBe.Preference).Cast<IPreferenceRestriction>().ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IEnumerable<IStudentAvailabilityDay> GetStudentAvailabilityDays(IScheduleDay scheduleDay)
		{
			return scheduleDay.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().ToArray();
		}

	}
}