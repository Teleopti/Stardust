using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Test class for PersonAbsence
    /// </summary>
    [TestFixture]
    public class PersonAbsenceTest
    {
        private IPerson person;
        private IScenario scenario;
        private IPersonAbsence target;
        private IAbsence absence;

        /// <summary>
        /// Runs before every test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            absence = new Absence();
            person = PersonFactory.CreatePerson();
            scenario = ScenarioFactory.CreateScenarioAggregate();
            target = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2001,1,1,2002,1,1)));
        }


        [Test]
        public void VerifyMainReference()
        {
            Assert.AreSame(person, target.MainRoot);
        }

        [Test]
        public void CanCreateInstance()
        {
            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Person);
            Assert.IsNotNull(target.Scenario);
            Assert.IsNotNull(target.Layer);
			
            Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence, target.FunctionPath);
        }

	    [Test]
	    public void ShouldAlwaysHaveCreationTimeAsLastChangeForCorrectPrioWhenSamePrioOnAbsence()
	    {
			target.LastChange.Value.Should().Be.GreaterThan(DateTime.UtcNow.AddSeconds(-30));
		}

        [Test]
        public void VerifyLastChange()
        {
            DateTime change = DateTime.Now;
            target.LastChange = change;
            Assert.AreEqual(change, target.LastChange);
        }

	    [Test]
	    public void ShouldNotProduceRequestAbsenceRemovedEventWhenNotInDefaultScenario()
	    {
		    var nonDefaultScenario = ScenarioFactory.CreateScenario("non default", false, false);
			var personAbsence = new PersonAbsence(person,nonDefaultScenario,new AbsenceLayer(absence,new DateTimePeriod(2001,1,1,2002,1,1)));
			personAbsence.NotifyDelete();

		    var events = personAbsence.PopAllEvents(null);
		    events.Count().Should().Be.EqualTo(0);

	    }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType()));
        }

        [Test]
        public void VerifyThatCloneWorks()
        {
            DateTimePeriod dtp = new DateTimePeriod(new DateTime(2007, 1, 1, 7, 0, 0, DateTimeKind.Utc),
                                     new DateTime(2007, 1, 3, 7, 0, 0, DateTimeKind.Utc));
            AbsenceLayer layer = new AbsenceLayer(AbsenceFactory.CreateAbsence("Sjuk"), dtp);
            target = new PersonAbsence(person, scenario, layer);
            PersonAbsence theClone = (PersonAbsence) target.Clone();

            Assert.AreEqual(dtp,target.Layer.Period);
            Assert.AreEqual(dtp, theClone.Layer.Period);
            Assert.IsNull(theClone.Id);
            Assert.AreEqual(target.Person, theClone.Person);
        }

        [Test]
        public void VerifyCreateWithReplacedParameters()
        {
            var newPer = new Person();
            var newScen = new Scenario("new scen");

            var moveToTheseParameters = new PersonAbsence(newPer, newScen, new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2000,12,12)));

            var newDo = ((PersonAbsence)target).CloneAndChangeParameters(moveToTheseParameters);

            Assert.IsNull(newDo.Id);
            Assert.AreSame(newPer, newDo.Person);
            Assert.AreSame(newScen, newDo.Scenario);
            Assert.AreEqual(target.Period, newDo.Period);
        }

        [Test]
        public void VerifyBelongsToScenario()
        {
            Assert.IsTrue(target.BelongsToScenario(target.Scenario));
            Assert.IsFalse(target.BelongsToScenario(new Scenario("f")));
            Assert.IsFalse(target.BelongsToScenario(null));
        }

        [Test]
        public void VerifyBelongsToPeriod()
        {
            Assert.IsTrue(target.BelongsToPeriod(new DateOnlyPeriod(new DateOnly(2000, 1, 1), new DateOnly(2003, 1, 1))));
            Assert.IsFalse(target.BelongsToPeriod(new DateOnlyPeriod(new DateOnly(1000, 1, 1), new DateOnly(1003, 1, 1))));
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            DateTimePeriod dtp = new DateTimePeriod(new DateTime(2007, 1, 1, 7, 0, 0, DateTimeKind.Utc),
                                     new DateTime(2007, 1, 3, 7, 0, 0, DateTimeKind.Utc));
            AbsenceLayer layer = new AbsenceLayer(AbsenceFactory.CreateAbsence("Sjuk"), dtp);
            target = new PersonAbsence(person, scenario, layer);
            target.SetId(Guid.NewGuid());
            target.Person.SetId(Guid.NewGuid());
            target.Scenario.SetId(Guid.NewGuid());

            IPersonAbsence clone = target.EntityClone();
            Assert.AreEqual(target.Id, clone.Id);
            Assert.AreEqual(target.Person.Id, clone.Person.Id);
            Assert.AreEqual(target.Scenario.Id, clone.Scenario.Id);

            clone = target.NoneEntityClone();
            Assert.AreEqual(target.Person.Id, clone.Person.Id);
            Assert.AreEqual(target.Scenario.Id, clone.Scenario.Id);
            Assert.IsNull(clone.Id);

            clone = (IPersonAbsence)target.CreateTransient();
            Assert.AreEqual(target.Person.Id, clone.Person.Id);
            Assert.AreEqual(target.Scenario.Id, clone.Scenario.Id);
            Assert.IsNull(clone.Id);
        }

        [Test]
        public void VerifyPeriod()
        {
            Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2002, 1, 1), target.Period);
        }

        [Test]
        public void VerifyCanSplit()
        {
            IList<IPersonAbsence> splitList;

            //period to remove
            DateTimePeriod splitPeriod1 = new DateTimePeriod(2001, 1, 10, 2001, 1, 11);
            DateTimePeriod splitPeriod2 = new DateTimePeriod(2000, 12, 30, 2001, 1, 11);
            DateTimePeriod splitPeriod3 = new DateTimePeriod(2001, 12, 10, 2002, 1, 10);
            DateTimePeriod splitPeriod4 = new DateTimePeriod(2001, 1, 1, 2002, 1, 1);
            //expected periods
            DateTimePeriod expectedPeriod1 = new DateTimePeriod(2001, 1, 1, 2001, 1, 10);
            DateTimePeriod expectedPeriod2 = new DateTimePeriod(2001, 1, 11, 2002, 1, 1);
            DateTimePeriod expectedPeriod3 = new DateTimePeriod(2001, 1, 11, 2002, 1, 1);
            DateTimePeriod expectedPeriod4 = new DateTimePeriod(2001, 1, 1, 2001, 12, 10);


            //split
            splitList = target.Split(splitPeriod1);
            //we should get two PersonAbsences back
            Assert.AreEqual(2, splitList.Count);
            //check first period
            Assert.AreEqual(expectedPeriod1, splitList[0].Period);
            //check last period
            Assert.AreEqual(expectedPeriod2, splitList[1].Period);

            //split
            splitList = target.Split(splitPeriod2);
            //we should get one PersonAbsence back
            Assert.AreEqual(1, splitList.Count);
            //check period
            Assert.AreEqual(expectedPeriod3, splitList[0].Period);

            //split
            splitList = target.Split(splitPeriod3);
            //we should get one PersonAbsence back
            Assert.AreEqual(1, splitList.Count);
            //check period
            Assert.AreEqual(expectedPeriod4, splitList[0].Period);

            //split
            splitList = target.Split(splitPeriod4);
            //we should get 0 PersonAbsence back
            Assert.AreEqual(0, splitList.Count);

        }

        [Test]
        public void VerifyCanMerge()
        {
            IPersonAbsence mergedAbsence;
            DateTimePeriod period1 = new DateTimePeriod(2000, 10, 1, 2001, 1, 1);
            DateTimePeriod period2 = new DateTimePeriod(2001, 1, 1, 2002, 1, 1);
            DateTimePeriod period3 = new DateTimePeriod(2002, 1, 1, 2003, 1, 1);
            DateTimePeriod period4 = new DateTimePeriod(2001, 10, 1, 2003, 1, 1);
            DateTimePeriod period5 = new DateTimePeriod(2005, 10, 1, 2006, 10, 1);


            IPersonAbsence personAbsence1 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, period1));
            IPersonAbsence personAbsence2 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, period2));
            IPersonAbsence personAbsence3 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, period3));
            IPersonAbsence personAbsence4 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, period4));
            IPersonAbsence personAbsence6 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, period5));
            IPersonAbsence personAbsence5 = new PersonAbsence(person, scenario, new AbsenceLayer(new Absence(), period4));

            //merge adjacent to start
            mergedAbsence = target.Merge(personAbsence1);
            Assert.AreEqual(new DateTimePeriod(2000, 10, 1, 2002, 1, 1), mergedAbsence.Period);

            //merge same same period
            mergedAbsence = target.Merge(personAbsence2);
            Assert.AreEqual(target.Period, mergedAbsence.Period);

            //merge adjacent to end
            mergedAbsence = target.Merge(personAbsence3);
            Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2003, 1, 1), mergedAbsence.Period);

            //merge from within and end after
            mergedAbsence = target.Merge(personAbsence4);
            Assert.AreEqual(new DateTimePeriod(2001, 1, 1, 2003, 1, 1), mergedAbsence.Period);

            //merge with not same absence
            mergedAbsence = target.Merge(personAbsence5);
            Assert.IsNull(mergedAbsence);

            //merge with period outside
            mergedAbsence = target.Merge(personAbsence6);
            Assert.IsNull(mergedAbsence);

        }


	}
}