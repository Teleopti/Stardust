using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateTest
	{
		private TeamSteadyState _target;
		private ITeamSteadyStateSchedulePeriod _teamSteadyStateSchedulePeriod;
		private ITeamSteadyStatePersonPeriod _teamSteadyStatePersonPeriod;
		private MockRepository _mocks;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IPersonPeriod _personPeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamSteadyStatePersonPeriod = _mocks.StrictMock<ITeamSteadyStatePersonPeriod>();
			_teamSteadyStateSchedulePeriod = _mocks.StrictMock<ITeamSteadyStateSchedulePeriod>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
			_target = new TeamSteadyState(_teamSteadyStatePersonPeriod, _teamSteadyStateSchedulePeriod);	
		}

		[Test]
		public void ShouldNotBeInSteadyStateWhenDifferentSchedulePeriods()
		{
			using(_mocks.Record())
			{
				Expect.Call(_teamSteadyStateSchedulePeriod.SchedulePeriodEquals(_virtualSchedulePeriod, _scheduleMatrixPro)).Return(false);
			}

			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.SteadyState(_virtualSchedulePeriod, _scheduleMatrixPro, _personPeriod));
			}
		}

		[Test]
		public void ShouldNotBeInSteadyStateWhenDifferentPersonPeriods()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamSteadyStateSchedulePeriod.SchedulePeriodEquals(_virtualSchedulePeriod, _scheduleMatrixPro)).Return(true);
				Expect.Call(_teamSteadyStatePersonPeriod.PersonPeriodEquals(_personPeriod)).Return(false);
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.SteadyState(_virtualSchedulePeriod, _scheduleMatrixPro, _personPeriod));
			}	
		}

		[Test]
		public void ShouldBeInSteadyStateWhenSameTargetValues()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamSteadyStateSchedulePeriod.SchedulePeriodEquals(_virtualSchedulePeriod, _scheduleMatrixPro)).Return(true);
				Expect.Call(_teamSteadyStatePersonPeriod.PersonPeriodEquals(_personPeriod)).Return(true);
			}

			using (_mocks.Playback())
			{
				Assert.IsTrue(_target.SteadyState(_virtualSchedulePeriod, _scheduleMatrixPro, _personPeriod));
			}	
		}
	}
}
