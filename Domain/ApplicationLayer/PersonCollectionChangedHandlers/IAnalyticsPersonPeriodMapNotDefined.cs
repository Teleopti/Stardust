using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public interface IAnalyticsPersonPeriodMapNotDefined
	{
		int MaybeThrowErrorOrNotDefined(string error);
	}

	public class ThrowExceptionOnSkillMapError : IAnalyticsPersonPeriodMapNotDefined
	{
		public int MaybeThrowErrorOrNotDefined(string error)
		{
			throw new ArgumentException($"Error when mapping skills: {error}");
		}
	}
}