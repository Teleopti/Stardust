using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class IntradayStepDefinitions
	{
		[Given(@"There is a skill to monitor called '(.*)'")]
		public void GivenThereIsASkillToMonitorCalled(string skill)
		{
			const string queue = "queue1";

			DataMaker.Data().Apply(new ActivityConfigurable
			{
				Name = "activity1"
			});

			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = skill,
				Activity = "activity1"
			});

			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queue,
				QueueId = 9
			});

			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = skill,
				SkillName = skill,
				QueueSourceName = queue,
				Open24Hours = true
			});
		}

		[Given(@"I select to create a new Skill Area")]
		public void GivenISelectToCreateANewSkillArea()
		{
			Browser.Interactions.Click(".skill-area-create");
		}

		[Given(@"I name the Skill Area '(.*)'")]
		public void GivenINameTheSkillArea(string skillAreaName)
		{
			Browser.Interactions.FillWith(".skill-area-name input", skillAreaName);
		}

		[Given(@"I select the skill '(.*)'")]
		public void GivenISelectTheSkill(string skillName)
		{
			Browser.Interactions.ClickContaining(".skill-area-list", skillName);
		}

		[When(@"I am done creating Skill Area")]
		public void WhenIAmDoneCreatingSkillArea()
		{
			Browser.Interactions.Click(".skill-area-save");
		}

		[Then(@"I should see that Skill Area '(.*)' is selected in monitor view")]
		public void ThenIShouldSeeThatSkillAreaIsSelectedInMonitorView(string skillAreaName)
		{
			Browser.Interactions.AssertAnyContains(".skill-area-select selected", skillAreaName);
		}

		[Then(@"I should see details for '(.*)'")]
		public void ThenIShouldSeeDetailsFor(string skillName)
		{
			Browser.Interactions.AssertAnyContains(".skill-area-table-item", skillName);
		}

		[Given(@"there is a Skill Area called '(.*)'")]
		public void GivenThereIsASkillAreaCalled(string p0)
		{
			ScenarioContext.Current.Pending();
		}

	}
}
