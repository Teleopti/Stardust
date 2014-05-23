using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class TeamBlockMoveTimeBetweenDaysServiceTest
	{
		private ITeamBlockMoveTimeOptimizer _teamBlockMoveTimeOptimizer;
		private ITeamBlockMoveTimeBetweenDaysService _target;
		private MockRepository _mock;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IPeriodValueCalculator _periodValueCalculator;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPerson _person1;
		private bool _cancel;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamBlockMoveTimeOptimizer = _mock.StrictMock<ITeamBlockMoveTimeOptimizer>();
			
			_optimizationPreferences = new OptimizationPreferences();
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_periodValueCalculator = _mock.StrictMock<IPeriodValueCalculator>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_person1 = PersonFactory.CreatePerson("test");
		}

		[Test]
		public void ShouldExecuteService()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro>{_matrix1,_matrix2};
			IList<IPerson> selectedPersons = new List<IPerson> {_person1};
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			using (_mock.Record())
			{
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice() ;
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeMatrix(_optimizationPreferences, matrixList, _rollbackService,
					_periodValueCalculator, _schedulingResultStateHolder, _matrix1)).Return(true);
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice() ;
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeMatrix(_optimizationPreferences, matrixList, _rollbackService,
					_periodValueCalculator, _schedulingResultStateHolder, _matrix1)).Return(false);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization))
					.Return(0.815689)
					.Repeat.Times(2);

			}
			using (_mock.Playback())
			{
				
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons, matrixesOnSelectedperiod);
			}
		}

		[Test]
		public void ShouldNotExecuteIfCanceled()
		{
			_target = new TeamBlockMoveTimeBetweenDaysService(_teamBlockMoveTimeOptimizer);
			_cancel = false;
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _matrix1, _matrix2 };
			IList<IPerson> selectedPersons = new List<IPerson> { _person1 };
			IList<IScheduleMatrixPro> matrixesOnSelectedperiod = new List<IScheduleMatrixPro> { _matrix1 };
			using (_mock.Record())
			{
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice();
				Expect.Call(_teamBlockMoveTimeOptimizer.OptimizeMatrix(_optimizationPreferences, matrixList, _rollbackService,
					_periodValueCalculator, _schedulingResultStateHolder, _matrix1)).Return(true);
				Expect.Call(_periodValueCalculator.PeriodValue(IterationOperationOption.WorkShiftOptimization)).Return(0.815689);

			}
			using (_mock.Playback())
			{
				_target.ReportProgress += testCancel;
				_cancel = true;
				_target.Execute(_optimizationPreferences, matrixList, _rollbackService, _periodValueCalculator,
					_schedulingResultStateHolder, selectedPersons, matrixesOnSelectedperiod);
				_target.ReportProgress -= testCancel;
			}
		}

		private void testCancel(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = _cancel;
		}
	}
}
