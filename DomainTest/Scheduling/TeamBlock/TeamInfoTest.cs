using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamInfoTest
	{
		private MockRepository _mocks;
		private ITeamInfo _target;
		private Group _group;
		private IScheduleMatrixPro _matrix;
		private IScheduleMatrixPro _matrix2;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnly _date;
		private IPerson _groupMember;
	    private IPerson _person1;
		private List<IList<IScheduleMatrixPro>> _groupMatrixes;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), new DateOnly());
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), new DateOnly());
			_group = new Group(new List<IPerson> { _person1 }, "Hej");
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {_matrix, _matrix2};
			_groupMatrixes = new List<IList<IScheduleMatrixPro>>();
			_groupMatrixes.Add(matrixes);
			_target = new TeamInfo(_group, _groupMatrixes);
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
				IScheduleMatrixPro result = _target.MatrixForMemberAndDate(_groupMember, _date.AddDays(1));
				Assert.AreSame(_matrix2, result);
			}
		}
	
		[Test]
		public void ShouldGiveMeCorrectMatrixesForMemberAndPeriod()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date, _date));

				Expect.Call(_matrix2.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(_date.AddDays(1), _date.AddDays(1)));

				Expect.Call(_matrix.Person).Return(_groupMember);
				Expect.Call(_matrix2.Person).Return(_groupMember);
			}

			using (_mocks.Playback())
			{
				var result = _target.MatrixesForMemberAndPeriod(_groupMember, new DateOnlyPeriod(_date, _date.AddDays(1)));
				Assert.That(result.Count(), Is.EqualTo(2));
			}
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
		public void TwoTeamInfoWithSameTeamMembersShouldBeEqual()
		{
			Assert.That(_target.Equals(null), Is.False);
			
			Group group1 = new Group(new List<IPerson> { _person1 }, "Hej");
			var teamInfo1 = new TeamInfo(group1, _groupMatrixes);
			Assert.That(_target.Equals(teamInfo1), Is.True);

			Group groupPerson2 = new Group(new List<IPerson> { _person1 }, "Hopp");
			var teamInfo2 = new TeamInfo(groupPerson2, _groupMatrixes);
			Assert.That(teamInfo1.Equals(teamInfo2), Is.True);

			Group groupPerson3 = new Group(new List<IPerson> { _person2 }, "Hej");
			var teamInfo3 = new TeamInfo(groupPerson3, _groupMatrixes);
			Assert.That(teamInfo2.Equals(teamInfo3), Is.False);

			Group groupPerson4 = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			var teamInfo4 = new TeamInfo(groupPerson4, _groupMatrixes);
			Assert.That(teamInfo3.Equals(teamInfo4), Is.False);
		}

		[Test]
		public void ShouldTrowIfTryingToLockAPersonThatIsNotMember()
		{
			Group group1 = new Group(new List<IPerson> { _person1 }, "Hej");
			var teamInfo1 = new TeamInfo(group1, _groupMatrixes);
			Assert.Throws<ArgumentOutOfRangeException>(() => teamInfo1.LockMember(new DateOnlyPeriod(_date, _date),  _person2));
		}

		[Test]
		public void ShouldExposeUnLockedMembers()
		{
			Group groupPerson4 = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			var teamInfo4 = new TeamInfo(groupPerson4, _groupMatrixes);
			teamInfo4.LockMember(new DateOnlyPeriod(_date, _date), _person1);
			var result = teamInfo4.UnLockedMembers(_date);
			Assert.AreEqual(_person2, result.First());
			Assert.AreEqual(1, result.Count());
		}

		[Test]
		public void LockedShouldBeCleared()
		{
			Group groupPerson4 = new Group(new List<IPerson> { _person1, _person2 }, "Hej");
			var teamInfo4 = new TeamInfo(groupPerson4, _groupMatrixes);
			teamInfo4.LockMember(new DateOnlyPeriod(_date, _date), _person1);
			Assert.AreEqual(1, teamInfo4.UnLockedMembers(_date).Count());

			teamInfo4.ClearLocks();
			Assert.AreEqual(2, teamInfo4.UnLockedMembers(_date).Count());
		}
	}
}