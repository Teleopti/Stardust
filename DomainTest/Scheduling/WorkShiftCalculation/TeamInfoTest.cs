using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class TeamInfoTest
	{
		private MockRepository _mocks;
		private ITeamInfo _target;
		private IGroupPerson _groupPerson;
		private IScheduleMatrixPro _matrix;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {_matrix, _matrix};
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>>();
			groupMatrixes.Add(matrixes);
				_target = new TeamInfo(_groupPerson, groupMatrixes);
		}

		[Test]
		public void ShouldContainGroupPerson()
		{
			IGroupPerson groupPerson = _target.GroupPerson;
			Assert.AreSame(_groupPerson, groupPerson);
		}

		[Test]
		public void ShouldContainMatrixListForAllGroupMembers()
		{
			Assert.AreEqual(2, _target.MatrixesForGroup().Count());
		}

		[Test]
		public void ShouldContainMatrixesForOneGroupMember()
		{
			Assert.AreEqual(2, _target.MatrixesForGroupMember(0).Count());
		}

		[Test]
		public void ShouldReturnSameHashAsContainedGroupPersonAndStartDateForTheTeam()
		{
			IPerson person = PersonFactory.CreatePerson();
			IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			_target = new TeamInfo(groupPerson1, new List<IList<IScheduleMatrixPro>>());

			Assert.AreEqual(groupPerson1.GetHashCode(), _target.GetHashCode());
		}

		[Test]
		public void EqualsShouldWorkWithDifferentGroupPersonsOnSameDate()
		{
			IPerson person = PersonFactory.CreatePerson();
			IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			IGroupPerson groupPerson2 = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			
			Assert.IsFalse(groupPerson1.Equals(groupPerson2));
			Assert.IsTrue(groupPerson1.Equals(groupPerson1));

			HashSet<IGroupPerson> list = new HashSet<IGroupPerson>();
			list.Add(groupPerson1);
			list.Add(groupPerson1);
			Assert.AreEqual(1, list.Count);

			list.Add(groupPerson2);
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		public void EqualsShouldWorkWithSameGroupPersonsButDifferentMembers()
		{
			IPerson person1 = PersonFactory.CreatePerson();
			IPerson person2 = PersonFactory.CreatePerson();
			Guid sameGuid = Guid.NewGuid();
			IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { person1 }, DateOnly.MinValue, "Hej", sameGuid);
			IGroupPerson groupPerson2 = new GroupPerson(new List<IPerson> { person1, person2 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			IGroupPerson groupPerson3 = new GroupPerson(new List<IPerson> { person1, person2 }, DateOnly.MinValue, "Hej", sameGuid);

			Assert.IsFalse(groupPerson1.Equals(groupPerson2));
			Assert.IsTrue(groupPerson1.Equals(groupPerson3));

			HashSet<IGroupPerson> list = new HashSet<IGroupPerson>();
			list.Add(groupPerson1);
			list.Add(groupPerson1);
			Assert.AreEqual(1, list.Count);

			list.Add(groupPerson2);
			Assert.AreEqual(2, list.Count);

			list.Add(groupPerson3);
			Assert.AreEqual(2, list.Count);
		}
	}
}