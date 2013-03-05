using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class BlockProviderTest
	{
		private BlockProvider _target;
		private MockRepository _mocks;
		private ITeamInfoCreator _teamInfoCreator;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamInfoCreator = _mocks.StrictMock<ITeamInfoCreator>();
			_teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
			_target = new BlockProvider(_teamInfoCreator, _teamBlockInfoFactory);
		}

		[Test]
		public void ShouldProvideBlocksOfTwoSingleDayBlocks()
		{
			var date = new DateOnly();
			var selectedPeriod = new DateOnlyPeriod(date, date.AddDays(1));
			var person = PersonFactory.CreatePerson("bill");
			var groupPerson1 = _mocks.StrictMock<IGroupPerson>();
			var groupPerson2 = _mocks.StrictMock<IGroupPerson>();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var schedulingOptions = new SchedulingOptions { BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay };
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var teamInfo1 = new TeamInfo(groupPerson1, new List<IScheduleMatrixPro> {matrix1});
			var teamInfo2 = new TeamInfo(groupPerson2, new List<IScheduleMatrixPro> {matrix2});
			var blockInfo1 = new BlockInfo(new DateOnlyPeriod(date, date));
			var blockInfo2 = new BlockInfo(new DateOnlyPeriod(date.AddDays(1), date.AddDays(1)));
			var teamBlockInfo1 = new TeamBlockInfo(teamInfo1, blockInfo1);
			var teamBlockInfo2 = new TeamBlockInfo(teamInfo2, blockInfo2);
			using (_mocks.Record())
			{
				Expect.Call(groupPerson1.Id).Return(Guid.NewGuid());
				Expect.Call(groupPerson2.Id).Return(Guid.NewGuid());
				Expect.Call(_teamInfoCreator.CreateTeamInfo(person, date, matrixes)).Return(teamInfo1);
				Expect.Call(_teamInfoCreator.CreateTeamInfo(person, date.AddDays(1), matrixes)).Return(teamInfo2);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo1, date, BlockFinderType.SingleDay)).Return(teamBlockInfo1);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo2, date.AddDays(1), BlockFinderType.SingleDay)).Return(teamBlockInfo2);
			}
			using (_mocks.Playback())
			{
				var result = _target.Provide(selectedPeriod, new List<IPerson> {person}, matrixes, schedulingOptions);

				Assert.That(result.Count, Is.EqualTo(2));
			}
		}
	}
}
