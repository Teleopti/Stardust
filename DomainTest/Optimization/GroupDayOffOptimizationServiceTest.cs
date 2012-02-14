using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupDayOffOptimizationServiceTest
    {
        private IGroupDayOffOptimizationService _target;
        private MockRepository _mocks;
        private IPeriodValueCalculator _periodValueCalculator;
        private IEnumerable<IGroupDayOffOptimizerContainer> _optimizers;
        private IGroupDayOffOptimizerContainer _container1;
        private IGroupDayOffOptimizerContainer _container2;
        private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private IResourceOptimizationHelper _resourceOptimizationHelper;
        private IScheduleDay _scheduleDay;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _periodValueCalculator = _mocks.StrictMock<IPeriodValueCalculator>();
            _container1 = _mocks.StrictMock<IGroupDayOffOptimizerContainer>();
            _container2 = _mocks.StrictMock<IGroupDayOffOptimizerContainer>();
            _schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _resourceOptimizationHelper = _mocks.StrictMock<IResourceOptimizationHelper>();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            _target = new GroupDayOffOptimizationService(_periodValueCalculator, _schedulePartModifyAndRollbackService, _resourceOptimizationHelper);
        }

        [Test]
        public void VerifySuccessfulOptimization()
        {
            //_target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IGroupDayOffOptimizerContainer> { _container1, _container2 };
            IPerson owner = PersonFactory.CreatePerson();
            
            using (_mocks.Record())
            {
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(8);
                // first round 
                Expect.Call(_container1.Execute())
                    .Return(true);
                Expect.Call(_container2.Execute())
                    .Return(true);
                Expect.Call(_container1.Execute())
                    .Return(false);
                Expect.Call(_container2.Execute())
                    .Return(false);

                // second round 
                Expect.Call(_container1.Execute())
                    .Return(true);
                Expect.Call(_container2.Execute())
                    .Return(true);
                Expect.Call(_container1.Execute())
                    .Return(false);
                Expect.Call(_container2.Execute())
                    .Return(false);

                Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(
                new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _scheduleDay })).Repeat.Times(4);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod).Repeat.Times(4);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2011, 1, 1)).Repeat.Times(4);
                Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback()).Repeat.Times(4);
                Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), false, false)).Repeat.Times(4);
                
                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(_container2.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(4);
            }

            using (_mocks.Playback())
            {
                _target.Execute(_optimizers);
            }
            //_target.ReportProgress -= _target_ReportProgress;
        }

        [Test]
        public void VerifyCancel()
        {
            _target.ReportProgress += _target_ReportProgress;
            _optimizers = new List<IGroupDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();

            using (_mocks.Record())
            {
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
                // only one round 
                Expect.Call(_container1.Execute())
                    .Return(true);

                Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization))
                    .Return(10).Repeat.AtLeastOnce();
                Expect.Call(_container1.Owner)
                    .Return(owner).Repeat.AtLeastOnce();
                Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection());
            }

            using (_mocks.Playback())
            {
                _target.Execute(_optimizers);
            }
            _target.ReportProgress -= _target_ReportProgress;
        }

        [Test]
        public void ShouldRollbackAndAddToListIfPeriodValueHigher()
        {
            _optimizers = new List<IGroupDayOffOptimizerContainer> { _container1 };
            IPerson owner = PersonFactory.CreatePerson();
            Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(2);
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
            Expect.Call(_container1.Execute()).Return(true);
            //worse
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(11);
            Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(
                new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> {_scheduleDay}));
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2011, 1, 1));
            Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), false, false));

            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(10);
            Expect.Call(_container1.Execute()).Return(true);
            //worse
            Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.DayOffOptimization)).Return(11);
            Expect.Call(_schedulePartModifyAndRollbackService.ModificationCollection).Return(
                new ReadOnlyCollection<IScheduleDay>(new List<IScheduleDay> { _scheduleDay }));
            Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
            Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2011, 1, 1));
            Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
            Expect.Call(() => _resourceOptimizationHelper.ResourceCalculateDate(new DateOnly(2011, 1, 1), false, false));


            Expect.Call(_container1.Owner).Return(owner).Repeat.AtLeastOnce();
            Expect.Call(() => _schedulePartModifyAndRollbackService.ClearModificationCollection()).Repeat.Times(2);
            _mocks.ReplayAll();

            _target.Execute(_optimizers);
            _mocks.VerifyAll();
        }

        static void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            e.Cancel = true;
        }

    }
}