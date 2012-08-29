using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupIntradayOptimizerExecuterTest
	{
		private MockRepository _mock;
		private IGroupIntradayOptimizerExecuter _target;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IDeleteSchedulePartService _deleteService;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IOptimizationPreferences _optimizerPreferences;
		private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private IGroupMatrixHelper _groupMatrixHelper;
		private IGroupSchedulingService _groupSchedulingService;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IList<IScheduleDay> _daysToSave;
		private IList<IScheduleDay> _daysToDelete;
		private IList<IScheduleMatrixPro> _allMatrixes;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDay _scheduleDay;
		private ISchedulingOptions _schedulingOptions;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
		private IMainShift _mainShift;
		private IPersonAssignment _personAssignment;
		private IPerson _person;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulePartModifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_deleteService = _mock.StrictMock<IDeleteSchedulePartService>();
			_schedulingOptionsCreator = _mock.StrictMock<ISchedulingOptionsCreator>();
			_optimizerPreferences = new OptimizationPreferences();
			_mainShiftOptimizeActivitySpecificationSetter = _mock.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_groupMatrixHelper = _mock.StrictMock<IGroupMatrixHelper>();
			_groupSchedulingService = _mock.StrictMock<IGroupSchedulingService>();
			_groupPersonBuilderForOptimization = _mock.StrictMock<IGroupPersonBuilderForOptimization>();
			_target = new GroupIntradayOptimizerExecuter(_schedulePartModifyAndRollbackService, _deleteService,
			                                             _schedulingOptionsCreator, _optimizerPreferences,
			                                             _mainShiftOptimizeActivitySpecificationSetter, _groupMatrixHelper,
			                                             _groupSchedulingService, _groupPersonBuilderForOptimization);
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_daysToDelete = new List<IScheduleDay> { _scheduleDay };
			_daysToSave = new List<IScheduleDay> { _scheduleDay };
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_allMatrixes = new List<IScheduleMatrixPro> { _scheduleMatrixPro };
			_schedulingOptions = new SchedulingOptions();
			_dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
			_mainShift = _mock.StrictMock<IMainShift>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_person = new Person();

		}

		[Test]
		public void ShouldReturnFalseIfNotSuccess()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
				                                                    _schedulePartModifyAndRollbackService, _schedulingOptions,
				                                                    _groupPersonBuilderForOptimization, _allMatrixes)).Return(false);
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnTrueIfSuccess()
		{
			using (_mock.Record())
			{
				commonMocks();
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
				Expect.Call(_scheduleDay.AssignmentHighZOrder()).Return(_personAssignment);
				Expect.Call(_personAssignment.MainShift).Return(_mainShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetSpecification(_schedulingOptions, _optimizerPreferences, _mainShift, new DateOnly(2012, 1, 1)));
				Expect.Call(_scheduleDay.Person).Return(_person);
				Expect.Call(_groupMatrixHelper.ScheduleSinglePerson(new DateOnly(2012, 1, 1), _person, _groupSchedulingService,
																	_schedulePartModifyAndRollbackService, _schedulingOptions,
																	_groupPersonBuilderForOptimization, _allMatrixes)).Return(true);
			}

			bool result;

			using (_mock.Playback())
			{
				result = _target.Execute(_daysToDelete, _daysToSave, _allMatrixes);
			}

			Assert.IsTrue(result);
		}

		private void commonMocks()
		{
			Expect.Call(() =>_schedulePartModifyAndRollbackService.ClearModificationCollection());
			Expect.Call(() => _deleteService.Delete(_daysToDelete, _schedulePartModifyAndRollbackService));
			Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizerPreferences)).Return(_schedulingOptions);
		}

	}
}