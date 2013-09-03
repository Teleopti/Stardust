using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    /// <summary>
    /// Tests for the AgentAssignmentLayer class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-14
    /// </remarks>
    [TestFixture]
    public class AgentAssignmentLayerTest
    {
        private IPersonAssignment personAssignment;
        private DateTimePeriod period;
        private LayerType typeOfLayer;
        private IActivity activity;
        private IShiftCategory shiftCategory;
        private IPerson agent;
        private IScenario scenario;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            period = new DateTimePeriod(
                new DateTime(2007, 8, 1, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2007, 8, 1, 16, 30, 0, DateTimeKind.Utc));
            typeOfLayer = LayerType.MainShift;
            activity = ActivityFactory.CreateActivity("Phone");
            shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Unknown");
            agent = PersonFactory.CreatePerson();
            scenario = ScenarioFactory.CreateScenarioAggregate();
            personAssignment = null;
        }

        /// <summary>
        /// Verifies the new assignment is created.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        [Test]
        public void VerifyNewAssignmentIsCreated()
        {
            personAssignment = AgentAssignmentLayer.AddLayerToAssignment(agent, scenario, typeOfLayer, shiftCategory, activity, period);

            Assert.IsNotNull(personAssignment);
            Assert.AreEqual(agent, personAssignment.Person);
            Assert.AreEqual(scenario, personAssignment.Scenario);
            Assert.IsNotNull(personAssignment.MainShift);
            Assert.AreEqual(shiftCategory, personAssignment.MainShift.ShiftCategory);
            Assert.AreEqual(1, personAssignment.MainShift.LayerCollection.Count);
            Assert.AreEqual(activity, personAssignment.MainShift.LayerCollection[0].Payload);
            Assert.AreEqual(period, personAssignment.MainShift.LayerCollection[0].Period);
        }

        /// <summary>
        /// Verifies the new assignment is created with null argument as agent assignment.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        [Test]
        public void VerifyNewAssignmentIsCreatedWithNullArgumentAsAgentAssignment()
        {
            personAssignment = AgentAssignmentLayer.AddLayerToAssignment(agent, scenario, personAssignment, typeOfLayer, shiftCategory, activity, period);

            Assert.IsNotNull(personAssignment);
            Assert.AreEqual(agent, personAssignment.Person);
            Assert.AreEqual(scenario, personAssignment.Scenario);
            Assert.IsNotNull(personAssignment.MainShift);
            Assert.AreEqual(shiftCategory, personAssignment.MainShift.ShiftCategory);
            Assert.AreEqual(1, personAssignment.MainShift.LayerCollection.Count);
            Assert.AreEqual(activity, personAssignment.MainShift.LayerCollection[0].Payload);
            Assert.AreEqual(period, personAssignment.MainShift.LayerCollection[0].Period);
        }

        /// <summary>
        /// Verifies the personal shift is added to agent assignment.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        [Test]
        public void VerifyPersonalShiftIsAddedToAgentAssignment()
        {
            typeOfLayer = LayerType.PersonalShift;
            personAssignment = new PersonAssignment(agent, scenario);

            personAssignment = AgentAssignmentLayer.AddLayerToAssignment(agent, scenario, personAssignment, typeOfLayer, shiftCategory, activity, period);

            Assert.IsNotNull(personAssignment);
            Assert.AreEqual(agent, personAssignment.Person);
            Assert.AreEqual(scenario, personAssignment.Scenario);
            Assert.AreEqual(1,personAssignment.PersonalShiftCollection.Count);
            Assert.AreEqual(1, personAssignment.PersonalShiftCollection[0].LayerCollection.Count);
            Assert.AreEqual(activity, personAssignment.PersonalShiftCollection[0].LayerCollection[0].Payload);
            Assert.AreEqual(period, personAssignment.PersonalShiftCollection[0].LayerCollection[0].Period);
        }

        /// <summary>
        /// Verifies the creation of main shift without shift category gives exception.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCreationOfMainShiftWithoutShiftCategoryGivesException()
        {
            shiftCategory = null;
            
            personAssignment = AgentAssignmentLayer.AddLayerToAssignment(agent, scenario, personAssignment, typeOfLayer, shiftCategory, activity, period);
        }
    }
}
