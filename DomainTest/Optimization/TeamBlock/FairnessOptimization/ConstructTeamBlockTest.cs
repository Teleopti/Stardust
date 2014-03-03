using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class ConstructTeamBlockTest
	{
		private MockRepository _mock;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person;
		private IList<IPerson> _persons;
		private ISchedulingOptions _schedulingOptions;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private IGroupPageLight _groupPageLight;
		private ConstructTeamBlock _target;
	
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			_person = PersonFactory.CreatePerson("Person");
			_persons = new List<IPerson>{_person};
			_schedulingOptions = _mock.StrictMock<ISchedulingOptions>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_groupPageLight = _mock.StrictMock<IGroupPageLight>();
			_target = new ConstructTeamBlock(_teamInfoFactory, _teamBlockInfoFactory);	
		}

		[Test]
		public void ShouldConstruct()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_schedulingOptions.GroupOnGroupPageForTeamBlockPer).Return(_groupPageLight).Repeat.AtLeastOnce();
				Expect.Call(_groupPageLight.Key).Return("SingleAgentTeam");
				Expect.Call(_schedulingOptions.UseTeamBlockPerOption).Return(false);
				Expect.Call(_schedulingOptions.BlockFinderTypeForAdvanceScheduling).Return(BlockFinderType.SingleDay);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, BlockFinderType.SingleDay, true, _scheduleMatrixPros)).Return(_teamBlockInfo);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions.UseTeamBlockPerOption,
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(_teamBlockInfo, result[0]);
			}
		}

		[Test]
		public void ShouldConstructForTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _scheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_schedulingOptions.GroupOnGroupPageForTeamBlockPer).Return(_groupPageLight).Repeat.AtLeastOnce();
				Expect.Call(_groupPageLight.Key).Return("SingleAgentTeam");
				Expect.Call(_schedulingOptions.UseTeamBlockPerOption).Return(true);
				Expect.Call(_schedulingOptions.BlockFinderTypeForAdvanceScheduling).Return(BlockFinderType.BetweenDayOff);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, BlockFinderType.BetweenDayOff, true, _scheduleMatrixPros)).Return(_teamBlockInfo);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_scheduleMatrixPros, _dateOnlyPeriod, _persons, _schedulingOptions.UseTeamBlockPerOption,
																 _schedulingOptions.BlockFinderTypeForAdvanceScheduling,
																 _schedulingOptions.GroupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(_teamBlockInfo, result[0]);
			}
		}
	}
}
