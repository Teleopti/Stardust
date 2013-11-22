using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class SwapRawServiceTest
	{
		private ISwapRawService _swapRawService;
		private MockRepository _mockRepository;
		private IScheduleDictionary _scheduleDictionary;
		private IList<IScheduleDay> _selectionOne;
		private IList<IScheduleDay> _selectionTwo;
		private IScheduleDay _scheduleDayOnePersonOne;
		private IScheduleDay _scheduleDayTwoPersonOne;
		private IScheduleDay _scheduleDayThreePersonOne;
		private IScheduleDay _scheduleDayOnePersonTwo;
		private IScheduleDay _scheduleDayTwoPersonTwo;
		private IScheduleDay _scheduleDayThreePersonTwo;
		private IPerson _personOne;
		private IPerson _personTwo;
		private IScenario _scenario;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IDictionary<IPerson, IList<DateOnly>> _locks;
	    private IPrincipalAuthorization _authorizationService;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
            _authorizationService = _mockRepository.StrictMock<IPrincipalAuthorization>();
			_swapRawService = new SwapRawService(_authorizationService);
			_scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
			_personOne = PersonFactory.CreatePerson("personOne");
			_personTwo = PersonFactory.CreatePerson("personTwo");
			_scenario = new Scenario("scenario1");
			_schedulePartModifyAndRollbackService = _mockRepository.StrictMock<ISchedulePartModifyAndRollbackService>();
			_locks = new Dictionary<IPerson, IList<DateOnly>>();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenRollbackServiceIsNull()
		{
			_swapRawService.Swap(null, _selectionOne, _selectionTwo, _locks);	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenSelectionOneIsNull()
		{
			_swapRawService.Swap(_schedulePartModifyAndRollbackService, null, _selectionTwo, _locks);	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenSelectionTwoIsNull()
		{
			_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, null, _locks);	
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenLocksIsNull()
		{
			_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ShouldThrowExceptionWhenSelectionsNotOfEqualSize()
		{
			using (_mockRepository.Record())
			{
				Expect.Call(_scheduleDictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));
				_scheduleDayOnePersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 1));

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne, _scheduleDayTwoPersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayOnePersonTwo };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);
			}
		}

        [Test, ExpectedException(typeof(PermissionException))]
        public void ShouldThrowExceptionWhenNoPermission()
        {
            using(_mockRepository.Record())
            {
                Expect.Call(_scheduleDictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
                Expect.Call(_authorizationService.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, new DateOnly(2011, 1, 1), _personOne)).Return(false);
            }

            using(_mockRepository.Playback())
            {
                _scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
                _scheduleDayOnePersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 1));

                _selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne};
                _selectionTwo = new List<IScheduleDay> { _scheduleDayOnePersonTwo};

                _swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSwapMainShiftAndDayOff()
		{
			using (_mockRepository.Record())
			{
                commonMocks();
			}

			using (_mockRepository.Playback())
			{
				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));
				_scheduleDayThreePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 3));
				_scheduleDayOnePersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 3));
				_scheduleDayTwoPersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 4));
				_scheduleDayThreePersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 5));

				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod2 = new DateTimePeriod(new DateTime(2011, 1, 2, 9, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 2, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod3 = new DateTimePeriod(new DateTime(2011, 1, 3, 10, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 3, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod4 = new DateTimePeriod(new DateTime(2011, 1, 4, 11, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 4, 17, 0, 0, DateTimeKind.Utc));

				_scheduleDayOnePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1));
				_scheduleDayTwoPersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod2));
				_scheduleDayOnePersonTwo.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personTwo, dateTimePeriod3));
				_scheduleDayTwoPersonTwo.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personTwo, dateTimePeriod4));
				_scheduleDayThreePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_scenario, _personOne, 
																					  new DateOnly(dateTimePeriod3.StartDateTime),
																					  TimeSpan.FromHours(24), TimeSpan.FromHours(0),
																					  TimeSpan.FromHours(12)));

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne, _scheduleDayTwoPersonOne, _scheduleDayThreePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayOnePersonTwo, _scheduleDayTwoPersonTwo, _scheduleDayThreePersonTwo };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				Assert.AreEqual(new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc), _scheduleDayOnePersonOne.PersonAssignment().Period.StartDateTime);
				Assert.AreEqual(new DateTime(2011, 1, 2, 11, 0, 0, DateTimeKind.Utc), _scheduleDayTwoPersonOne.PersonAssignment().Period.StartDateTime);
				Assert.AreEqual(new DateTime(2011, 1, 3, 8, 0, 0, DateTimeKind.Utc), _scheduleDayOnePersonTwo.PersonAssignment().Period.StartDateTime);
				Assert.AreEqual(new DateTime(2011, 1, 4, 9, 0, 0, DateTimeKind.Utc), _scheduleDayTwoPersonTwo.PersonAssignment().Period.StartDateTime);
				Assert.AreEqual(1, _scheduleDayThreePersonOne.PersistableScheduleDataCollection().Count());
				Assert.IsNotNull(_scheduleDayThreePersonTwo.PersonAssignment().DayOff());
			}
		}

		private void commonMocks()
		{
			Expect.Call(_authorizationService.IsPermitted(null, new DateOnly(2011, 1, 1), _personOne))
			      .IgnoreArguments()
			      .Return(true)
			      .Repeat.AtLeastOnce();
			Expect.Call(_scheduleDictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
			Expect.Call(() => _schedulePartModifyAndRollbackService.Modify(_scheduleDayOnePersonOne))
			      .IgnoreArguments()
			      .Repeat.AtLeastOnce();
		}

		[Test]
		public void ShouldSwapUnderLayingMainShiftWhenFullDayAbsence()
		{
			using (_mockRepository.Record())
			{
                commonMocks();
			}

			using (_mockRepository.Playback())
			{
				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));

				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));

				_scheduleDayOnePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1));
				_scheduleDayOnePersonOne.CreateAndAddAbsence(new AbsenceLayer(AbsenceFactory.CreateAbsence("absence"), dateTimePeriod1));

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayTwoPersonOne };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				_scheduleDayOnePersonOne.PersonAssignment().Should().Not.Be.Null();
				_scheduleDayTwoPersonOne.PersonAssignment().Should().Not.Be.Null();
				Assert.AreEqual(0, _scheduleDayOnePersonOne.PersonAbsenceCollection().Count);
				Assert.AreEqual(0, _scheduleDayTwoPersonOne.PersonAbsenceCollection().Count);
			}
		}

		[Test]
		public void ShouldSwapEmptyDay()
		{
			using (_mockRepository.Record())
			{
				commonMocks();
			}

			using (_mockRepository.Playback())
			{
				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));

				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));

				_scheduleDayOnePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1));

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayTwoPersonOne };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				Assert.AreEqual(0, _scheduleDayOnePersonOne.PersonAssignment().MainLayers().Count());
				Assert.AreEqual(1, _scheduleDayTwoPersonOne.PersonAssignment().MainLayers().Count());
			}
		}

		[Test]
		public void ShouldNotSwapPersonalShiftInSwap()
		{
			using (_mockRepository.Record())
			{
				commonMocks();
			}

			using (_mockRepository.Playback())
			{
				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod2 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc));

				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));

				_scheduleDayOnePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1));
				_scheduleDayOnePersonOne.PersonAssignment().AddPersonalActivity(new Activity("activity"), dateTimePeriod2);

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayTwoPersonOne };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				Assert.AreEqual(0, _scheduleDayTwoPersonOne.PersonAssignment().PersonalLayers().Count());
				Assert.AreEqual(1, _scheduleDayOnePersonOne.PersonAssignment().PersonalLayers().Count());
				Assert.IsTrue(_scheduleDayOnePersonOne.SignificantPart() == SchedulePartView.PersonalShift);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldDeleteOvertimeInSwap()
		{
			using (_mockRepository.Record())
			{
				commonMocks();
			}

			using (_mockRepository.Playback())
			{
				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod2 = new DateTimePeriod(new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc));
				var definitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("name", MultiplicatorType.Overtime);

				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayTwoPersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 2));

				var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1);
				_scheduleDayOnePersonOne.Add(ass);
				ass.AddOvertimeLayer(new Activity("activity"), dateTimePeriod2, definitionSet);
			
				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayTwoPersonOne };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				Assert.AreEqual(0, _scheduleDayTwoPersonOne.PersonAssignment().OvertimeLayers().Count());
			}	
		}

		[Test]
		public void ShouldNotSwapDaysWithLocks()
		{
			using (_mockRepository.Record())
			{
                Expect.Call(_authorizationService.IsPermitted(null, new DateOnly(2011, 1, 1), _personOne)).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDictionary.Scenario).Return(_scenario).Repeat.AtLeastOnce();
			}

			using (_mockRepository.Playback())
			{
				IList<DateOnly> lockedDates = new List<DateOnly>{new DateOnly(2011, 1, 1)};
				_locks.Add(_personOne, lockedDates);

				_scheduleDayOnePersonOne = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personOne, new DateOnly(2011, 1, 1));
				_scheduleDayOnePersonTwo = ExtractedSchedule.CreateScheduleDay(_scheduleDictionary, _personTwo, new DateOnly(2011, 1, 3));

				var dateTimePeriod1 = new DateTimePeriod(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc));
				var dateTimePeriod3 = new DateTimePeriod(new DateTime(2011, 1, 3, 10, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 3, 17, 0, 0, DateTimeKind.Utc));

				_scheduleDayOnePersonOne.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personOne, dateTimePeriod1));
				_scheduleDayOnePersonTwo.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _personTwo, dateTimePeriod3));

				_selectionOne = new List<IScheduleDay> { _scheduleDayOnePersonOne };
				_selectionTwo = new List<IScheduleDay> { _scheduleDayOnePersonTwo };

				_swapRawService.Swap(_schedulePartModifyAndRollbackService, _selectionOne, _selectionTwo, _locks);

				Assert.AreEqual(new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc), _scheduleDayOnePersonOne.PersonAssignment().Period.StartDateTime);
				Assert.AreEqual(new DateTime(2011, 1, 3, 10, 0, 0, DateTimeKind.Utc), _scheduleDayOnePersonTwo.PersonAssignment().Period.StartDateTime);
			}
		}
	}
}
