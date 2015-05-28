using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkillEqualComparer : IEqualityComparer<IScheduleForecastSkillKey>
	{
		public bool Equals(IScheduleForecastSkillKey x, IScheduleForecastSkillKey y)
		{
			if (x == null || y == null)
				return false;

			return x.StartDateTime.Equals(y.StartDateTime) && x.IntervalId == y.IntervalId &&
					 x.SkillCode.Equals(y.SkillCode) && x.ScenarioCode.Equals(y.ScenarioCode);
		}

		public int GetHashCode(IScheduleForecastSkillKey obj)
		{
			int result = obj.StartDateTime.GetHashCode();
			result = (result * 397) ^ obj.IntervalId;
			result = (result * 397) ^ obj.SkillCode.GetHashCode();
			result = (result * 397) ^ obj.ScenarioCode.GetHashCode();
			return result;
		}
	}
}