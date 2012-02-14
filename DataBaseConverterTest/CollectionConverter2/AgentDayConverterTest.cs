using System;
using System.Collections.Generic;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter2;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.DomainTest.FakeData;
using AgentAssignment=Teleopti.Ccc.Domain.Scheduling.AgentAssignment;
using ShiftCategory=Domain.ShiftCategory;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter2
{
    /// <summary>
    /// Tests for the AgentConverter
    /// </summary>
    [TestFixture]
    public class AgentDayConverterTest
    {
        //private DummyCacheBase _cBase;
        //private CacheBaseConverter<Dummy, Dummy> _converter;
        private MockRepository _mocks;
        private IUnitOfWork _uowMock;
        private IRepository<AgentAssignment> _repMock;
        private IRepository<AgentAbsence> _repAbsenceMock;
        private IRepository<AgentDayOff> _repDayOffMock;
        private IRepository<AgentAvailability> _repAvailabilityMock;
        //private IMapper<Dummy, Dummy> _mapMock;


        private DateTime fromDate;
        private DateTime toDate;

        private AgentDayFactory agdFactory;
        private AgentAbsenceMapper agAbsMapper;
        private AgentDayOffMapper agDayOffMapper;
        private AgentAvailabilityMapper agAvailabilityMapper;
        // private ObjectPairCollection<ShiftCategory, Domain.Scheduling.ShiftCategory> ShiftCatPairList;
        private AgentDayConverter agdConverter;
        private AgentAssignmentMapper agAssMapper;
        private Availability defaultAvailability;

        /// <summary>
        /// Sets the up.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //Mocks
            _mocks = new MockRepository();
            _uowMock = _mocks.CreateMock<IUnitOfWork>();
            _repMock = _mocks.CreateMock<IRepository<AgentAssignment>>();
            _repAbsenceMock = _mocks.CreateMock<IRepository<AgentAbsence>>();
            _repDayOffMock = _mocks.CreateMock<IRepository<AgentDayOff>>();
            _repAvailabilityMock = _mocks.CreateMock<IRepository<AgentAvailability>>();
            //_mapMock = _mocks.CreateMock<IMapper<Dummy, Dummy>>();

            fromDate = new DateTime(2000, 1, 1);
            toDate = new DateTime(2000, 1, 1);

            defaultAvailability = AvailabilityFactory.CreateAvailability("Available");

            agdFactory = new AgentDayFactory();
            MappedObjectPair mapped = new MappedObjectPair();
            mapped.Agent = agdFactory.AgentPairList;
            mapped.Activity = agdFactory.ActPairList;
            mapped.Absence = agdFactory.AbsPairList;
            mapped.ShiftCategory = new ObjectPairCollection<ShiftCategory, Domain.Scheduling.ShiftCategory>();
            mapped.Scenario = agdFactory.ScenarioPairList;

            agAbsMapper = new AgentAbsenceMapper(mapped,TimeZoneInfo.Utc);

            agDayOffMapper = new AgentDayOffMapper(mapped, TimeZoneInfo.Utc);

            agAvailabilityMapper = new AgentAvailabilityMapper(mapped, TimeZoneInfo.Utc, defaultAvailability);

            agdConverter = new AgentDayConverter(fromDate, toDate);
            agAssMapper = new AgentAssignmentMapper(mapped, TimeZoneInfo.Utc);
        }

        /// <summary>
        /// Determines whether this instance can convert an AgentDay.
        /// </summary>
        [Test]
        public void CanConvertAgentDay()
        {
            //Setup agentdagar
            AgentDay agentDay = agdFactory.AgentDay();
            AgentDay absDay = agdFactory.AgentDay();
            AgentDay DayOffDay = agdFactory.AgentDay();
            AgentDay availDay = agdFactory.AgentDay();
            AgentDay emptyDay = agdFactory.AgentDay();
            absDay.AgentDayAssignment.SetAssigned(agdFactory.Absence("Semester", "SE", false),
                                                  new SchedType(1, "Web", true, false, false));
            DayOffDay.AgentDayAssignment.SetAssigned(agdFactory.Absence("Day Off", "FR", true),
                                                     new SchedType(1, "Web", true, false, false));
            availDay.Limitation = agdFactory.AgentLimitation();
            agentDay.AgentDayAssignment.AddFillup(agdFactory.FillUpShift(), agdFactory.ScheduleType);
            IList<AgentDay> agdColl = new List<AgentDay>();
            agdColl.Add(agentDay);
            agdColl.Add(absDay);
            agdColl.Add(DayOffDay);
            agdColl.Add(availDay);
            agdColl.Add(emptyDay);

            //mocks unique for this test
            IAgentDayReader agdReader = _mocks.CreateMock<IAgentDayReader>();
            IAgentDayCollection tempColl = _mocks.CreateMock<IAgentDayCollection>();

            using (_mocks.Record())
            {
                Expect.Call(agdReader.LoadAgentDays(new DatePeriod(fromDate, toDate)))
                    .Return(tempColl);
                Expect.Call(tempColl.GetEnumerator()).Return(agdColl.GetEnumerator());
                _repMock.Add(null); //spara ny agentass
                LastCall.Repeat.Once().IgnoreArguments();
                _repAbsenceMock.Add(null);
                LastCall.Repeat.Once().IgnoreArguments();
                _repDayOffMock.Add(null);
                LastCall.Repeat.Once().IgnoreArguments();
                _repAvailabilityMock.Add(null);
                LastCall.Repeat.Once().IgnoreArguments();
                _uowMock.PersistAll();
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(4,
                                agdConverter.Convert(_repMock, _repAbsenceMock, _repDayOffMock, _repAvailabilityMock,
                                                     _uowMock, agdReader, agAssMapper,
                                                     agAbsMapper, agDayOffMapper, agAvailabilityMapper));
            }
        }
    }
}