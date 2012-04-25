using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockDayOffOptimizerContainer
    {
        bool Execute();
        IPerson Owner { get; }
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

        public bool Execute()
        {
            string agent =
                _matrix.Person.Name.ToString(NameOrderOption.FirstNameLastName);

            using (PerformanceOutput.ForOperation("Day off optimization for " + agent))
            {
                // note that we do not change the order of the decisionmakers
                if (_decisionMakers.Any(runDecisionMaker))
                    return true;
            }
            return false;
        }

        private bool runDecisionMaker(IDayOffDecisionMaker decisionMaker)
        {
            bool dayOffOptimizerResult = _blockDayOffOptimizer.Execute(_matrix, _originalStateContainer, decisionMaker);
            return dayOffOptimizerResult;
        }

        public IPerson Owner
        {
            get { return _matrix.Person; }
        }

    }
}
