using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	/// <summary>
	/// Intraday optimization container, which contatins a logic to try to do one move on one matrix
	/// - Checks for old and new period value.
	/// - Reschedule moved days.
	/// - Checks for white spots.
	/// - Does rollback for the moved days if move is not successful. 
	/// - Manages temporary locks to unsuccessfull days
	/// </summary>
	public class IntradayOptimizer2 : IIntradayOptimizer2
	{
		private readonly IScheduleResultDataExtractor _personalSkillsDataExtractor;
		private readonly IIntradayDecisionMaker _decisionMaker;
		private readonly IScheduleMatrixPro _matrix;
		private readonly IntradayOptimizeOneday _optimizeOneday;

		public IntradayOptimizer2(
						IScheduleResultDataExtractor personalSkillsDataExtractor,
						IIntradayDecisionMaker decisionMaker,
						IScheduleMatrixPro matrix,
						IntradayOptimizeOneday optimizeOneday)
		{
			_personalSkillsDataExtractor = personalSkillsDataExtractor;
			_decisionMaker = decisionMaker;
			_matrix = matrix;
			_optimizeOneday = optimizeOneday;
		}

		public bool Execute()
		{
			var dayToBeMoved = _decisionMaker.Execute(_matrix, _personalSkillsDataExtractor);

			return dayToBeMoved.HasValue && _optimizeOneday.Execute(dayToBeMoved.Value);
		}

		public IPerson ContainerOwner
		{
			get { return _matrix.Person; }
		}
	}
}
