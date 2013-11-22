using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for the AgentAssignmentMapper
    /// </summary>
    [TestFixture]
    public class AgentAssignmentMapperTest : MapperTest<global::Domain.AgentDay>
    {
        private AgentDayFactory _agdFactory;
        private global::Domain.AgentDay _agDay;
        private MappedObjectPair mappedObjectPair;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 13; }
        }

        /// <summary>
        /// Creates the agent day.
        /// </summary>
        [SetUp]
        public void CreateAgentDay()
        {
            _agdFactory = new AgentDayFactory();
            _agDay = _agdFactory.AgentDay();
            mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.Activity = _agdFactory.ActPairList;
            mappedObjectPair.Agent = _agdFactory.AgentPairList;
            mappedObjectPair.ShiftCategory = _agdFactory.ShiftCatPairList;
            mappedObjectPair.Scenario = _agdFactory.ScenarioPairList;
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment].
        /// </summary>
        [Test]
        public void CanMapAgentBaseShiftAssignment()
        {
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.WorkShift(), _agdFactory.ScheduleType);
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
            Assert.IsNotNull(newAgAss.ShiftCategory);
            Assert.IsNotNull(newAgAss.Scenario);
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment].
        /// </summary>
        [Test]
        public void CanMapAgentBaseShiftAssignmentWithAbsence()
        {
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.WorkShift(), _agdFactory.ScheduleType);
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.Absence("Semester", "SE", false),
                                                  new global::Domain.SchedType(-1, "Web", true, false, false));
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
						Assert.IsNotNull(newAgAss.ShiftCategory);
            Assert.IsNotNull(newAgAss.Scenario);
        }

        [Test]
        public void CanMapAgentBaseShiftAssignmentWithDayOffAbsence()
        {
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.WorkShift(), _agdFactory.ScheduleType);
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.Absence("Day Off", "DO", true),
                                                  new global::Domain.SchedType(-1, "Web", true, false, false));
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
			ObjectPairCollection<global::Domain.Absence, IDayOffTemplate> dayOffPairList = new ObjectPairCollection<global::Domain.Absence, IDayOffTemplate>();

			dayOffPairList.Add(_agDay.AgentDayAssignment.Assigned.AssignedAbsence, new DayOffTemplate(new Description("test DO")));
			mappedObjectPair.DayOff = dayOffPairList;

            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss);
			Assert.AreEqual("test DO", newAgAss.DayOff().Description.Name);
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment].
        /// </summary>
        [Test]
        public void CanMapAgentBaseShiftAssignmentWithAbsenceAndOvertime()
        {
            SetOvertime();
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.WorkShift(), _agdFactory.ScheduleType);
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.Absence("Semester", "SE", false),
                                                  new global::Domain.SchedType(-1, "Web", true, false, false));
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
						Assert.IsNotNull(newAgAss.ShiftCategory);
            Assert.IsNotNull(newAgAss.Scenario);
            Assert.AreEqual(1, newAgAss.OvertimeActivities().Count());
        }

        /// <summary>
        /// Determines whether this instance [can map agent personal assignments].
        /// </summary>
        [Test]
        public void CanMapAgentPersonalAssignment()
        {
	        var fillup = _agdFactory.FillUpShift();
            _agDay.AgentDayAssignment.AddFillup(fillup, _agdFactory.ScheduleType);
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
            Assert.AreEqual(fillup.LayerCollection.Count, newAgAss.PersonalActivities().Count());
        }

        /// <summary>
        /// Determines whether this instance [can map agent base shift assignment with overtime].
        /// </summary>
        [Test]
        public void CanMapAgentBaseShiftAssignmentWithOvertime()
        {
            SetOvertime();
            _agDay.AgentDayAssignment.SetAssigned(_agdFactory.WorkShift(), _agdFactory.ScheduleType);
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
						Assert.IsNotNull(newAgAss.ShiftCategory);
            Assert.IsNotNull(newAgAss.Scenario);
            Assert.AreEqual(1,newAgAss.OvertimeActivities().Count());
        }

        private void SetOvertime()
        {
            var activity = _agdFactory.ActPairList.Obj1Collection().Last();
            global::Domain.Overtime overtime = new global::Domain.Overtime(activity.Id, "Overtime", activity.ColorLayout, false, false, false, activity.Id);

            mappedObjectPair.OvertimeActivity.Add(activity, overtime);
            mappedObjectPair.OvertimeUnderlyingActivity.Add(activity, activity);
            mappedObjectPair.MultiplicatorDefinitionSet = new ObjectPairCollection<global::Domain.Overtime, IMultiplicatorDefinitionSet>();
            mappedObjectPair.MultiplicatorDefinitionSet.Add(overtime, new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime));
        }

        /// <summary>
        /// Determines whether this instance [can map agent personal assignments with overtime].
        /// </summary>
        [Test]
        public void CanMapAgentPersonalAssignmentWithOvertime()
        {
					//ett övertidslager och två personalshiftlager
            SetOvertime();
	        var fillup = _agdFactory.FillUpShift();
            _agDay.AgentDayAssignment.AddFillup(fillup, _agdFactory.ScheduleType);
            AgentAssignmentMapper agAssMapper = new AgentAssignmentMapper(mappedObjectPair, (TimeZoneInfo.Utc));
            IPersonAssignment newAgAss = agAssMapper.Map(_agDay);
            Assert.IsNotNull(newAgAss.Person);
            Assert.AreEqual(2, newAgAss.PersonalActivities().Count());
            Assert.AreEqual(1, newAgAss.OvertimeActivities().Count());
        }
    }
}