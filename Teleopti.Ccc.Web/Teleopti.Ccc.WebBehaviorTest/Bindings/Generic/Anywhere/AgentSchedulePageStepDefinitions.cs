using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class AgentSchedulePageStepDefinitions
	{
		[Then(@"I should see agent schedule for '(.*)' on '(.*)'")]
		public void ThenIShouldSeeAgentScheduleForAgentOnDate(string name, string date)
		{
			var id = UserFactory.User(name).Person.Id.ToString();
			EventualAssert.That(() => Browser.Current.Url.Contains(id), Is.True);
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see a shift")]
		public void ThenIShouldSeeAShift()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).Exists, Is.True);
		}

		[Then(@"I should see (.*) shift layers")]
		public void ThenIShouldSeeNumberOfShiftLayers(int numberOfShifts)
		{
			EventualAssert.That(() => Browser.Current.Elements.Filter(Find.BySelector(".shift .layer")).Count, Is.EqualTo(numberOfShifts));
		}

		[Then(@"I should not see any shift")]
		public void ThenIShouldNotSeeAnyShift()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift")).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".shift .layer")).Exists, Is.False);
		}

	}
}