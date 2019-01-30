using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class MoveDataBetweenSchedulesTest
    {
        private IMoveDataBetweenSchedules target;
        private IScheduleDictionary destination;
        private INewBusinessRuleCollection newRules;
	    private IPersistableScheduleDataPermissionChecker permissionChecker;

		private void setup()
        {
	        permissionChecker = new PersistableScheduleDataPermissionChecker(new FullPermission());
			newRules = NewBusinessRuleCollection.Minimum();
            target = new MoveDataBetweenSchedules(newRules, new DoNothingScheduleDayChangeCallBack());
            destination = new ScheduleDictionary(new Scenario("dest scen"), new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 10)), permissionChecker, new FullPermission());
        }

        [Test]
        public void VerifySimpleExport()
        {
	        setup();
			var person1 = new Person().WithName(new Name("person1", "person1")).InTimeZone(TimeZoneInfo.Utc);
            var person2 = new Person().WithName(new Name("person2", "person2")).InTimeZone(TimeZoneInfo.Utc);
            IPersonAssignment assValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1,
                                                                                        new Scenario("dummy1"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
            IPersonAssignment assNonValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(person2,
                                                                                        new Scenario("dummy2"), new DateTimePeriod(2000, 1, 2, 2000, 1, 4));

            putScheduleDataToDic(PersonAssignmentFactory.CreateAssignmentWithMainShift(new Person(), destination.Scenario, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
            putScheduleDataToDic(PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, destination.Scenario, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
            IScheduleDay part1 = createPartWithData(assValid, new DateOnly(2000,1,3));
            IScheduleDay part2 = createPartWithData(assNonValid, new DateOnly(2000,1,2));

            target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { part1, part2 });

            Assert.AreEqual(3, destination.Count); //tre snubbar
            var pers1Data = destination[person1].ScheduledDay(new DateOnly(2000,1,3)).PersistableScheduleDataCollection();
            var pers2Data = destination[person2].ScheduledDay(new DateOnly(2000, 1, 3)).PersistableScheduleDataCollection();
            Assert.AreEqual(1, pers1Data.Count());
            Assert.AreEqual(0, pers2Data.Count());
            var data1 = pers1Data.First();
            Assert.IsNull(data1.Id);
            Assert.AreSame(person1, data1.Person);
            Assert.AreSame(destination.Scenario, data1.Scenario);
            Assert.AreEqual(assValid.Period, data1.Period);
            Assert.AreNotSame(assValid, data1);
        }

        [Test]
        public void VerifyCorrectBusinessRuleIsReturned()
        {
			setup();
			var nfRule = new NewFailingRule();
            var nr = NewBusinessRuleCollection.Minimum();
            nr.Add(nfRule);
            target =new MoveDataBetweenSchedules( nr, new DoNothingScheduleDayChangeCallBack());

            IPersonAssignment assValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Person().InTimeZone(TimeZoneInfo.Utc),
                                                                            new Scenario("sdf"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4));

            IScheduleDay part = createPartWithData(assValid, new DateOnly(2000,1,3));
            var res = target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { part });
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void VerifyDoNotDoAnythingOnScheduleDataNotImplementingIExportToAnotherScenario()
        {
			setup();
			var person = PersonFactory.CreatePerson();
            var prefDay = new PreferenceDay(person, new DateOnly(2000, 1, 2), new PreferenceRestriction());
            var org1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
                                                                            destination.Scenario, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));
            org1.SetId(Guid.NewGuid());
            var org2 = new PreferenceDay(person, new DateOnly(2000, 1, 2), new PreferenceRestriction());
            ((IEntity) org2).SetId(Guid.NewGuid());
            putScheduleDataToDic(org1);
            putScheduleDataToDic(org2);

            IScheduleDay part = createPartWithData(prefDay, new DateOnly(2000,1,2));

            target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { part });
            Assert.IsTrue(destination[person].Contains(org2));
            Assert.IsFalse(destination[person].Contains(org1));
            Assert.IsFalse(destination[person].Contains(prefDay));
        }

		[Test]
		public void ShouldNotAffectAbsencesOnNextDay()
		{
			setup();
			var scenario = new Scenario("scenario");
			var person = PersonFactory.CreatePerson("person");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2000, 1, 3, 8, 2000, 1, 3, 9));
			putScheduleDataToDic(PersonAbsenceFactory.CreatePersonAbsence(person, destination.Scenario, new DateTimePeriod(2000, 1, 4, 8, 2000, 1, 5, 9)));
			var schedulePart = createPartWithData(personAssignment, new DateOnly(2000, 1, 3));
			target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { schedulePart });
			var scheduleDay = destination[person].ScheduledDay(new DateOnly(2000, 1, 3));

			Assert.AreEqual(2, scheduleDay.PersistableScheduleDataCollection().Count());
			Assert.AreEqual(1, scheduleDay.PersistableScheduleDataCollection().OfType<PersonAssignment>().Count());
			Assert.AreEqual(1, scheduleDay.PersistableScheduleDataCollection().OfType<PersonAbsence>().Count());
		}

			[Test]
			public void ShouldReplaceAssignmentIfSameDatePersonScenario()
			{
			setup();
			var person = PersonFactory.CreatePerson("person");
				var date = new DateOnly(2000, 1, 1);
				var orgAss = new PersonAssignment(person, destination.Scenario, date);
				orgAss.SetId(Guid.NewGuid());
				putScheduleDataToDic(orgAss);

				var newAss = new PersonAssignment(person, destination.Scenario, date);
				newAss.SetId(Guid.NewGuid());
				newAss.AddActivity(new Activity("dsf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
				var schedulePart = createPartWithData(newAss, date);

				target.CopySchedulePartsToAnotherDictionary(destination, new[] {schedulePart});
				var modifiedPart = destination[person].ScheduledDay(date);
				var assResult = modifiedPart.PersonAssignment();
				assResult.Id.Should().Be.EqualTo(orgAss.Id.Value);
				assResult.ShiftLayers.Count().Should().Be.EqualTo(1);
			}

			[Test]
			public void ShouldConsiderAllExportedDaysWhenValidatingNightlyRest()
			{
			setup();
			var activity = new Activity("activity") { InWorkTime = true };
				var shiftCategory = new ShiftCategory("shiftCategory");
				var person1 = new Person().WithName(new Name("person1", "person1")).InTimeZone(TimeZoneInfo.Utc);
				var dateOnly = new DateOnly(new DateTime(2000, 1, 1));
				var team = new Team();
				var personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
				var personPeriod = PersonPeriodFactory.CreatePersonPeriod(dateOnly, personContract, team);
				var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				var nightlyRest = new NewNightlyRestRule(workTimeStartEndExtractor);
				var rules = NewBusinessRuleCollection.Minimum();

				person1.AddPersonPeriod(personPeriod);
				rules.Add(nightlyRest);
				target = new MoveDataBetweenSchedules(rules, new DoNothingScheduleDayChangeCallBack());

				var startTimeLateDay1 = new DateTime(2000, 1, 3, 15, 0, 0, DateTimeKind.Utc);
				var endTimeLateDay1 = new DateTime(2000, 1, 3, 22, 0, 0, DateTimeKind.Utc);

				var startTimeLateDay2 = new DateTime(2000, 1, 4, 15, 0, 0, DateTimeKind.Utc);
				var endTimeLateDay2 = new DateTime(2000, 1, 4, 22, 0, 0, DateTimeKind.Utc);

				var startTimeEarlyDay1 = new DateTime(2000, 1, 3, 6, 0, 0, DateTimeKind.Utc);
				var endTimeEarlyDay1 = new DateTime(2000, 1, 3, 15, 0, 0, DateTimeKind.Utc);

				var startTimeEarlyDay2 = new DateTime(2000, 1, 4, 6, 0, 0, DateTimeKind.Utc);
				var endTimeEarlyDay2 = new DateTime(2000, 1, 4, 15, 0, 0, DateTimeKind.Utc);

				var sourceDay1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("dummy1"), activity, new DateTimePeriod(startTimeLateDay1, endTimeLateDay1), shiftCategory);
				var sourceDay2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, new Scenario("dummy1"), activity, new DateTimePeriod(startTimeLateDay2, endTimeLateDay2), shiftCategory);

				var destinationDay1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, destination.Scenario, activity, new DateTimePeriod(startTimeEarlyDay1, endTimeEarlyDay1), shiftCategory);
				var destinationDay2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, destination.Scenario, activity, new DateTimePeriod(startTimeEarlyDay2, endTimeEarlyDay2), shiftCategory);

				putScheduleDataToDic(destinationDay1);
				putScheduleDataToDic(destinationDay2);

				var sourceScheduleDay1 = createPartWithData(sourceDay1, new DateOnly(2000, 1, 3));
				var sourceScheduleDay2 = createPartWithData(sourceDay2, new DateOnly(2000, 1, 4));

				var result = target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { sourceScheduleDay1, sourceScheduleDay2 });
				Assert.IsEmpty(result);
			}

		private static IScheduleDay createPartWithData(IScheduleData data, DateOnly dateOnly)
        {
            IScenario scenario = data.Scenario;
            if (scenario == null)
                scenario = new Scenario("sdf");
            IScheduleDictionary dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(data.Period), new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
            var part = ExtractedSchedule.CreateScheduleDay(dic, data.Person, dateOnly, CurrentAuthorization.Make());
            part.Add(data);
            return part;
        }

        private void putScheduleDataToDic(IScheduleData data)
        {
            ((ScheduleRange)destination[data.Person]).Add(data);
        }

        private class NewFailingRule : INewBusinessRule
        {
            public bool IsMandatory
            {
                get { return false; }
            }

            public bool HaltModify
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool Configurable => true;

            public bool ForDelete
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
            {
                IList<IBusinessRuleResponse> ret = new List<IBusinessRuleResponse>();
                var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(), new DateOnly());
                IBusinessRuleResponse resp = new BusinessRuleResponse(typeof(NewFailingRule), "", true, true,
                                                                      new DateTimePeriod(), PersonFactory.CreatePerson(), dateOnlyPeriod, FriendlyName);
                ret.Add(resp);
                return ret;
            }

	        public string FriendlyName
	        {
		        get { return "tjillevippen"; }
	        }

			public string Description => "Description of NewFailingRule";
		}
    }
}