using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
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
        private ISaveForecastToSkillCommand saveForecastToSkillCommand;
        private ISkillDayLoadHelper skillDayLoadHelper;
        private IScenarioProvider scenarioProvider;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            multisiteSkill = SkillFactory.CreateMultisiteSkill("test");
            childSkill = SkillFactory.CreateChildSkill("ChildSkill1", multisiteSkill);
            targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

            jobResultFeedback = mocks.DynamicMock<IJobResultFeedback>();
            saveForecastToSkillCommand = mocks.DynamicMock<ISaveForecastToSkillCommand>();
            skillDayLoadHelper = mocks.DynamicMock<ISkillDayLoadHelper>();
            scenarioProvider = mocks.DynamicMock<IScenarioProvider>();

            target = new MultisiteForecastToSkillCommand(saveForecastToSkillCommand, skillDayLoadHelper, scenarioProvider, jobResultFeedback);
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
                Expect.Call(scenarioProvider.DefaultScenario(multisiteSkill.BusinessUnit)).Return(sourceScenario);
                Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] {multisiteSkill},
                                                                      sourceScenario)).Return(
                                                                          new Dictionary<ISkill, IList<ISkillDay>> { { childSkill, new [] { childSkillDay } } });

                Expect.Call(() => saveForecastToSkillCommand.Execute(selection.Period, targetSkill, null)).Constraints(
                    Rhino.Mocks.Constraints.Is.Equal(selection.Period), Rhino.Mocks.Constraints.Is.Equal(targetSkill),
                    Rhino.Mocks.Constraints.Is.Anything());
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
				Expect.Call(scenarioProvider.DefaultScenario(multisiteSkill.BusinessUnit)).Return(sourceScenario);
				Expect.Call(skillDayLoadHelper.LoadSchedulerSkillDays(selection.Period, new[] { multisiteSkill },
																	  sourceScenario)).Return(
																		  new Dictionary<ISkill, IList<ISkillDay>> { { childSkill, new[] { childSkillDay } } });

				Expect.Call(childSkillDay.SkillStaffPeriodCollection).Return(
					new ReadOnlyCollection<ISkillStaffPeriod>(new ISkillStaffPeriod[] {  }));
				Expect.Call(()=>jobResultFeedback.Warning("")).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(selection);
			}
		}
    }
}
