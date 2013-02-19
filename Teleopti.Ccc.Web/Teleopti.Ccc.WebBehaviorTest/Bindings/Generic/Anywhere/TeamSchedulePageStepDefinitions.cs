using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class TeamSchedulePageStepDefinitions
	{
		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).Text.Contains(personName), Is.True);
			EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).TableCell(QuicklyFind.ByClass("shift")).ChildrenOfType<WatiN.Core.List>().First().OwnListItems.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).Text.Contains(personName), Is.True);
			EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).TableCell(QuicklyFind.ByClass("shift")).ChildrenOfType<WatiN.Core.List>().First().OwnListItems.Count, Is.EqualTo(0));
		}

		[When(@"I select '(.*)'")]
		public void WhenISelectPierreBaldi(string personName)
		{
			Pages.Pages.AnywherePage.RowByPerson(personName).EventualClick();
		}

	}
}