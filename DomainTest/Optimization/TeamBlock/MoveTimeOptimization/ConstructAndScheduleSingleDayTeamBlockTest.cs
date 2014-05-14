using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.MoveTimeOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.MoveTimeOptimization
{
	[TestFixture]
	public class ConstructAndScheduleSingleDayTeamBlockTest
	{
		private MockRepository _mock;
		private IConstructAndScheduleSingleDayTeamBlock _target;
		private ILockUnSelectedInTeamBlock _lockUnSelectedInTeamBlock;
		private ITeamBlockGenerator _teamBlockGenerator;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ITeamBlockRestrictionOverLimitValidator _teamBlockRestrictionOverLimitValidator;
		private IList<IScheduleMatrixPro> _matrixList;
		private IScheduleMatrixPro _matrix1;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IOptimizationPreferences _optimizationPreferences;
		private IPerson _person1;
		private ITeamBlockInfo _teamBlockInfo;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_lockUnSelectedInTeamBlock = _mock.StrictMock<ILockUnSelectedInTeamBlock>();
			_teamBlockGenerator = _mock.StrictMock<ITeamBlockGenerator>();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_teamBlockRestrictionOverLimitValidator = _mock.StrictMock<ITeamBlockRestrictionOverLimitValidator>();
			_target = new ConstructAndScheduleSingleDayTeamBlock(_lockUnSelectedInTeamBlock, _teamBlockGenerator,
				_teamBlockScheduler, _teamBlockRestrictionOverLimitValidator);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro>{_matrix1};
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_optimizationPreferences = new OptimizationPreferences();
			_person1 = PersonFactory.CreatePerson("test");
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
		}

		[Test]
		public void ShouldScheduleSuccessfully()
		{
			var dayDate = new DateOnly(2014,05,14);
			IList<ITeamBlockInfo> listOfTeamBlock = new List<ITeamBlockInfo>{_teamBlockInfo};
			using (_mock.Record())
			{
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice();
				Expect.Call(_teamBlockGenerator.Generate(_matrixList, new DateOnlyPeriod(dayDate, dayDate),
					new List<IPerson> {_person1}, _schedulingOptions)).Return(listOfTeamBlock);
				Expect.Call(
					() =>
						_lockUnSelectedInTeamBlock.Lock(_teamBlockInfo, new List<IPerson> {_person1}, new DateOnlyPeriod(dayDate, dayDate)));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, dayDate, _schedulingOptions, _rollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo, _optimizationPreferences)).Return(true);
			}

			using (_mock.Playback())
			{
				
				_target.Schedule(_matrixList, dayDate, _matrix1, _schedulingOptions, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNotValidated()
		{
			var dayDate = new DateOnly(2014, 05, 14);
			IList<ITeamBlockInfo> listOfTeamBlock = new List<ITeamBlockInfo> { _teamBlockInfo };
			using (_mock.Record())
			{
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice();
				Expect.Call(_teamBlockGenerator.Generate(_matrixList, new DateOnlyPeriod(dayDate, dayDate),
					new List<IPerson> { _person1 }, _schedulingOptions)).Return(listOfTeamBlock);
				Expect.Call(
					() =>
						_lockUnSelectedInTeamBlock.Lock(_teamBlockInfo, new List<IPerson> { _person1 }, new DateOnlyPeriod(dayDate, dayDate)));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, dayDate, _schedulingOptions, _rollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(true);
				Expect.Call(_teamBlockRestrictionOverLimitValidator.Validate(_teamBlockInfo, _optimizationPreferences)).Return(false);
			}

			using (_mock.Playback())
			{

				_target.Schedule(_matrixList, dayDate, _matrix1, _schedulingOptions, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _optimizationPreferences);
			}
		}

		[Test]
		public void ShouldReturnFalseIfNotScheduled()
		{
			var dayDate = new DateOnly(2014, 05, 14);
			IList<ITeamBlockInfo> listOfTeamBlock = new List<ITeamBlockInfo> { _teamBlockInfo };
			using (_mock.Record())
			{
				Expect.Call(_matrix1.Person).Return(_person1).Repeat.Twice();
				Expect.Call(_teamBlockGenerator.Generate(_matrixList, new DateOnlyPeriod(dayDate, dayDate),
					new List<IPerson> { _person1 }, _schedulingOptions)).Return(listOfTeamBlock);
				Expect.Call(
					() =>
						_lockUnSelectedInTeamBlock.Lock(_teamBlockInfo, new List<IPerson> { _person1 }, new DateOnlyPeriod(dayDate, dayDate)));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, dayDate, _schedulingOptions, _rollbackService,
					_resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).IgnoreArguments().Return(false);
			}

			using (_mock.Playback())
			{

				_target.Schedule(_matrixList, dayDate, _matrix1, _schedulingOptions, _rollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _optimizationPreferences);
			}
		}
	}
}
