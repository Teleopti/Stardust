using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamInfoTest
	{
		private MockRepository _mocks;
		private ITeamInfo _target;
		private IGroupPerson _groupPerson;
		private IScheduleMatrixPro _matrix;
		private IScheduleMatrixPro _matrix2;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnly _date;
		private IPerson _groupMember;
	    private IPerson _person1;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPerson = _mocks.StrictMock<IGroupPerson>();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {_matrix, _matrix2};
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>>();
			groupMatrixes.Add(matrixes);
			_target = new TeamInfo(_groupPerson, groupMatrixes);
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_date = new DateOnly(2013, 03, 11);
			_groupMember = PersonFactory.CreatePerson("kalle");
		}

		[Test]
		public void ShouldGiveMeTheCorrectMatrixesForGroupOnAPeriod()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));

				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));

				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));

				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));
			}

			using (_mocks.Playback())
			{
				var result = _target.MatrixesForGroupAndPeriod(new DateOnlyPeriod(_date, _date)).ToList();
				Assert.AreEqual(1, result.Count());
				Assert.AreSame(_matrix, result.First());
				result = _target.MatrixesForGroupAndPeriod(new DateOnlyPeriod(_date, _date.AddDays(1))).ToList();
				Assert.AreEqual(2, result.Count());
				Assert.IsTrue(result.Contains(_matrix));
				Assert.IsTrue(result.Contains(_matrix2));
			}
		}

		[Test]
		public void ShouldGiveMeTheCorrectMatrixesForGroupOnADate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));

				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));
			}

			using (_mocks.Playback())
			{
				var result = _target.MatrixesForGroupAndDate(_date.AddDays(1)).ToList();
				Assert.AreEqual(1, result.Count());
				Assert.AreSame(_matrix2, result.First());
			}
		}

		[Test]
		public void ShouldGiveMeCorrectMatrixForMemberAndDate()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));

				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));

				Expect.Call(_matrix2.Person).Return(_groupMember);
			}

			using (_mocks.Playback())
			{
				IScheduleMatrixPro result = _target.MatrixesForMemberAndDate(_groupMember, _date.AddDays(1));
				Assert.AreSame(_matrix2, result);
			}
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
			_person1 = PersonFactory.CreatePerson();
            IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			_target = new TeamInfo(groupPerson1, new List<IList<IScheduleMatrixPro>>());

			Assert.AreEqual(groupPerson1.GetHashCode(), _target.GetHashCode());
		}

		[Test]
		public void EqualsShouldWorkWithDifferentGroupPersonsOnSameDate()
		{
			_person1 = PersonFactory.CreatePerson();
            IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
            IGroupPerson groupPerson2 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			
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
			_person1 = PersonFactory.CreatePerson();
			IPerson person2 = PersonFactory.CreatePerson();
			Guid sameGuid = Guid.NewGuid();
            IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", sameGuid);
            IGroupPerson groupPerson2 = new GroupPerson(new List<IPerson> { _person1, person2 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
            IGroupPerson groupPerson3 = new GroupPerson(new List<IPerson> { _person1, person2 }, DateOnly.MinValue, "Hej", sameGuid);

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