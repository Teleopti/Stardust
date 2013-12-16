using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockIntradayDecisionMakerTest
	{
		private TeamBlockIntradayDecisionMaker _target;
		private MockRepository _mocks;
		private OptimizationPreferences _optimizerPreferences;
		private SchedulingOptions _schedulingOptions;
		private IScheduleResultDataExtractorProvider _dataExtractorProvider;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
			_optimizerPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_target = new TeamBlockIntradayDecisionMaker(_dataExtractorProvider);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldRecalculateStandardDeviationOfOneTeamBlock()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> {scheduleMatrixPro1 };
		    var groupMatrixList = new List<IList<IScheduleMatrixPro>> {matrixList};

			var person = PersonFactory.CreatePerson();
			var group = new Group(new List<IPerson>{person}, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
            var blockInfo1 = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
            var teamBlockInfo1 = new TeamBlockInfo(teaminfo, blockInfo1);
			var dataExtractor1 = _mocks.StrictMock<IScheduleResultDataExtractor>();
			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> periodList1 = new List<IScheduleDayPro>
				{
					scheduleDayPro1
				};
			
			using (_mocks.Record())
			{
				Expect.Call(_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro1,
				                                                                                             _schedulingOptions))
				      .Return(dataExtractor1);
				Expect.Call(dataExtractor1.Values()).Return(new List<double?> {0.2, 0.2, 0.2});
				Expect.Call(scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList1));
				Expect.Call(scheduleDayPro1.Day).Return(dateOnly).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var result = _target.RecalculateTeamBlock(teamBlockInfo1, _optimizerPreferences, _schedulingOptions);

				Assert.That(result, Is.EqualTo(teamBlockInfo1));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldDecideInADescendOrder()
		{
			var dateOnly = new DateOnly();
			var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleMatrixPro2 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1, scheduleMatrixPro2 };
			var groupMatrixList = new List<IList<IScheduleMatrixPro>> { matrixList };

			var person = PersonFactory.CreatePerson();
			var group = new Group(new List<IPerson> { person }, "Hej");
			var teaminfo = new TeamInfo(group, groupMatrixList);
			var blockInfo1 = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var blockInfo2 = new BlockInfo(new DateOnlyPeriod(dateOnly.AddDays(1), dateOnly.AddDays(1)));
			var teamBlockInfo1 = new TeamBlockInfo(teaminfo, blockInfo1);
			var teamBlockInfo2 = new TeamBlockInfo(teaminfo, blockInfo2);
			var teamBlocks = new List<ITeamBlockInfo> { teamBlockInfo1, teamBlockInfo2 };

			var dataExtractor1 = _mocks.StrictMock<IScheduleResultDataExtractor>();
			var dataExtractor2 = _mocks.StrictMock<IScheduleResultDataExtractor>();

			var scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> periodList1 = new List<IScheduleDayPro>
				{
					scheduleDayPro1
				};

			var scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			IList<IScheduleDayPro> periodList2 = new List<IScheduleDayPro>
				{
					scheduleDayPro2
				};

			using (_mocks.Record())
			{
				Expect.Call(_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro1,
																											 _schedulingOptions))
					  .Return(dataExtractor1).Repeat.Twice();
				Expect.Call(dataExtractor1.Values()).Return(new List<double?> { 0.2, 0.2, 0.2 }).Repeat.Twice();
				Expect.Call(_dataExtractorProvider.CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro2,
																											 _schedulingOptions))
					  .Return(dataExtractor2).Repeat.Twice();
				Expect.Call(dataExtractor2.Values()).Return(new List<double?> { 0.4, 0.4, 0.4 }).Repeat.Twice();
				Expect.Call(scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList1)).Repeat.Twice();
				Expect.Call(scheduleMatrixPro2.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(periodList2)).Repeat.Twice();
				Expect.Call(scheduleDayPro1.Day).Return(dateOnly).Repeat.AtLeastOnce();
				Expect.Call(scheduleDayPro2.Day).Return(dateOnly.AddDays(1)).Repeat.AtLeastOnce();
			}
			using (_mocks.Playback())
			{
				var expected = new List<ITeamBlockInfo> { teamBlockInfo2, teamBlockInfo1 };

				var result = _target.Decide(teamBlocks, _optimizerPreferences, _schedulingOptions);

				Assert.That(result, Is.EqualTo(expected));
			}
		}
	}
}
