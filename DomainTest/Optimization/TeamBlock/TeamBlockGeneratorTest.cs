using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockGeneratorTest
	{
		private ITeamBlockGenerator _target;
		private MockRepository _mocks;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_teamInfoFactory = _mocks.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mocks.StrictMock<ITeamBlockInfoFactory>();
            _teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			_target = new TeamBlockGenerator(_teamInfoFactory, _teamBlockInfoFactory,_teamBlockSchedulingOptions);
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
			var groupPerson = new GroupPerson(new List<IPerson>{person}, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
            var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
            var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(person, selectedPeriod, matrixes)).Return(teaminfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teaminfo, dateOnly,
																	  schedulingOptions.BlockFinderTypeForAdvanceScheduling, false, matrixes))
				      .Return(teamBlockInfo);
			}
			using (_mocks.Playback())
			{
				var expected = new List<ITeamBlockInfo> {teamBlockInfo};

				var result = _target.Generate(matrixes, selectedPeriod, persons, schedulingOptions);

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
				Expect.Call(_teamInfoFactory.CreateTeamInfo(person, selectedPeriod, matrixes)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Generate(matrixes, selectedPeriod, persons, schedulingOptions);

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
			var groupPerson = new GroupPerson(new List<IPerson> { person }, DateOnly.MinValue, "Hej", null);
			var teaminfo = new TeamInfo(groupPerson, groupMatrixList);
			using (_mocks.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(person, selectedPeriod, matrixes)).Return(teaminfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(teaminfo, dateOnly,
																	  schedulingOptions.BlockFinderTypeForAdvanceScheduling, false, matrixes))
					  .Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Generate(matrixes, selectedPeriod, persons, schedulingOptions);

				Assert.That(result.Count, Is.EqualTo(0));
			}

		}
	}
}
