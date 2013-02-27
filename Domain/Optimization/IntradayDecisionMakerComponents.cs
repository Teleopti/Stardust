using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayDecisionMakerComponents
	{
		IScheduleMatrixLockableBitArrayConverter MatrixConverter { get; }
		IScheduleResultDataExtractor DataExtractor { get; }
	}

	public class IntradayDecisionMakerComponents : IIntradayDecisionMakerComponents
	{
		public IScheduleMatrixLockableBitArrayConverter MatrixConverter { get; private set; }
		public IScheduleResultDataExtractor DataExtractor { get; private set; }

		public IntradayDecisionMakerComponents(IScheduleMatrixLockableBitArrayConverter matrixConverter, IScheduleResultDataExtractor dataExtractor)
		{
			MatrixConverter = matrixConverter;
			DataExtractor = dataExtractor;
		}
	}
}