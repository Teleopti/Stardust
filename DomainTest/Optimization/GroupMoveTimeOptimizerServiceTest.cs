using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture ]
    public class GroupMoveTimeOptimizerServiceTest
    {
        private MockRepository _mock;
        private IGroupMoveTimeOptimizerService _target;
        private IGroupMoveTimeOptimizer _optimizer;
        private IList<IGroupMoveTimeOptimizer> _optimizers;
        private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
        private IGroupMoveTimeOptimizationExecuter _groupMoveTimeOptimizerExecuter;
        private IScheduleMatrixPro _matrix;
        private IList<IScheduleMatrixPro> _allMatrixes;
        private IPerson _person;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private bool _eventExecuted;
        private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;
        private IScheduleMatrixPro _matrix2;
        private IScheduleDayPro _scheduleDayPro2;
        private IGroupMoveTimeValidatorRunner _groupMoveTimeValidatorRunner;
        private ISchedulingOptions _schedulingOptions;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _optimizer = _mock.StrictMock<IGroupMoveTimeOptimizer>();
            _optimizers = new List<IGroupMoveTimeOptimizer> { _optimizer };
            _groupOptimizerFindMatrixesForGroup = _mock.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
            _groupMoveTimeOptimizerExecuter = _mock.DynamicMock<IGroupMoveTimeOptimizationExecuter>();
            _groupMoveTimeValidatorRunner = _mock.Stub<IGroupMoveTimeValidatorRunner>();
            _target = new GroupMoveTimeOptimizerService(_optimizers, _groupOptimizerFindMatrixesForGroup,
                                                        _groupMoveTimeOptimizerExecuter, _groupMoveTimeValidatorRunner);
            _schedulingOptions = _mock.StrictMock<ISchedulingOptions>();
            _matrix = _mock.DynamicMock<IScheduleMatrixPro>();
            _matrix2 = _mock.DynamicMock<IScheduleMatrixPro>();
            _allMatrixes = new List<IScheduleMatrixPro> { _matrix, _matrix2 };
            _person = PersonFactory.CreatePerson();
            _scheduleDayPro = _mock.DynamicMock<IScheduleDayPro>();
            _scheduleDayPro2 = _mock.DynamicMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
        }

        [Test]
        public void ShouldRunUntilAllOptimizersFailsInSchedulingStep()
        {
            var date = new DateOnly(2012, 1, 1);
            var date2 = new DateOnly(2012, 1, 2);

            _groupMoveTimeValidatorRunner.Stub(x => x.Run(_person, new List<DateOnly>(), true,_allMatrixes)).IgnoreArguments() .Return(new ValidatorResult(){Success = true});
            
            using (_mock.Record())
            {
                Expect.Call(_optimizer.Execute()).Return(new List<DateOnly> { date, date2 });
                Expect.Call(_optimizer.Execute()).Return(new List<DateOnly>());
                Expect.Call(_optimizer.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date)).Return(_allMatrixes).Repeat.AtLeastOnce();
                Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date2)).Return(_allMatrixes).Repeat.AtLeastOnce() ;
                Expect.Call(_matrix.Person).Return(_person).Repeat.AtLeastOnce() ;
                Expect.Call(_matrix2.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.IsMatrixForDateAndPerson(date, _person)).Return(true).Repeat.AtLeastOnce() ;
                Expect.Call(_optimizer.IsMatrixForDateAndPerson(date2, _person)).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.Matrix).Return(_matrix).Repeat.AtLeastOnce() ;
                Expect.Call(_matrix.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
                Expect.Call(_matrix.GetScheduleDayByKey(date2)).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.PeriodValue()).Return(2).Repeat.AtLeastOnce();
                Expect.Call(() => _optimizer.LockDate(date2)).Repeat.AtLeastOnce();
                Expect.Call(() => _optimizer.LockDate(date)).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.OptimizationOverLimitByRestrictionDecider).Return(_optimizationOverLimitByRestrictionDecider).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, new CccTimeZoneInfo() )).Repeat.AtLeastOnce();
                Expect.Call(_groupMoveTimeOptimizerExecuter.SchedulingOptions).Return(_schedulingOptions);
                Expect.Call(_schedulingOptions.UseSameDayOffs).Return(true).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                _target.Execute(_allMatrixes);
            }
        }

        [Test]
        public void ShouldRunUntilAllOptimizersFailsInFindDateStep()
        {
            using (_mock.Record())
            {
                Expect.Call(_optimizer.Execute()).Return(new List<DateOnly>( ));
                Expect.Call(_optimizer.Person).Return(_person);
            }

            using (_mock.Playback())
            {
                _target.Execute(_allMatrixes);
            }
        }

        [Test]
        public void ShouldBreakUntilWithCancelOptimizer()
        {
            using (_mock.Record())
            {
                Expect.Call(_optimizer.Execute()).Return(new List<DateOnly>( )).Repeat.AtLeastOnce() ;
                Expect.Call(_optimizer.Person).Return(_person).Repeat.AtLeastOnce() ;
            }
            _target = new GroupMoveTimeOptimizerService(new List<IGroupMoveTimeOptimizer> { _optimizer, _optimizer }, _groupOptimizerFindMatrixesForGroup,
                                                        _groupMoveTimeOptimizerExecuter, _groupMoveTimeValidatorRunner);
            using (_mock.Playback())
            {
                _target.Execute(_allMatrixes);
            }
        }

        [Test]
        public void VerifyReportProgressEventExecutedAndCanCancel()
        {
            var date = new DateOnly(2012, 1, 1);
            var date2 = new DateOnly(2012, 1, 2);
            _target.ReportProgress += targetReportProgress;
            _groupMoveTimeValidatorRunner.Stub(x => x.Run(_person, new List<DateOnly>(), true, _allMatrixes)).IgnoreArguments().Return(new ValidatorResult() { Success = true });
            
            using (_mock.Record())
            {
                verifyReportProgressEventExecutedAndCanCancelExpectValues(date2, date);
                Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay).Repeat.AtLeastOnce();
                Expect.Call(() => _optimizer.LockDate(date2)).Repeat.AtLeastOnce();
                Expect.Call(() => _optimizer.LockDate(date)).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.OptimizationOverLimitByRestrictionDecider).Return(_optimizationOverLimitByRestrictionDecider);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, new CccTimeZoneInfo())).Repeat.AtLeastOnce();
                Expect.Call(_optimizer.PeriodValue()).Return(2).Repeat.AtLeastOnce();
                Expect.Call(_groupMoveTimeOptimizerExecuter.SchedulingOptions).Return(_schedulingOptions);
                Expect.Call(_schedulingOptions.UseSameDayOffs).Return(true).Repeat.AtLeastOnce();
            }
            using (_mock.Playback())
            {
                _target.Execute(_allMatrixes);
                _target.ReportProgress -= targetReportProgress;
                Assert.IsTrue(_eventExecuted);
            }
        }

        private void verifyReportProgressEventExecutedAndCanCancelExpectValues(DateOnly date2, DateOnly date)
        {
            Expect.Call(_optimizer.Execute()).Return(new List<DateOnly> {date, date2});
            Expect.Call(_optimizer.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date)).Return(_allMatrixes).Repeat.AtLeastOnce();
            Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date2)).Return(_allMatrixes).Repeat.AtLeastOnce();
            Expect.Call(_matrix.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_matrix2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_optimizer.IsMatrixForDateAndPerson(date, _person)).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_optimizer.IsMatrixForDateAndPerson(date2, _person)).Return(true).Repeat.AtLeastOnce();
            Expect.Call(_optimizer.Matrix).Return(_matrix).Repeat.AtLeastOnce();
            Expect.Call(_matrix.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.AtLeastOnce();
            Expect.Call(_matrix.GetScheduleDayByKey(date2)).Return(_scheduleDayPro2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
        }

        void targetReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
        {
            _eventExecuted = true;
            e.Cancel = true;
        }
    }
}
