using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class AllTeamMembersInSelectionSpecificationTest
	{
		private IAllTeamMembersInSelectionSpecification _target;
		private ITeamInfo  _teamInfo;
		private MockRepository _mock;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPerson _person4;

		[SetUp]
		public void Setup()
		{
			_target = new AllTeamMembersInSelectionSpecification();
			_mock = new MockRepository();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_person1 = PersonFactory.CreatePerson("test1");
			_person2 = PersonFactory.CreatePerson("test2");
			_person3 = PersonFactory.CreatePerson("test3");
			_person4 = PersonFactory.CreatePerson("test4");
		}

		[Test]
		public void ReturnFalseIfNoPersonIsSelected()
		{
			var selectedPersons = new List<IPerson>() {_person1, _person2};
			var groupMembers = new List<IPerson>() {_person3, _person4};
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.GroupMembers).Return(groupMembers);
			}
			Assert.IsFalse(_target.IsSatifyBy(_teamInfo,selectedPersons));
		}

		[Test]
		public void ReturnFalseIfOnePersonIsSelected()
		{
			var selectedPersons = new List<IPerson>() { _person1, _person2 , _person3  };
			var groupMembers = new List<IPerson>() { _person3, _person4 };
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.GroupMembers).Return(groupMembers);
			}
			Assert.IsFalse(_target.IsSatifyBy(_teamInfo, selectedPersons));
		}

		[Test]
		public void ReturnTrueIfAllPersonIsSelected()
		{
			var selectedPersons = new List<IPerson>() { _person1, _person2, _person3,_person4  };
			var groupMembers = new List<IPerson>() { _person3, _person4 };
			using (_mock.Record())
			{
				Expect.Call(_teamInfo.GroupMembers).Return(groupMembers);
			}
			Assert.IsTrue( _target.IsSatifyBy(_teamInfo, selectedPersons));
		}
	}

	
}
