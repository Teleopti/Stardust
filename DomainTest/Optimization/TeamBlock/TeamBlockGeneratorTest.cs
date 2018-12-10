using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockGeneratorTest
	{
		private ITeamBlockGenerator _target;
		private MockRepository _mocks;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamInfoFactory = _mocks.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
			_target = new TeamBlockGenerator(_teamInfoFactory, _teamBlockInfoFactory, new BlockPreferencesMapper());
		}

		[Test]
		public void ShouldGenerateTeamBlocks()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> {person};
			var schedulingOptions = new SchedulingOptions();
			 var groupMatrixList = new List<IList<IScheduleMatrixPro>> {matrixes};
			var group = new Group(new List<IPerson>{person}, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
            var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
            var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, person, selectedPeriod, matrixes)).Return(teaminfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teaminfo, dateOnly,
																	  schedulingOptions.BlockFinder())).IgnoreArguments()
				      .Return(teamBlockInfo);
			}
			using (_mocks.Playback())
			{
				var expected = new List<ITeamBlockInfo> {teamBlockInfo};

				var result = _target.Generate(null, matrixes, selectedPeriod, persons, schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}

		}

		[Test]
		public void ShouldNotGenerateTeamBlocksIfTeamIsEmpty()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> {matrix1, matrix2};
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> {person};
			var schedulingOptions = new SchedulingOptions();
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, person, selectedPeriod, matrixes)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Generate(null, matrixes, selectedPeriod, persons, schedulingOptions);

				Assert.That(result.Count, Is.EqualTo(0));
			}

		}

		[Test]
		public void ShouldNotAddAnEmptyTeamBlock()
		{
			var dateOnly = new DateOnly();
			var matrix1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrix2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { matrix1, matrix2 };
			var selectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var person = PersonFactory.CreatePerson("Bill");
			var persons = new List<IPerson> { person };
			var schedulingOptions = new SchedulingOptions();
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixes };
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, person, selectedPeriod, matrixes)).Return(teaminfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teaminfo, dateOnly,
																	  schedulingOptions.BlockFinder())).IgnoreArguments()
						.Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Generate(null, matrixes, selectedPeriod, persons, schedulingOptions);

				Assert.That(result.Count, Is.EqualTo(0));
			}

		}
	}
}
