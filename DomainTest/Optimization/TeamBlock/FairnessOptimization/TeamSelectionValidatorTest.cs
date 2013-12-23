using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamSelectionValidatorTest
	{
		private MockRepository _mock;
		private TeamSelectionValidator _target;
		private ITeamInfoFactory _teamInfoFactory;
		private IList<IPerson> _selectedPersonList;
		private IList<IScheduleMatrixPro> _scheduleMatrixList;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private DateOnlyPeriod _dateOnlyPeriod;
			
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_person1 = PersonFactory.CreatePerson("Person1", "Person1");
			_person2 = PersonFactory.CreatePerson("Person2", "Person2");
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_selectedPersonList = new List<IPerson>();
			_scheduleMatrixList = new List<IScheduleMatrixPro>{_scheduleMatrixPro1, _scheduleMatrixPro2};
			_dateOnly = new DateOnly(2013, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamInfo2 = _mock.StrictMock<ITeamInfo>();
			_target = new TeamSelectionValidator(_teamInfoFactory, _scheduleMatrixList);
		}

		[Test]
		public void ShouldReturnTrueIfAllInTeamIsSelected()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person1, _dateOnly, _scheduleMatrixList)).Return(_teamInfo1);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person2, _dateOnly, _scheduleMatrixList)).Return(_teamInfo2);
				Expect.Call(_teamInfo1.GroupMembers).Return(new List<IPerson> { _person1 });
				Expect.Call(_teamInfo2.GroupMembers).Return(new List<IPerson> { _person2 });
			}

			using (_mock.Playback())
			{
				_selectedPersonList.Add(_person1);
				_selectedPersonList.Add(_person2);
				var result = _target.ValidateSelection(_selectedPersonList, _dateOnlyPeriod);

				Assert.IsTrue(result);	
			}	
		}

		[Test]
		public void ShouldReturnFalseIfNotAllInTeamIsSelected()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person1, _dateOnly, _scheduleMatrixList)).Return(_teamInfo1);
				Expect.Call(_teamInfo1.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mock.Playback())
			{
				_selectedPersonList.Add(_person1);
				var result = _target.ValidateSelection(_selectedPersonList, _dateOnlyPeriod);

				Assert.IsFalse(result);
			}		
		}

		[Test]
		public void ShouldReturnFalseIfSelectedPersonListIsNull()
		{
			var result = _target.ValidateSelection(null, _dateOnlyPeriod);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfSelectedPersonListIsEmpty()
		{
			var result = _target.ValidateSelection(new List<IPerson>(), _dateOnlyPeriod);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfPersonNotInTeam()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person1, _dateOnly, _scheduleMatrixList)).Return(null);
			}

			using (_mock.Playback())
			{
				_selectedPersonList.Add(_person1);
				var result = _target.ValidateSelection(_selectedPersonList, _dateOnlyPeriod);

				Assert.IsFalse(result);
			}	
		}
	}
}
