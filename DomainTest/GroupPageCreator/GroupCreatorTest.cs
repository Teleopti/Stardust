using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.GroupPageCreator
{
	[TestFixture]
	public class GroupCreatorTest
	{
		private IGroupCreator _target;

		[SetUp]
		public void Setup()
		{
			_target = new GroupCreator();
		}

		[Test]
		public void ShouldReturnCorrectGroupForAgentInRootGroup()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var groupPageForDate = new GroupPage("page1");
			var rootPersonGroup = new RootPersonGroup("g1");
			rootPersonGroup.AddPerson(person1);
			rootPersonGroup.AddPerson(person2);
			groupPageForDate.AddRootPersonGroup(rootPersonGroup);
			var allPermittedPersons = new HashSet<IPerson>{person1, person2};

			var result = _target.CreateGroupForPerson(person1, groupPageForDate, allPermittedPersons);
			Assert.That(result.Name == "g1");
			Assert.That(result.GroupMembers.ToList().Contains(person1));
			Assert.That(result.GroupMembers.ToList().Contains(person2));
		}

		[Test]
		public void AgentNotInGroupShouldBeReturnedInSingleAgentGroup()
		{
			var person1 = PersonFactory.CreatePerson("p1");
			var groupPageForDate = new GroupPage("page1");
			var allPermittedPersons = new HashSet<IPerson> { person1 };

			var result = _target.CreateGroupForPerson(person1, groupPageForDate, allPermittedPersons);
			Assert.That(result.Name == "p1 p1");
			Assert.That(result.GroupMembers.ToList().Contains(person1));
		}

		[Test]
		public void AgentNotInGroupShouldNotBeReturnedIfNotPermitted()
		{
			var person1 = PersonFactory.CreatePerson("p1");
			var groupPageForDate = new GroupPage("page1");
			var allPermittedPersons = new HashSet<IPerson> ();

			var result = _target.CreateGroupForPerson(person1, groupPageForDate, allPermittedPersons);
			Assert.IsNull(result);
		}

		[Test]
		public void AgentInGroupShouldNotBeReturnedIfNotPermitted()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var groupPageForDate = new GroupPage("page1");
			var rootPersonGroup = new RootPersonGroup("g1");
			rootPersonGroup.AddPerson(person1);
			rootPersonGroup.AddPerson(person2);
			groupPageForDate.AddRootPersonGroup(rootPersonGroup);
			var allPermittedPersons = new HashSet<IPerson> { person2 };

			var result = _target.CreateGroupForPerson(person1, groupPageForDate, allPermittedPersons);
			Assert.That(result.Name == "g1");
			Assert.That(!result.GroupMembers.ToList().Contains(person1));
			Assert.That(result.GroupMembers.ToList().Contains(person2));
		}

	}
}