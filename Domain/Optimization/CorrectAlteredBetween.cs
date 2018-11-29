using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class CorrectAlteredBetween
	{
		private readonly IUserTimeZone _userTimeZone;

		public CorrectAlteredBetween(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public bool Execute(DateOnly date,
											IVisualLayerCollection oldProjection, 
											IVisualLayerCollection currentProjection,
											OptimizerActivitiesPreferences optimizerActivitiesPreferences)
		{
			if (optimizerActivitiesPreferences?.AllowAlterBetween == null)
				return true;

			var timeZone = _userTimeZone.TimeZone();
			var allowPeriod = optimizerActivitiesPreferences.UtcPeriodFromDateAndTimePeriod(date, timeZone).Value;
			var baseDate = TimeZoneHelper.ConvertToUtc(date.Date, timeZone);
			var start = baseDate.AddDays(-1).Add(allowPeriod.EndDateTime.Subtract(baseDate));
			var periodBefore = new DateTimePeriod(start, allowPeriod.StartDateTime);
			var shiftLayersOutsideBefore = currentProjection.FilterLayers(periodBefore);
			var originalLayersOutsideBefore = oldProjection.FilterLayers(periodBefore);

			if (!shiftLayersOutsideBefore.IsSatisfiedBy(VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideBefore)))
				return false;

			var end = baseDate.AddDays(1).Add(allowPeriod.StartDateTime.Subtract(baseDate));
			var periodAfter = new DateTimePeriod(allowPeriod.EndDateTime, end);
			var shiftLayersOutsideAfter = currentProjection.FilterLayers(periodAfter);
			var originalLayersOutsideAfter = oldProjection.FilterLayers(periodAfter);
			if (!shiftLayersOutsideAfter.IsSatisfiedBy(VisualLayerCollectionSpecification.IdenticalLayers(originalLayersOutsideAfter)))
				return false;

			return true;
		}
	}
}