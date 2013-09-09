using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizerContainer
    {
		bool Execute(ISchedulingOptions schedulingOptions);
        IPerson Owner { get; }
        IScheduleMatrixPro Matrix { get; }
        ILockableBitArray WorkingBitArray { get; }
    }

    public class BlockDayOffOptimizerContainer : IBlockDayOffOptimizerContainer
    {
        private readonly IList<IDayOffDecisionMaker> _decisionMakers;
        private readonly IScheduleMatrixPro _matrix;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IBlockDayOffOptimizer _blockDayOffOptimizer;

        public BlockDayOffOptimizerContainer(
            IEnumerable<IDayOffDecisionMaker> decisionMakers,
            IScheduleMatrixPro matrix,
            IScheduleMatrixOriginalStateContainer originalStateContainer,
            IBlockDayOffOptimizer blockDayOffOptimizer)
        {
            _decisionMakers = new List<IDayOffDecisionMaker>(decisionMakers);
            _matrix = matrix;
            _originalStateContainer = originalStateContainer;
            _blockDayOffOptimizer = blockDayOffOptimizer;
        }

        public bool Execute(ISchedulingOptions schedulingOptions)
        {
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {
                // note that we do not change the order of the decisionmakers
            	foreach (var dayOffDecisionMaker in _decisionMakers)
            	{
					if (runDecisionMaker(dayOffDecisionMaker, schedulingOptions))
						return true;
            	}
            }
            return false;
        }

		private bool runDecisionMaker(IDayOffDecisionMaker decisionMaker, ISchedulingOptions schedulingOptions)
        {
            bool dayOffOptimizerResult = _blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, decisionMaker, schedulingOptions);
            return dayOffOptimizerResult;
        }

        public IPerson Owner
        {
            get { return _matrix.Person; }
        }

        public IScheduleMatrixPro Matrix
        {
            get { return _matrix; }
        }

        public ILockableBitArray WorkingBitArray
        {
            get { return _blockDayOffOptimizer.WorkingBitArray;}
        }
    }
}
