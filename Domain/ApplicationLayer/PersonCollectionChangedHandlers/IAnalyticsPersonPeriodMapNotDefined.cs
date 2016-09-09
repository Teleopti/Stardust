using System;
using log4net;

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

	public class ReturnNotDefined : IAnalyticsPersonPeriodMapNotDefined
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ReturnNotDefined));
		public int MaybeThrowErrorOrNotDefined(string error)
		{
			logger.Warn($"Potential error when mapping skills: {error}");
			return -1;
		}
	}
}