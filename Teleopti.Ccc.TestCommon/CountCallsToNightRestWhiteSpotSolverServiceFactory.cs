using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.TestCommon
{
	public class CountCallsToNightRestWhiteSpotSolverServiceFactory : INightRestWhiteSpotSolverServiceFactory
	{
		public int NumberOfNightRestWhiteSpotServiceCalls { get; private set; }

		public INightRestWhiteSpotSolverService Create(bool considerShortBreaks)
		{
			return new countNightRestWhiteSpotSolverServiceCalls(this);
		}

		private class countNightRestWhiteSpotSolverServiceCalls : INightRestWhiteSpotSolverService
		{
			private readonly CountCallsToNightRestWhiteSpotSolverServiceFactory _countCallsToNightRestWhiteSpotSolverServiceFactory;

			public countNightRestWhiteSpotSolverServiceCalls(CountCallsToNightRestWhiteSpotSolverServiceFactory countCallsToNightRestWhiteSpotSolverServiceFactory)
			{
				_countCallsToNightRestWhiteSpotSolverServiceFactory = countCallsToNightRestWhiteSpotSolverServiceFactory;
			}

			public bool Resolve(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
			{
				_countCallsToNightRestWhiteSpotSolverServiceFactory.NumberOfNightRestWhiteSpotServiceCalls++;
				return true;
			}
		}
	}
}