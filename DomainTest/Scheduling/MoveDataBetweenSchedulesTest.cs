using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class MoveDataBetweenSchedulesTest
    {
        private IMoveDataBetweenSchedules target;
        private IScheduleDictionary destination;
        private INewBusinessRuleCollection newRules;

        [SetUp]
        public void Setup()
        {
            newRules = NewBusinessRuleCollection.Minimum();
            target = new MoveDataBetweenSchedules(newRules,new ResourceCalculationOnlyScheduleDayChangeCallback());
            destination = new ScheduleDictionary(new Scenario("dest scen"), new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2000, 1, 10)));
        }

        [Test]
        public void VerifySimpleExport()
        {
            var person1 = new Person {Name=new Name("person1", "person1")};
            var person2 = new Person { Name = new Name("person2", "person2") };
            IPersonAssignment assValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("dummy1"), 
                                                                                        person1,
                                                                                        new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
            IPersonAssignment assNonValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("dummy2"),
                                                                                        person2,
                                                                                        new DateTimePeriod(2000, 1, 2, 2000, 1, 4));

            putScheduleDataToDic(PersonAssignmentFactory.CreateAssignmentWithMainShift(destination.Scenario, new Person(), new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
            putScheduleDataToDic(PersonAssignmentFactory.CreateAssignmentWithMainShift(destination.Scenario, person1, new DateTimePeriod(2000, 1, 3, 2000, 1, 4)));
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
            var nfRule = new NewFailingRule();
            var nr = NewBusinessRuleCollection.Minimum();
            nr.Add(nfRule);
            target =new MoveDataBetweenSchedules( nr,new ResourceCalculationOnlyScheduleDayChangeCallback());

            IPersonAssignment assValid = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("sdf"),
                                                                            new Person(),
                                                                            new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
           
            IScheduleDay part = createPartWithData(assValid, new DateOnly(2000,1,3));
            var res = target.CopySchedulePartsToAnotherDictionary(destination, new List<IScheduleDay> { part });
            Assert.AreEqual(1, res.Count());
        }

        [Test]
        public void VerifyDoNotDoAnythingOnScheduleDataNotImplementingIExportToAnotherScenario()
        {
            var person = PersonFactory.CreatePerson();
            var prefDay = new PreferenceDay(person, new DateOnly(2000, 1, 2), new PreferenceRestriction());
            var org1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(destination.Scenario, person,
                                                                            new DateTimePeriod(2000, 1, 2, 2000, 1, 3));
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
			var scenario = new Scenario("scenario");
			var person = PersonFactory.CreatePerson("person");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, new DateTimePeriod(2000, 1, 3, 2000, 1, 3));
			putScheduleDataToDic(PersonAbsenceFactory.CreatePersonAbsence(person, destination.Scenario, new DateTimePeriod(2000, 1, 4, 2000, 1, 5)));
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
				var person = PersonFactory.CreatePerson("person");
				var date = new DateOnly(2000, 1, 1);
				var orgAss = new PersonAssignment(person, destination.Scenario, date);
				orgAss.SetId(Guid.NewGuid());
				putScheduleDataToDic(orgAss);

				var newAss = new PersonAssignment(person, destination.Scenario, date);
				newAss.SetId(Guid.NewGuid());
				newAss.AddMainLayer(new Activity("dsf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
				var schedulePart = createPartWithData(newAss, date);

				target.CopySchedulePartsToAnotherDictionary(destination, new[] {schedulePart});
				var modifiedPart = destination[person].ScheduledDay(date);
				var assResult = modifiedPart.PersonAssignment();
				assResult.Id.Should().Be.EqualTo(orgAss.Id.Value);
				assResult.ShiftLayers.Count().Should().Be.EqualTo(1);
			}


        private static IScheduleDay createPartWithData(IScheduleData data, DateOnly dateOnly)
        {
            IScenario scenario = data.Scenario;
            if (scenario == null)
                scenario = new Scenario("sdf");
            IScheduleDictionary dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(data.Period));
            var part = ExtractedSchedule.CreateScheduleDay(dic, data.Person, dateOnly);
            part.Add(data);
            return part;
        }

        private void putScheduleDataToDic(IScheduleData data)
        {
            ((ScheduleRange)destination[data.Person]).Add(data);
        }

        
        private class NewFailingRule : INewBusinessRule
        {
            public string ErrorMessage
            {
                get { return "hotta brudar från djursholm"; }
            }

            public bool IsMandatory
            {
                get { return false; }
            }

            public bool HaltModify
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

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
                                                                      new DateTimePeriod(), PersonFactory.CreatePerson(), dateOnlyPeriod);
                ret.Add(resp);
                return ret;
            }
        }
    }

}
