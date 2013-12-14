using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
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
		private List<IList<IScheduleMatrixPro>> _groupMatrixes;
		private Guid _groupPersonId;
		private IPerson _person2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPersonId = Guid.NewGuid();
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), new DateOnly());
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), new DateOnly());
			_groupPerson = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", _groupPersonId);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {_matrix, _matrix2};
			_groupMatrixes = new List<IList<IScheduleMatrixPro>>();
			_groupMatrixes.Add(matrixes);
			_target = new TeamInfo(_groupPerson, _groupMatrixes);
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
		public void EqualsShouldWorkWithSameGroupPersonsButDifferentMembers()
		{
			
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

		[Test]
		public void TwoTeamInfoWithSameTeamMembersShouldBeEqual()
		{
			Assert.That(_target.Equals(null), Is.False);
			
			IGroupPerson groupPerson1 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			var teamInfo1 = new TeamInfo(groupPerson1, _groupMatrixes);
			Assert.That(_target.Equals(teamInfo1), Is.True);

			IGroupPerson groupPerson2 = new GroupPerson(new List<IPerson> { _person1 }, DateOnly.MinValue, "Hopp", null);
			var teamInfo2 = new TeamInfo(groupPerson2, _groupMatrixes);
			Assert.That(teamInfo1.Equals(teamInfo2), Is.True);

			IGroupPerson groupPerson3 = new GroupPerson(new List<IPerson> { _person2 }, DateOnly.MaxValue, "Hej", _groupPersonId);
			var teamInfo3 = new TeamInfo(groupPerson3, _groupMatrixes);
			Assert.That(teamInfo2.Equals(teamInfo3), Is.False);

			IGroupPerson groupPerson4 = new GroupPerson(new List<IPerson> { _person1, _person2 }, DateOnly.MaxValue, "Hej", _groupPersonId);
			var teamInfo4 = new TeamInfo(groupPerson4, _groupMatrixes);
			Assert.That(teamInfo3.Equals(teamInfo4), Is.False);
		}

	}
}