using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class NoteMapperTest
    {
        private NoteMapper _target;
        private AgentDayFactory _agdFactory;
        private MappedObjectPair _mappedObjectPair;
        private ICccTimeZoneInfo _timeZoneInfo;

        [SetUp]
        public void Setup()
        {
            _timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _agdFactory = new AgentDayFactory();
            _mappedObjectPair = new MappedObjectPair();
            _mappedObjectPair.Agent = _agdFactory.AgentPairList;
            _mappedObjectPair.Scenario = _agdFactory.ScenarioPairList;
            _target = new NoteMapper(_mappedObjectPair, _timeZoneInfo);
        }
        [Test]
        public void CanCreateMapper()
        {
            _target = new NoteMapper(new MappedObjectPair(), new CccTimeZoneInfo(TimeZoneInfo.Utc));
            Assert.IsNotNull(_target);
        }
        [Test]
        public void CanMap()
        {
            IScenario expectedScenario = _agdFactory.ScenarioPairList.GetPaired(_agdFactory.AgentDay().AgentScenario);
            IPerson expectedPerson = _agdFactory.AgentPairList.GetPaired(_agdFactory.AgentDay().AssignedAgent);
            INote note = _target.Map(_agdFactory.AgentDay());
            Assert.AreEqual(_agdFactory.AgentDay().Note, note.ScheduleNote);
            Assert.AreEqual(expectedScenario, note.Scenario);
            Assert.AreEqual(_agdFactory.AgentDay().AgentDate, note.NoteDate.Date);
            Assert.AreEqual(expectedPerson, note.Person);
        }
        [Test]
        public void VerifyNullIsReturnedIfNoteIsNullOrEmpty()
        {
            INote note = _target.Map(_agdFactory.AgentDayWithoutNote());
            Assert.IsNull(note);
        }
    }
}
