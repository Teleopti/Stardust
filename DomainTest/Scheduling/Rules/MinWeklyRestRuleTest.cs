using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class MinWeeklyRestRuleTest
	{
		private MockRepository _mocks;
		private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private MinWeeklyRestRule _target;
		private IPermissionInformation _permissionInformation;
		private TimeZoneInfo _timeZone;
		private IContract _contract;
		private IPersonContract _personContract;
		private IPersonPeriod _personPeriod;
		private IPersonWeekViolatingWeeklyRestSpecification _personWeekViolatingWeeklyRestSpecification;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_weeksFromScheduleDaysExtractor = _mocks.StrictMock<IWeeksFromScheduleDaysExtractor>();
			_personWeekViolatingWeeklyRestSpecification = _mocks.StrictMock<IPersonWeekViolatingWeeklyRestSpecification>();
			_target = new MinWeeklyRestRule(_weeksFromScheduleDaysExtractor, _personWeekViolatingWeeklyRestSpecification);
			_permissionInformation = _mocks.StrictMock<IPermissionInformation>();
			_timeZone = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var minTimePerWeek = new TimeSpan(0, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			var weeklyRest = new TimeSpan(50, 0, 0);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek,
					nightlyRest,
					weeklyRest)
			};
			_personContract = _mocks.StrictMock<IPersonContract>();
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
		}

		[Test]
		public void CanCreateRuleAndAccessSimpleProperties()
		{
			Assert.IsNotNull(_target);
			Assert.IsFalse(_target.IsMandatory);
			Assert.IsTrue(_target.HaltModify);
			// ska man kunna ändra det??
			_target.HaltModify = false;
			Assert.IsFalse(_target.HaltModify);
		}

		[Test]
		public void ValidateReturnListWithSevenWhenNoPersonPeriod()
		{
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new IPersonPeriod[] { });
				Expect.Call(person.Name).Return(new Name("nn", "mm")).Repeat.Times(7);
			}

			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(7, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ValidateReturnEmptyListWhenNoAssignments()
		{
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, TimeSpan.FromHours(40))).Return(true).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ValidateReturnEmptyListWhenFirstAssignmentsHasRestBefore()
		{
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var weeklyRest = TimeSpan.FromHours(36);
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);

				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, weeklyRest)).Return(true).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ShouldConsiderDayOffBeforeWeek()
		{
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var minTimePerWeek = new TimeSpan(0, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			var weeklyRest = TimeSpan.FromHours(36);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek, nightlyRest, weeklyRest)
			};
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true))
					.Return(personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });
				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, weeklyRest)).Return(false).IgnoreArguments();

			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(7, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ShouldConsiderDayOffAfterWeek()
		{
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var minTimePerWeek = new TimeSpan(0, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			var weeklyRest = TimeSpan.FromHours(36);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek, nightlyRest, weeklyRest)
			};
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();
			
			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true))
					.Return(personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23,2010,8,29))).Return(new []{_personPeriod});
				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, weeklyRest)).Return(false).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(7, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ValidateReturnEmptyListWhenLastRestBetweenAssignments()
		{
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};


			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, TimeSpan.FromHours(40))).Return(true).IgnoreArguments();

			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ValidateReturnEmptyListWhenLastAssignmentsHasRestAfter()
		{
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();
			
			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();

				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });

				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, TimeSpan.FromHours(40))).Return(true).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
			Test, SetUICulture("sv-SE")]
		public void ValidateReturnListOfErrorsWhenNotEnoughWeeklyRest()
		{
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var minTimePerWeek = new TimeSpan(0, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			// four days weekly rest
			var weeklyRest = new TimeSpan(4, 0, 0, 0);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek,
					nightlyRest,
					weeklyRest)
			};
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });
				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, weeklyRest)).Return(false).IgnoreArguments();

			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays).ToArray();
				foreach (var response in ret)
				{
					Assert.IsTrue(response.FriendlyName.StartsWith("Veckan uppfyller inte"));
					Assert.IsTrue(response.Message.StartsWith("Veckan har inte tillräckligt"));
				}
				Assert.AreEqual(7, ret.Length);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"),
		 Test]
		public void ShouldNotReturnListIfSundayAndNextMondayIsNotScheduled()
		{
			var maxTimePerWeek = new TimeSpan(40, 0, 0);
			var minTimePerWeek = new TimeSpan(0, 0, 0);
			var nightlyRest = new TimeSpan(8, 0, 0);
			// four days weekly rest
			var weeklyRest = TimeSpan.FromHours(36);
			_contract = new Contract("for test")
			{
				WorkTimeDirective = new WorkTimeDirective(minTimePerWeek, maxTimePerWeek,
					nightlyRest,
					weeklyRest)
			};
			var person = _mocks.StrictMock<IPerson>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var dic = new Dictionary<IPerson, IScheduleRange> {{person, range}};
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();

			var lstOfDays = new List<IScheduleDay> {scheduleDay, scheduleDay2};

			var weekPeriod = new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29);
			var personWeek = new PersonWeek(person, weekPeriod);

			var personWeeks = new List<PersonWeek> {personWeek};
			var oldResponses = new List<IBusinessRuleResponse>();

			using (_mocks.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(lstOfDays, true)).Return(
					personWeeks);
				Expect.Call(range.BusinessRuleResponseInternalCollection).Return(oldResponses);
				Expect.Call(person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
				Expect.Call(_permissionInformation.DefaultTimeZone()).Return(_timeZone).Repeat.AtLeastOnce();
				Expect.Call(person.PersonPeriods(new DateOnlyPeriod(2010, 8, 23, 2010, 8, 29))).Return(new[] { _personPeriod });
				Expect.Call(_personPeriod.PersonContract).Return(_personContract).Repeat.Times(1);
				Expect.Call(_personContract.Contract).Return(_contract);
				Expect.Call(_personWeekViolatingWeeklyRestSpecification.IsSatisfyBy(range, personWeek.Week, weeklyRest)).Return(true).IgnoreArguments() ;
			}
			using (_mocks.Playback())
			{
				var ret = _target.Validate(dic, lstOfDays);
				Assert.AreEqual(0, ret.Count());
			}
		}

	}

}

