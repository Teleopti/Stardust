using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    /// <summary>
    /// Tests for the PersonDayOff class
    /// </summary>
    [TestFixture]
    public class PersonDayOffTest
    {
        private PersonDayOff _target;
        private DayOffTemplate _dayOff;
        private IPerson _person;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _dayOff = new DayOffTemplate(new Description("dayOff"));
            _dayOff.Anchor = TimeSpan.FromHours(10);
            _dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));
            _person = PersonFactory.CreatePerson("testAgent");
            _person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            _target = new PersonDayOff(_person,
                                    ScenarioFactory.CreateScenarioAggregate(),
                                    _dayOff,
                                    new DateOnly(2007, 1, 1));
                                    
        }

        [Test]
        public void VerifyMainReference()
        {
            Assert.AreSame(_person, _target.MainRoot);
        }

        /// <summary>
        /// Verify that new and properties work
        /// </summary>
        [Test]
        public void VerifyDefaultProperties()
        {
            var date = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual("testAgent", _target.Person.Name.FirstName);
            Assert.AreEqual(_dayOff.Flexibility, _target.DayOff.Flexibility);
            Assert.AreEqual(_dayOff.TargetLength, _target.DayOff.TargetLength);
            Assert.AreEqual(_dayOff.Anchor, _target.DayOff.AnchorLocal(TeleoptiPrincipal.Current.Regional.TimeZone).TimeOfDay);
            Assert.AreEqual(_dayOff.Description, _target.DayOff.Description);
            Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff, _target.FunctionPath);
            Assert.AreEqual(date, _target.DayOff.Anchor.Date);
            Assert.AreEqual(new DateTimePeriod(_target.DayOff.Anchor, _target.DayOff.Anchor.AddTicks(1)), ((IScheduleParameters)_target).Period);
            Assert.IsNotNull(_target.Scenario);
            Assert.IsNull(_target.CreatedBy);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.CreatedOn);
            Assert.IsNull(_target.UpdatedOn);
        }

        [Test]
        public void VerifyProtectedConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyThatCloneWorks()
        {
            PersonDayOff theClone = (PersonDayOff)_target.Clone();
            Assert.IsNull(theClone.Id);
            Assert.AreEqual(_target.DayOff.Anchor, theClone.DayOff.Anchor);
            Assert.AreEqual(TimeSpan.FromHours(4), theClone.DayOff.TargetLength);
            Assert.AreEqual(TimeSpan.FromHours(1), theClone.DayOff.Flexibility);
            Assert.AreEqual(_target.Person, theClone.Person);
        }

        [Test]
        public void VerifyCreateWithReplacedParameters()
        {
            var newPer = new Person();
            var newScen = new Scenario("new scen");

            var moveToTheseParameters = new PersonDayOff(newPer, newScen, _dayOff, new DateOnly(2000,1,1));

            IPersistableScheduleData newDo = _target.CloneAndChangeParameters(moveToTheseParameters);

            Assert.IsNull(newDo.Id);
            Assert.AreSame(newPer, newDo.Person);
            Assert.AreSame(newScen, newDo.Scenario);
            Assert.AreEqual(_target.Period, newDo.Period);
        }


        [Test]
        public void VerifyICloneableEntity()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());
            _target.Scenario.SetId(Guid.NewGuid());
            _target.Person.SetId(Guid.NewGuid());

            IPersonDayOff clone = _target.EntityClone();
            Assert.AreEqual(_target.Id, clone.Id);
            Assert.AreEqual(_target.Scenario.Id, clone.Scenario.Id);
            Assert.AreEqual(_target.Person.Id, clone.Person.Id);

            clone = _target.NoneEntityClone();
            Assert.IsNull(clone.Id);
            Assert.AreEqual(_target.Scenario.Id, clone.Scenario.Id);
            Assert.AreEqual(_target.Person.Id, clone.Person.Id);

            clone = (IPersonDayOff)_target.CreateTransient();
            Assert.IsNull(clone.Id);
            Assert.AreEqual(_target.Scenario.Id, clone.Scenario.Id);
            Assert.AreEqual(_target.Person.Id, clone.Person.Id);

        }

        [Test]
        public void VerifyBelongsToScenario()
        {
            Assert.IsTrue(_target.BelongsToScenario(_target.Scenario));
            Assert.IsFalse(_target.BelongsToScenario(new Scenario("f")));
            Assert.IsFalse(_target.BelongsToScenario(null));
        }

        [Test]
        public void VerifyBelongsToPeriod()
        {
            Assert.IsTrue(_target.BelongsToPeriod(new DateOnlyPeriod(new DateOnly(2007, 1, 1), new DateOnly(2007, 1, 1))));
        }

        [Test]
        public void CanCompareDayOffTemplateWithDayOffAndIsTrue()
        {
            
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));
            

            IPerson person = PersonFactory.CreatePerson("testAgent");

            TimeZoneInfo timeZoneInfo = StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone;
            person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

            PersonDayOff personDayOff = new PersonDayOff(person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));

            Assert.IsTrue(personDayOff.CompareToTemplate(dayOffTemplate));
        }

        [Test]
        public void CanCompareDayOffTemplateWithDayOffAndIsFalse()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);

            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));
            IPerson person = PersonFactory.CreatePerson("testAgent");

            TimeZoneInfo timeZoneInfo = StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone;
            person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

            IPersonDayOff personDayOff = new PersonDayOff(person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate, new DateOnly(2007, 1, 1));

            //Wrong Ancor
            IDayOffTemplate wrongAncorDayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(9);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));

            Assert.IsFalse(personDayOff.CompareToTemplate(wrongAncorDayOffTemplate));

            //Wrong Flexibility
            IDayOffTemplate wrongFlexibilityDayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(2));

            Assert.IsFalse(personDayOff.CompareToTemplate(wrongFlexibilityDayOffTemplate));

            //Wrong TargetLength
            IDayOffTemplate wrongTargetLengthDayOffTemplate = new DayOffTemplate(new Description("DayOffTemplate"));
            dayOffTemplate.Anchor = TimeSpan.FromHours(10);
            dayOffTemplate.SetTargetAndFlexibility(TimeSpan.FromHours(5), TimeSpan.FromHours(1));

            Assert.IsFalse(personDayOff.CompareToTemplate(wrongTargetLengthDayOffTemplate));
        }

        [Test]
        public void CanCompareDayOffTemplateForLocking()
        {
            IDayOffTemplate dayOffTemplate1 = DayOffFactory.CreateDayOff(new Description("DayOff1", "DO1"));
            IDayOffTemplate dayOffTemplate11 = DayOffFactory.CreateDayOff(new Description("DayOff1", "DO1"));
            IDayOffTemplate dayOffTemplate2 = DayOffFactory.CreateDayOff(new Description("DayOff2", "DO2"));

            _person = PersonFactory.CreatePerson("testAgent");
            TimeZoneInfo timeZoneInfo = StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone;
            _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

            IPersonDayOff personDayOff1 = new PersonDayOff(_person,
                                   ScenarioFactory.CreateScenarioAggregate(),
                                   dayOffTemplate1, new DateOnly(2007, 1, 1));
            IPersonDayOff personDayOff11 = new PersonDayOff(_person,
                       ScenarioFactory.CreateScenarioAggregate(),
                       dayOffTemplate11, new DateOnly(2007, 1, 2));
            IPersonDayOff personDayOff2 = new PersonDayOff(_person,
           ScenarioFactory.CreateScenarioAggregate(),
           dayOffTemplate2, new DateOnly(2007, 1, 2));

            Assert.IsTrue(personDayOff1.CompareToTemplateForLocking(dayOffTemplate1));
            Assert.IsTrue(personDayOff11.CompareToTemplateForLocking(dayOffTemplate1));
            Assert.IsFalse(personDayOff2.CompareToTemplateForLocking(dayOffTemplate1));
            Assert.IsTrue(personDayOff2.CompareToTemplateForLocking(dayOffTemplate2));

        }

        [Test]
        public void VerifyChecksum()
        {
            Assert.AreNotEqual(0, _target.Checksum());
        }
    }
}