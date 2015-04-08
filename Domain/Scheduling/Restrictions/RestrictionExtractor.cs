using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionExtractor : IRestrictionExtractor
	{
		private readonly IRestrictionCombiner _restrictionCombiner;
		private readonly IRestrictionRetrievalOperation _restrictionRetrievalOperation;

		public RestrictionExtractor(IRestrictionCombiner restrictionCombiner, IRestrictionRetrievalOperation restrictionRetrievalOperation)
		{
			_restrictionCombiner = restrictionCombiner;
			_restrictionRetrievalOperation = restrictionRetrievalOperation;
		}

		public IExtractedRestrictionResult Extract(IScheduleDay scheduleDay)
		{
			if (scheduleDay == null)
				return new ExtractedRestrictionResult(_restrictionCombiner, Enumerable.Empty<IRotationRestriction>(),
					Enumerable.Empty<IAvailabilityRestriction>(), Enumerable.Empty<IPreferenceRestriction>(),
					Enumerable.Empty<IStudentAvailabilityDay>());

			var restrictions = scheduleDay.RestrictionCollection().ToArray();
			return new ExtractedRestrictionResult(_restrictionCombiner, _restrictionRetrievalOperation.GetRotationRestrictions(restrictions),
				_restrictionRetrievalOperation.GetAvailabilityRestrictions(restrictions),
				_restrictionRetrievalOperation.GetPreferenceRestrictions(restrictions),
				_restrictionRetrievalOperation.GetStudentAvailabilityDays(scheduleDay));
		}
	}
}