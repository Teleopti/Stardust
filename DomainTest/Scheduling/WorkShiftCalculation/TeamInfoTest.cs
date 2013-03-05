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
			_target = new TeamInfo(_groupPerson, new List<IScheduleMatrixPro> {_matrix});
		}

		[Test]
		public void ShouldContainGroupPerson()
		{
			IGroupPerson groupPerson = _target.GroupPerson;
			Assert.AreSame(_groupPerson, groupPerson);
		}

		[Test]
		public void ShouldContainMatrixist()
		{
			IScheduleMatrixPro matrix = _target.MatrixesForGroup.FirstOrDefault();
			Assert.AreSame(_matrix, matrix);
		}

		[Test]
		public void ShouldReturnSameHashAsContainedGroupPerson()
		{
			IPerson person = PersonFactory.CreatePerson();
			IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			_target = new TeamInfo(groupPerson1, new List<IScheduleMatrixPro>());

			Assert.AreEqual(groupPerson1.GetHashCode(), _target.GetHashCode());

		}

		[Test]
		public void EqualsShouldWork()
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
	}
}