using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class BlockDayOffOptimizationServiceTest
    {
        private BlockDayOffOptimizationService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IEnumerable<IBlockDayOffOptimizerContainer> _optimizers;
        private IBlockDayOffOptimizerContainer _container1;
        private IBlockDayOffOptimizerContainer _container2;
        private ISchedulePartModifyAndRollbackService _rollbackService;
    	private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IBlockDayOffOptimizerContainer>();
            _container2 = _mocks.StrictMock<IBlockDayOffOptimizerContainer>();
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingOptions = new SchedulingOptions();
            _target = new BlockDayOffOptimizationService(_periodValueCalculator, _rollbackService);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
        public void VerifySuccessfulOptimization()
        {
            //_target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IBlockDayOffOptimizerContainer> { _container1, _container2 };
            IPerson owner = PersonFactory.CreatePerson();
            
            using (_mocks.Record())
            {
                // first round
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());

                // second round
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(true);
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container2.Execute(_schedulingOptions))
                    .Return(false);
                Expect.Call(() => _rollbackService.Rollback());


                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(_container2.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
				_target.Execute(_optimizers, _schedulingOptions);
            }
            //_target.ReportProgress -= _target_ReportProgress;
        }

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IBlockDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();

            using (_mocks.Record())
            {
                // only one round 
                Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_container1.Execute(_schedulingOptions))
                    .Return(true);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
				_target.Execute(_optimizers, _schedulingOptions);
            }
            _target.ReportProgress -= _target_ReportProgress;
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

    }
}