using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
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

			using (_mocks.Record())
			{
				Expect.Call(_groupPerson.Id).Return(Guid.NewGuid());
			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(_groupPerson.GetHashCode(), _target.GetHashCode());
			}
      
      
		}
	}
}