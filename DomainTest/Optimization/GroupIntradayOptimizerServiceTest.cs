using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupIntradayOptimizerServiceTest
	{
		private MockRepository _mock;
		private IGroupIntradayOptimizerService _target;
		private IGroupIntradayOptimizer _optimizer;
		private IList<IGroupIntradayOptimizer> _optimizers;
		private IGroupOptimizerFindMatrixesForGroup _groupOptimizerFindMatrixesForGroup;
		private IGroupIntradayOptimizerExecuter _groupIntradayOptimizerExecuter;
		private IScheduleMatrixPro _matrix;
		private IList<IScheduleMatrixPro> _allMatrixes;
		private IPerson _person;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private bool _eventExecuted;
		private IOptimizationOverLimitByRestrictionDecider _optimizationOverLimitByRestrictionDecider;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_optimizer = _mock.StrictMock<IGroupIntradayOptimizer>();
			_optimizers = new List<IGroupIntradayOptimizer>{_optimizer};
			_groupOptimizerFindMatrixesForGroup = _mock.StrictMock<IGroupOptimizerFindMatrixesForGroup>();
			_groupIntradayOptimizerExecuter = _mock.StrictMock<IGroupIntradayOptimizerExecuter>();
			_target = new GroupIntradayOptimizerService(_optimizers, _groupOptimizerFindMatrixesForGroup,
			                                            _groupIntradayOptimizerExecuter);
			_matrix = _mock.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro>{_matrix};
			_person = PersonFactory.CreatePerson();
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_optimizationOverLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();

		}

		[Test]
		public void ShouldRunUntilAllOptimizersFailsInSchedulingStep()
		{
			DateOnly date = new DateOnly(2012, 1, 1);
			using (_mock.Record())
			{
				Expect.Call(_optimizer.Execute()).Return(date);
				Expect.Call(_optimizer.Person).Return(_person);
				Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date)).Return(_allMatrixes);
				Expect.Call(_matrix.Person).Return(_person);
				Expect.Call(_optimizer.IsMatrixForDateAndPerson(date, _person)).Return(true);
				Expect.Call(_optimizer.Matrix).Return(_matrix);
				Expect.Call(_matrix.GetScheduleDayByKey(date)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay);
				Expect.Call(() => _optimizer.LockDate(date));
				Expect.Call(_optimizer.OptimizationOverLimitByRestrictionDecider).Return(_optimizationOverLimitByRestrictionDecider);
                Expect.Call(_optimizer.PeriodValue(new DateOnly())).IgnoreArguments().Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_groupIntradayOptimizerExecuter.Execute(new List<IScheduleDay> {_scheduleDay},
				                                                    new List<IScheduleDay> {_scheduleDay}, _allMatrixes,
				                                                    _optimizationOverLimitByRestrictionDecider)).Return(
				                                                    	false);
			}

			using (_mock.Playback())
			{
				_target.Execute(_allMatrixes);
			}
		}

		[Test]
		public void ShouldRunUntilAllOptimizersFailsInFindDateStep()
		{
			//DateOnly date = new DateOnly(2012, 1, 1);
			using (_mock.Record())
			{
				Expect.Call(_optimizer.Execute()).Return(null);
				Expect.Call(_optimizer.Person).Return(_person);
			}

			using (_mock.Playback())
			{
				_target.Execute(_allMatrixes);
			}
		}

		[Test]
		public void VerifyReportProgressEventExecutedAndCanCancel()
		{
			DateOnly date = new DateOnly(2012, 1, 1);
			_optimizers = new List<IGroupIntradayOptimizer> { _optimizer, _optimizer };
			_target = new GroupIntradayOptimizerService(_optimizers, _groupOptimizerFindMatrixesForGroup,
														_groupIntradayOptimizerExecuter);
			_target.ReportProgress += _target_ReportProgress;
			using (_mock.Record())
			{
				Expect.Call(_optimizer.Person).Return(_person).Repeat.Twice();
				Expect.Call(_optimizer.Execute()).Return(date);
				Expect.Call(_groupOptimizerFindMatrixesForGroup.Find(_person, date)).Return(_allMatrixes);
				Expect.Call(_matrix.Person).Return(_person).Repeat.Twice();
				Expect.Call(_optimizer.IsMatrixForDateAndPerson(date, _person)).Return(true).Repeat.Twice();
				Expect.Call(_optimizer.Matrix).Return(_matrix).Repeat.Twice();
				Expect.Call(_matrix.GetScheduleDayByKey(date)).Return(_scheduleDayPro).Repeat.Twice();
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.Twice();
				Expect.Call(_scheduleDay.Clone()).Return(_scheduleDay).Repeat.Twice();
				Expect.Call(_optimizer.OptimizationOverLimitByRestrictionDecider).Return(_optimizationOverLimitByRestrictionDecider);
                Expect.Call(_optimizer.PeriodValue(new DateOnly())).IgnoreArguments().Return(0.5).Repeat.AtLeastOnce();
				Expect.Call(_groupIntradayOptimizerExecuter.Execute(new List<IScheduleDay> {_scheduleDay},
				                                                    new List<IScheduleDay> {_scheduleDay}, _allMatrixes,
				                                                    _optimizationOverLimitByRestrictionDecider)).IgnoreArguments().
					Return(
						true);
				Expect.Call(() => _optimizer.LockDate(date)).Repeat.Times(2);
			}
			using (_mock.Playback())
			{
				_target.Execute(_allMatrixes);
				_target.ReportProgress -= _target_ReportProgress;
				Assert.IsTrue(_eventExecuted);
			}
		}


		void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			_eventExecuted = true;
			e.Cancel = true;
		}
	}
}