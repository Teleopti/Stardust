using Microsoft.Owin.Security.Provider;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
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
			Browser.Interactions.FillWith("#skillAreaName", skillAreaName);
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

		[Then(@"I select to monitor skill area '(.*)'")]
		[When(@"I select to monitor skill area '(.*)'")]
		public void ThenISelectToMonitorSkillArea(string skillArea)
		{
			var listId = "#" + Browser.Interactions.Javascript("return $('#skill-area-id input').attr(\"aria-owns\")");
			Browser.Interactions.Javascript(string.Format("$('{0} li:contains(\"{1}\")').click()", listId, skillArea));
			Browser.Interactions.AssertAnyContains(".intraday-monitor-item", skillArea);
		}

		[Then(@"I should no longer be able to monitor '(.*)'")]
		public void ThenIShouldNoLongerBeAbleToMonitor(string skillArea)
		{
			Browser.Interactions.Javascript("document.querySelector(\"#skill-area-input\").focus();");
			var listId = "#" + Browser.Interactions.Javascript("return $('#skill-area-id input').attr(\"aria-owns\")");
			Browser.Interactions.AssertNotExistsUsingJQuery(listId, string.Format("{0} li:contains(\"{1}\")", listId, skillArea));
		}
		
		[Then(@"I should monitor '(.*)'")]
		public void ThenIShouldMonitor(string monitorItem)
		{
			Browser.Interactions.AssertAnyContains(".intraday-monitor-item", monitorItem);
		}

		[When(@"I select to remove '(.*)'")]
		public void WhenISelectToRemove(string skillArea)
		{
			Browser.Interactions.Click(".skill-area-options");
			Browser.Interactions.Click(".skill-area-delete");
			Browser.Interactions.Click(".skill-area-delete-confirm");
		}

		[Given(@"there is a Skill Area called '(.*)' that monitors skill '(.*)'")]
		public void GivenThereIsASkillAreaCalledThatMonitorsSkill(string skillArea, string skill)
		{
			DataMaker.Data().Apply(new SkillAreaConfigurable()
			{
				Name = skillArea,
				Skill = skill 
			});
		}
	}
}
