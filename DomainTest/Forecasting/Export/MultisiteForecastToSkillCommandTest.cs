using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
	[TestFixture]
	public class MultisiteForecastToSkillCommandTest
	{
		private IMultisiteSkill multisiteSkill;
		private IChildSkill childSkill;
		private ISkill targetSkill;
		private MultisiteForecastToSkillAnalyzer target;
		private MockRepository mocks;
		private IJobResultFeedback jobResultFeedback;
		private ISplitImportForecastMessage _splitImportForecastMessage;
		private ISkillDayLoadHelper skillDayLoadHelper;
		private IScenarioRepository scenarioRepository;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			multisiteSkill = SkillFactory.CreateMultisiteSkill("test");
			childSkill = SkillFactory.CreateChildSkill("ChildSkill1", multisiteSkill);
			targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

			jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
			_splitImportForecastMessage = mocks.DynamicMock<ISplitImportForecastMessage>();
			skillDayLoadHelper = mocks.DynamicMock<ISkillDayLoadHelper>();
			scenarioRepository = mocks.DynamicMock<IScenarioRepository>();

			target = new MultisiteForecastToSkillAnalyzer(skillDayLoadHelper, scenarioRepository, jobResultFeedback, _splitImportForecastMessage);
		}

		[Test]
		public void ShouldCopyForecastFromMultisiteSubSkillToSkill()
		{
			var multisiteSkillForExport = new MultisiteSkillForExport { MultisiteSkill = multisiteSkill };
			multisiteSkillForExport.AddSubSkillMapping(new SkillExportCombination
			{ SourceSkill = childSkill, TargetSkill = targetSkill });
			var selection = new SkillExportSelection(new[] { multisiteSkillForExport });
			selection.Period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
			var sourceScenario = ScenarioFactory.CreateScenarioAggregate();
			var childSkillDay = mocks.DynamicMock<ISkillDay>();
			var skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(targetSkill,
				new DateTime(2016, 06, 27, 11, 40, 0, DateTimeKind.Utc), 50, 20, 10);

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario(multisiteSkill.GetOrFillWithBusinessUnit_DONTUSE())).Return(sourceScenario);
				Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] { multisiteSkill },
																	  sourceScenario)).Return(
																		  new Dictionary<ISkill, IEnumerable<ISkillDay>> { { childSkill, new[] { childSkillDay } } });
				var forecastcastRow = new[] {new ForecastsRow()};
				Expect.Call(() => _splitImportForecastMessage.Process(forecastcastRow, targetSkill, selection.Period)).IgnoreArguments();
				Expect.Call(childSkillDay.SkillStaffPeriodCollection).Return(new[] { skillStaffPeriod });
			}
			using (mocks.Playback())
			{
				target.Execute(selection);
			}
		}

		[Test]
		public void ShouldWarnWhenTargetSkillIsMissingWorkloads()
		{
			var multisiteSkillForExport = new MultisiteSkillForExport { MultisiteSkill = multisiteSkill };
			multisiteSkillForExport.AddSubSkillMapping(new SkillExportCombination { SourceSkill = childSkill, TargetSkill = targetSkill });
			var selection = new SkillExportSelection(new[] { multisiteSkillForExport });
			selection.Period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
			var sourceScenario = ScenarioFactory.CreateScenarioAggregate();
			var childSkillDay = mocks.DynamicMock<ISkillDay>();

			targetSkill.RemoveWorkload(targetSkill.WorkloadCollection.First());

			using (mocks.Record())
			{
				Expect.Call(scenarioRepository.LoadDefaultScenario(multisiteSkill.GetOrFillWithBusinessUnit_DONTUSE())).Return(sourceScenario);
				Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] { multisiteSkill },
																	  sourceScenario)).Return(
																		  new Dictionary<ISkill, IEnumerable<ISkillDay>> { { childSkill, new[] { childSkillDay } } });

				Expect.Call(childSkillDay.SkillStaffPeriodCollection).Return(new ISkillStaffPeriod[] { }).Repeat.Any();
				Expect.Call(() => jobResultFeedback.Warning("")).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(selection);
			}
		}
	}
}
