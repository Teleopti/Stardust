using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public class IntradayOptimizer2 : IIntradayOptimizer2
	{
		private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
		private readonly IntradayDecisionMaker _decisionMaker;
		private readonly IScheduleMatrixPro _matrix;
		private readonly IIntradayOptimizeOneday _optimizeOneday;

		public IntradayOptimizer2(
						IScheduleResultDataExtractor personalSkillsDataExtractor,
						IntradayDecisionMaker decisionMaker,
						IScheduleMatrixPro matrix,
						IIntradayOptimizeOneday optimizeOneday)
		{
			_personalSkillsDataExtractor = personalSkillsDataExtractor;
			_decisionMaker = decisionMaker;
			_matrix = matrix;
			_optimizeOneday = optimizeOneday;
		}

		public DateOnly? Execute(IEnumerable<DateOnly> skipDates)
		{
			var dayToBeMoved = _decisionMaker.Execute(_matrix, _personalSkillsDataExtractor, skipDates);

			if (dayToBeMoved.HasValue && _optimizeOneday.Execute(dayToBeMoved.Value))
				return dayToBeMoved.Value;
			return null;
		}

		public IPerson ContainerOwner
		{
			get { return _matrix.Person; }
		}
	}
}
