﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class MoveTimeOptimizerCreatorTest
	{
		private MoveTimeOptimizerCreator _target;
		private MockRepository _mocks;

		private IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixOriginalStateContainerList;
		private IList<IScheduleMatrixOriginalStateContainer> _workShiftOriginalStateContainerList;
		private IMoveTimeDecisionMaker _decisionMaker;
		private IScheduleService _scheduleService;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		private IScheduleMatrixOriginalStateContainer _matrixContainer1;
		private IScheduleMatrixOriginalStateContainer _matrixContainer2;
		private IScheduleMatrixOriginalStateContainer _workShiftContainer1;
		private IScheduleMatrixOriginalStateContainer _workShiftContainer2;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IResourceCalculation _resourceOptimizationHelper;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
		private IPerson _person;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrixContainer1 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_matrixContainer2 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_workShiftContainer1 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_workShiftContainer2 = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
			_matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixOriginalStateContainerList = new List<IScheduleMatrixOriginalStateContainer> { _matrixContainer1, _matrixContainer2 };
			_workShiftOriginalStateContainerList = new List<IScheduleMatrixOriginalStateContainer> { _workShiftContainer1, _workShiftContainer2 };
			_decisionMaker = _mocks.StrictMock<IMoveTimeDecisionMaker>();
			_scheduleService = _mocks.StrictMock<IScheduleService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_resourceOptimizationHelper = _mocks.StrictMock<IResourceCalculation>();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences());
			_person = new Person();
			_scheduleDayPro1 = new ScheduleDayPro(new DateOnly(2015, 1, 1), _matrix1);
			_scheduleDayPro2 = new ScheduleDayPro(new DateOnly(2015, 1, 1), _matrix2);

			_target = new MoveTimeOptimizerCreator(
												   _decisionMaker,
												   _scheduleService,
												   () => _schedulingResultStateHolder,
												   _effectiveRestrictionCreator,
												   _resourceOptimizationHelper,
													 MockRepository.GenerateStub<IDeleteAndResourceCalculateService>(),
													 new ScheduleResultDataExtractorProvider(new PersonalSkillsProvider(), new SkillPriorityProvider(), UserTimeZone.Make()), UserTimeZone.Make());
		}

		[Test]
		public void VerifyCreateOneOptimizerForEachMatrix()
		{

			using (_mocks.Record())
			{
				Expect.Call(_matrixContainer1.ScheduleMatrix)
					.Return(_matrix1)
					.Repeat.AtLeastOnce();
				Expect.Call(_matrixContainer2.ScheduleMatrix)
					.Return(_matrix2)
					.Repeat.AtLeastOnce();
				Expect.Call(_schedulingResultStateHolder.Schedules)
					.Return(null)
					.Repeat.Any();

				Expect.Call(_matrix1.Person).Return(_person);
				Expect.Call(_matrix1.EffectivePeriodDays).Return(new []{_scheduleDayPro1});

				Expect.Call(_matrix2.Person).Return(_person);
				Expect.Call(_matrix2.EffectivePeriodDays).Return(new[] { _scheduleDayPro2 });
			}
			using (_mocks.Playback())
			{
				IList<IMoveTimeOptimizer> optimizers = _target.Create(_scheduleMatrixOriginalStateContainerList,
													 _workShiftOriginalStateContainerList, new OptimizationPreferences(), _dayOffOptimizationPreferenceProvider, _rollbackService);
				Assert.AreEqual(_scheduleMatrixOriginalStateContainerList.Count, optimizers.Count);
			}
		}
	}
}
