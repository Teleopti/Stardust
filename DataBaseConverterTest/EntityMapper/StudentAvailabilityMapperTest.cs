using System;
using System.Collections.Generic;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Interfaces.Domain;
using TimePeriod = Domain.TimePeriod;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class StudentAvailabilityMapperTest
    {
        private StudentAvailabilityMapper _target;
        private MappedObjectPair _mappedObjectPair;
        private int _empId;

        [SetUp]
        public void Setup()
        {
            _empId = 1234;
            var agent = new Agent(_empId, "Ben", "Dog", "ben@dog.com", "sign", null, null, null, "note");
            IPerson person = new Domain.Common.Person();
            var personsPair = new ObjectPairCollection<Agent, IPerson> {{agent, person}};
            _mappedObjectPair = new MappedObjectPair {Agent = personsPair};

            _target = new StudentAvailabilityMapper(_mappedObjectPair);
        }

        [Test]
        public void ShouldMapOldEntityWithOneRestrictionToNewEntity()
        {
            var agentDayFactory = new AgentDayFactory();
            AgentDay oldEntity = agentDayFactory.AgentDay();
            var openLayer = new OpenLayer(new TimePeriod(new TimeSpan(0, 0, 0), new TimeSpan(1, 0, 0)));
            var cccListCollection = new CccListCollection<OpenLayer>(new List<OpenLayer>{openLayer}, CollectionType.Locked);
            oldEntity.Limitation = new AgentLimitation(cccListCollection);

            IStudentAvailabilityDay newEntity = _target.Map(oldEntity);

            Assert.AreEqual(new DateOnly(oldEntity.AgentDate), newEntity.RestrictionDate);
            Assert.IsNotNull(newEntity.RestrictionCollection);
            Assert.AreEqual(1, newEntity.RestrictionCollection.Count);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[0].Period.StartTime, newEntity.RestrictionCollection[0].StartTimeLimitation.StartTime);
            Assert.That(newEntity.RestrictionCollection[0].StartTimeLimitation.EndTime, Is.Null);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[0].Period.EndTime, newEntity.RestrictionCollection[0].EndTimeLimitation.EndTime);
            Assert.That(newEntity.RestrictionCollection[0].EndTimeLimitation.StartTime, Is.Null);
        }

        [Test]
        public void ShouldMapOldEntityWithTwoRestrictionsToNewEntity()
        {
            var agentDayFactory = new AgentDayFactory();
            AgentDay oldEntity = agentDayFactory.AgentDay();
            var openLayer1 = new OpenLayer(new TimePeriod(new TimeSpan(0, 0, 0), new TimeSpan(0, 12, 0)));
            var openLayer2 = new OpenLayer(new TimePeriod(new TimeSpan(0, 21, 0), new TimeSpan(1, 6, 0)));
            var cccListCollection = new CccListCollection<OpenLayer>(new List<OpenLayer> { openLayer1, openLayer2 }, CollectionType.Locked);
            oldEntity.Limitation = new AgentLimitation(cccListCollection);

            IStudentAvailabilityDay newEntity = _target.Map(oldEntity);

            Assert.AreEqual(new DateOnly(oldEntity.AgentDate), newEntity.RestrictionDate);
            Assert.IsNotNull(newEntity.RestrictionCollection);
            Assert.AreEqual(2, newEntity.RestrictionCollection.Count);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[0].Period.StartTime, newEntity.RestrictionCollection[0].StartTimeLimitation.StartTime);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[0].Period.EndTime, newEntity.RestrictionCollection[0].EndTimeLimitation.EndTime);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[1].Period.StartTime, newEntity.RestrictionCollection[1].StartTimeLimitation.StartTime);
            Assert.AreEqual(oldEntity.Limitation.WebCoreTime[1].Period.EndTime, newEntity.RestrictionCollection[1].EndTimeLimitation.EndTime);
        }

        [Test]
        public void ShouldReturnNullWhenTryingToMapOldEntityWithoutRestrictions()
        {
            var agentDayFactory = new AgentDayFactory();
            AgentDay oldEntity = agentDayFactory.AgentDay();

            IStudentAvailabilityDay newEntity = _target.Map(oldEntity);

            Assert.IsNull(newEntity);
        }
    }
}
