using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Tests for AssignmentBelongsToAgentSpecification
    /// </summary>
    [TestFixture]
    public class AssignmentBelongsToAgentSpecificationTest
    {
        private List<IPersonAssignment> orgList;
        private IPerson agent1;
        private IPerson agent2;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            orgList = new List<IPersonAssignment>();
            agent1 = PersonFactory.CreatePerson();
            agent2 = PersonFactory.CreatePerson();
            orgList.Add(PersonAssignmentFactory.CreatePersonAssignment(agent1));
            orgList.Add(PersonAssignmentFactory.CreatePersonAssignment(agent2));
        }

        /// <summary>
        /// Verifies the issatisfiedby works when one agent is used.
        /// </summary>
        [Test]
        public void VerifyIsSatisfiedByWorksWhenOneAgentIsUsed()
        {
            AssignmentBelongsToAgentSpecification spec = new AssignmentBelongsToAgentSpecification(agent1);
            IList<IPersonAssignment> retList = orgList.FindAll(spec.IsSatisfiedBy);
            Assert.AreEqual(1, retList.Count);
            Assert.AreSame(agent1, retList[0].Person);
        }

        /// <summary>
        /// Verifies the verify issatisfiedby works when multiple agent is used.
        /// </summary>
        [Test]
        public void VerifyVerifyIsSatisfiedByWorksWhenMultipleAgentIsUsed()
        {
            IList<IPerson> agentList = new List<IPerson>();
            agentList.Add(agent1);
            agentList.Add(agent2);
            agentList.Add(PersonFactory.CreatePerson());
            AssignmentBelongsToAgentSpecification spec = new AssignmentBelongsToAgentSpecification(agentList);
            IList<IPersonAssignment> retList = orgList.FindAll(spec.IsSatisfiedBy);
            Assert.AreEqual(2, retList.Count);
        }

        /// <summary>
        /// Verifies the null as agentlist does nothing.
        /// </summary>
        [Test]
        public void VerifyNullAsAgentListDoesNothing()
        {
            IList<IPerson> foo = null;
            AssignmentBelongsToAgentSpecification spec = new AssignmentBelongsToAgentSpecification(foo);
            IList<IPersonAssignment> retList = orgList.FindAll(spec.IsSatisfiedBy);
            Assert.AreEqual(0, retList.Count);
        }
    }
}