using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class XCellTest
    {
        private XCell _xCell;
        private Person _agent;
        private AbsenceLayer _absLayer;
        private Absence _abs;
        private DateTimePeriod _dtp;
        private DateTime _start = new DateTime(2007, 11, 5, 8, 0, 0, DateTimeKind.Utc);
        private DateTime _end = new DateTime(2007, 11, 5, 17, 0, 0, DateTimeKind.Utc);
        private DateTime _endAnotherDay = new DateTime(2007, 11, 8, 17, 0, 0, DateTimeKind.Utc);
        private AnchorDateTimePeriod _anchorDateTimePeriod;
        private Scenario _scenario;
        private PersonAssignment _personAssignment;
        private PersonAbsence _personAbsence;
        private IDictionary<AgentDate, DailyAgentAbsenceCollection> _agentAbsDic;
        private IDictionary<AgentDate, DailyAgentAssignmentCollection> _agentAssignmentDictionary;
        private IDictionary<AgentDate, PersonDayOff> _agentDayOffDictionary;
        private AgentDate _key;
        private XCellCache _cache;
        private DateTimePeriod _dtpSeveralDays;
        private AbsenceLayer _absLayer2;
        private PersonAbsence _newAbsence;

        [SetUp]
        public void Setup()
        {
            Percent percent = new Percent(0.7);
            _anchorDateTimePeriod = new AnchorDateTimePeriod(_start, new TimeSpan(1, 0, 0), percent);
            _scenario = new Scenario("Test");
            _dtp = TimeZoneHelper.NewDateTimePeriodFromLocalDateTime(_start, _end);
            _dtpSeveralDays = new DateTimePeriod(_start, _endAnotherDay);
            _abs = new Absence();
            _agent = new Person();
            _key = new AgentDate(_agent, _start.Date);

            _personAssignment = new PersonAssignment(_agent, _scenario);

            MainShift ms = new MainShift(new ShiftCategory("Morgon"));
            ms.LayerCollection.Add(new MainShiftActivityLayer(new Activity("Telefon"), _dtp));
            ms.LayerCollection.Add(new MainShiftActivityLayer(new Activity("Tjänsteresa"), _dtpSeveralDays));

            PersonalShift ps = new PersonalShift();
            ps.LayerCollection.Add(new PersonalShiftActivityLayer(new Activity("Telefon"), _dtp));

            _personAssignment.AddPersonalShift(ps);
            _personAssignment.SetMainShift(ms);
            _agentAssignmentDictionary = new Dictionary<AgentDate, DailyAgentAssignmentCollection>();
            DailyAgentAssignmentCollection daylyAssignments = new DailyAgentAssignmentCollection();
            daylyAssignments.Add(_personAssignment);
            _agentAssignmentDictionary.Add(_key, daylyAssignments);

            _agentDayOffDictionary = new Dictionary<AgentDate, PersonDayOff>();

            _absLayer = new AbsenceLayer(_abs, _dtpSeveralDays, false);
            
            _newAbsence = new PersonAbsence(_agent, _scenario, _absLayer);
            _agentAbsDic = new Dictionary<AgentDate, DailyAgentAbsenceCollection>();
            DailyAgentAbsenceCollection dailyAbsences = new DailyAgentAbsenceCollection();
            _personAbsence = new PersonAbsence(_agent, _scenario, _absLayer);
            _absLayer2 = (AbsenceLayer) (_absLayer.Clone());
            _absLayer2.MoveLayer(new TimeSpan(1,0,0,0));
           // _newAbsence.LayerCollection.Add(_absLayer2);
            dailyAbsences.Add(_personAbsence);
            _agentAbsDic.Add(_key, dailyAbsences);

            dailyAbsences = new DailyAgentAbsenceCollection();
            _newAbsence = new PersonAbsence(_agent, _scenario, _absLayer2);
            dailyAbsences.Add(_newAbsence);
            _agentAbsDic.Add(new AgentDate(_agent, _key.Date.AddDays(1)), dailyAbsences);

            _agentDayOffDictionary.Add(new AgentDate(_agent, _key.Date), new PersonDayOff(_agent, _scenario, _anchorDateTimePeriod));

            _cache = new XCellCache(_agentAbsDic, _agentAssignmentDictionary, _agentDayOffDictionary);
            _xCell = _cache.GetXCell(_key);
        }

        [Test]
        public void CheckCanRemoveXCellFromCache()
        {
            XCell x = _xCell;
            _cache.RemoveXCell(_key);
            Assert.AreNotSame(x,_cache.GetXCell(_key));
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            Assert.IsNotNull(_xCell);
            Assert.AreSame(_agent, _xCell.Agent);
            Assert.AreEqual(_start.Date, _xCell.ScheduledDate);
        }
        [Test]
        public void VerifyThatThisCellCanAffectOtherCell()
        {
            Assert.IsTrue(_xCell.AffectsOtherCell);
        }

        [Test]
        public void AddsAgentAbsenceDisplayToList()
        {
            Assert.IsTrue(_xCell.AgentAbsenceDisplayList.Count > 0);
        }
        [Test]
        public void AddsAgentAssignmentDisplayToList()
        {
            Assert.IsTrue(_xCell.AgentAssignmentDisplayList.Count > 0);
        }
        [Test]
        public void XCellCanContainItemsInAllList()
        {
            Assert.IsTrue(_xCell.AgentAssignmentDisplayList.Count > 0);
            Assert.IsTrue(_xCell.AgentAbsenceDisplayList.Count > 0);
            Assert.IsTrue(_xCell.HasPersonalShift);
            Assert.IsNotNull(_xCell.DayOff);
        }
        [Test]
        public void AddsPersonalShiftDisplayToList()
        {
            Assert.IsTrue(_xCell.HasPersonalShift);
        }

        [Test]
        public void CanClearCache()
        {
            _cache.ClearCache();
        }
    }
}