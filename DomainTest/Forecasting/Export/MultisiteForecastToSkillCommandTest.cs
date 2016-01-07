using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
    [TestFixture]
    public class MultisiteForecastToSkillCommandTest
    {
        private IMultisiteSkill multisiteSkill;
        private IChildSkill childSkill;
        private ISkill targetSkill;
        private MultisiteForecastToSkillCommand target;
        private MockRepository mocks;
        private IJobResultFeedback jobResultFeedback;
        private IImportForecastToSkillCommand importForecastToSkillCommand;
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
            importForecastToSkillCommand = mocks.DynamicMock<IImportForecastToSkillCommand>();
            skillDayLoadHelper = mocks.DynamicMock<ISkillDayLoadHelper>();
            scenarioRepository = mocks.DynamicMock<IScenarioRepository>();

            target = new MultisiteForecastToSkillCommand(importForecastToSkillCommand, skillDayLoadHelper, scenarioRepository, jobResultFeedback);
        }

        [Test]
        public void ShouldCopyForecastFromMultisiteSubSkillToSkill()
        {
            var multisiteSkillForExport = new MultisiteSkillForExport {MultisiteSkill = multisiteSkill};
            multisiteSkillForExport.AddSubSkillMapping(new SkillExportCombination
                                                           {SourceSkill = childSkill, TargetSkill = targetSkill});
            var selection = new SkillExportSelection(new[] {multisiteSkillForExport});
            selection.Period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
            var sourceScenario = ScenarioFactory.CreateScenarioAggregate();
            var childSkillDay = mocks.DynamicMock<ISkillDay>();
            var skillStaffPeriod = mocks.DynamicMock<ISkillStaffPeriod>();

            using (mocks.Record())
            {
                Expect.Call(scenarioRepository.LoadDefaultScenario(multisiteSkill.BusinessUnit)).Return(sourceScenario);
                Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] {multisiteSkill},
                                                                      sourceScenario)).Return(
                                                                          new Dictionary<ISkill, IList<ISkillDay>> { { childSkill, new [] { childSkillDay } } });

                Expect.Call(() => importForecastToSkillCommand.Execute(childSkill, targetSkill, null, new DateOnlyPeriod())).Constraints(
                    Rhino.Mocks.Constraints.Is.Equal(childSkill), Rhino.Mocks.Constraints.Is.Equal(targetSkill),
                    Rhino.Mocks.Constraints.Is.Anything(), Rhino.Mocks.Constraints.Is.Anything());
                Expect.Call(childSkillDay.SkillStaffPeriodCollection).Return(
                    new ReadOnlyCollection<ISkillStaffPeriod>(new[] {skillStaffPeriod}));
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
				Expect.Call(scenarioRepository.LoadDefaultScenario(multisiteSkill.BusinessUnit)).Return(sourceScenario);
				Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] { multisiteSkill },
																	  sourceScenario)).Return(
																		  new Dictionary<ISkill, IList<ISkillDay>> { { childSkill, new[] { childSkillDay } } });

				Expect.Call(childSkillDay.SkillStaffPeriodCollection).Return(
					new ReadOnlyCollection<ISkillStaffPeriod>(new ISkillStaffPeriod[] {  })).Repeat.Any();
				Expect.Call(()=>jobResultFeedback.Warning("")).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(selection);
			}
		}
    }
}
