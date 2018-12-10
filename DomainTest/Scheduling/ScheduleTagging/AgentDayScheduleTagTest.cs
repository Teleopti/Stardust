using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.ScheduleTagging
{
    [TestFixture]
    public class AgentDayScheduleTagTest
    {
        private IAgentDayScheduleTag _target;
        private IPerson _person;
        private DateOnly _tagDate;
        private IScenario _scenario;
        private IScheduleTag _scheduleTag;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson("Kalle");
            _tagDate = new DateOnly(2010, 4, 1);
            _scenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
            _scheduleTag = new ScheduleTag();
            _scheduleTag.Description = "Tag";
            _target = new AgentDayScheduleTag(_person, _tagDate, _scenario, _scheduleTag);
        }

        [Test]
        public void VerifyInstanceCanBeCreated()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProtectedConstructorExists()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        [Test]
        public void VerifyProperties()
        {
            DateTime dateTime = TimeZoneHelper.ConvertToUtc(_tagDate.Date, _person.PermissionInformation.DefaultTimeZone());
            DateTimePeriod expectedPeriod = new DateTimePeriod(dateTime, dateTime.AddDays(1));
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(expectedPeriod, _target.Period);
            Assert.AreEqual(_scenario, _target.Scenario);
            Assert.AreEqual(_scheduleTag.Description, _target.ScheduleTag.Description);
            Assert.AreEqual(_person, _target.MainRoot);
            Assert.AreEqual(_tagDate, _target.TagDate);

            _scheduleTag = new ScheduleTag();
            _scheduleTag.Description = "newTag";
            _target.ScheduleTag = _scheduleTag;
            Assert.AreEqual("newTag", _target.ScheduleTag.Description);
        }

        [Test]
        public void VerifyBelongsToPeriod()
        {
            Assert.IsTrue(_target.BelongsToPeriod(new DateOnlyAsDateTimePeriod(_tagDate, _person.PermissionInformation.DefaultTimeZone())));
            Assert.IsFalse(
                _target.BelongsToPeriod(new DateOnlyAsDateTimePeriod(_tagDate.AddDays(-1),
                                                                     _person.PermissionInformation.DefaultTimeZone())));
        }

        [Test]
        public void VerifyBelongsToPeriod2()
        {
            Assert.IsTrue(_target.BelongsToPeriod(new DateOnlyPeriod(_tagDate, _tagDate)));
            Assert.IsFalse(
                _target.BelongsToPeriod(new DateOnlyPeriod(_tagDate.AddDays(2), _tagDate.AddDays(3))));
        }

        [Test]
        public void VerifyClone()
        {
            IAgentDayScheduleTag clone = (IAgentDayScheduleTag)_target.Clone();

            Assert.AreNotSame(clone, _target);
            Assert.AreEqual(clone.ScheduleTag, _target.ScheduleTag);
            Assert.AreEqual(clone.Period, _target.Period);
            Assert.AreEqual(clone.Person, _target.Person);
            Assert.AreEqual(clone.Scenario, _target.Scenario);
        }

        //[Test]
        //public void VerifyNoneEntityClone()
        //{
        //    IAgentDayScheduleTag clone = _target.NoneEntityClone();

        //    Assert.AreNotSame(clone, _target);
        //    Assert.AreEqual(clone.ScheduleNote, _target.ScheduleNote);
        //    Assert.AreEqual(clone.Period, _target.Period);
        //    Assert.AreEqual(clone.Person, _target.Person);
        //    Assert.AreEqual(clone.Scenario, _target.Scenario);
        //}

        [Test]
        public void VerifyBelongsToScenario()
        {
            _target.BelongsToScenario(_scenario);
        }

        [Test]
        public void VerifyCorrectApplicationFunctionPathIsReturned()
        {
            Assert.AreEqual(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment, _target.FunctionPath);
        }

        [Test]
        public void VerifyCreateTransient()
        {
            Guid guid = Guid.NewGuid();
            _target.SetId(guid);

            Assert.AreEqual(guid, _target.Id);

			var data = _target.CreateTransient();

            Assert.IsNull(data.Id);
        }
        
        
        [Test]
        public void VerifyCloneAndChangeParameters()
        {
            var newScenario = new Scenario("new scenario");
            var tag = _target.ScheduleTag;

            var moveToTheseParameters = new AgentDayScheduleTag(_target.Person, _target.TagDate, newScenario, tag);

            var newTag = _target.CloneAndChangeParameters(moveToTheseParameters);
            IAgentDayScheduleTag castedTag = ((IAgentDayScheduleTag)newTag);
            Assert.IsNull(newTag.Id);
            Assert.AreSame(_target.Person, newTag.Person);
            Assert.AreNotSame(_target.Scenario, newTag.Scenario);
            Assert.AreSame(newScenario, newTag.Scenario);
            Assert.AreEqual(_target.Period, newTag.Period);
            Assert.AreEqual(_target.ScheduleTag, castedTag.ScheduleTag);
            Assert.AreEqual(_target.TagDate, castedTag.TagDate);
        }

    }
}