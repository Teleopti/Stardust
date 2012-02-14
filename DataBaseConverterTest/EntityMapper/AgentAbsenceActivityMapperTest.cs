using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class AgentAbsenceActivityMapperTest : MapperTest<global::Domain.AgentDay>
    {
        private AgentDayFactory _agdFactory;
        private global::Domain.AgentDay _agDay;
        private MappedObjectPair _mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 13; }
        }

        [SetUp]
        public void Setup()
        {
            _agdFactory = new AgentDayFactory();
            _mappedObjectPair = new MappedObjectPair();
            _mappedObjectPair.Absence = _agdFactory.AbsPairList;
            _mappedObjectPair.Agent = _agdFactory.AgentPairList;
            _mappedObjectPair.Scenario = _agdFactory.ScenarioPairList;
            _mappedObjectPair.AbsenceActivity = _agdFactory.AbsenceActivityList;
        }

        [Test]
        public void CanMapAgentAssignmentWithAbsenceActivity()
        {
            _agDay = _agdFactory.AgentDay();
            global::Domain.WorkShift ws = _agdFactory.WorkShift();
            _agDay.AgentDayAssignment.SetAssigned(ws, _agdFactory.ScheduleType);

            AgentAbsenceActivityMapper mapper = new AgentAbsenceActivityMapper(_mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IList<IPersonAbsence> retList = mapper.Map(_agDay);
            Assert.AreEqual(1, retList.Count);
        }

        [Test]
        public void CanMapAgentAssignmentWithoutAbsenceActivity()
        {
            _agDay = _agdFactory.AgentDay();
            IList<ActivityLayer> actLayerList = _agdFactory.ActivityLayerList();
            actLayerList.RemoveAt(2);
            global::Domain.WorkShift ws = _agdFactory.WorkShift(actLayerList);
            
            _agDay.AgentDayAssignment.SetAssigned(ws, _agdFactory.ScheduleType);

            AgentAbsenceActivityMapper mapper = new AgentAbsenceActivityMapper(_mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Utc));
            IList<IPersonAbsence> retList = mapper.Map(_agDay);
            Assert.AreEqual(0, retList.Count);
        }
    }
}
