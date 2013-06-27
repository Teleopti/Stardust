using System;
using System.Globalization;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class WeekScheduleTimeIndicatorStepDefinitions
	{
		private WeekSchedulePage _page { get { return Pages.Pages.WeekSchedulePage; } }

		[Given(@"I should see the time indicator at time '(.*)'")]
		[Then(@"I should see the time indicator at time '(.*)'")]
		public void ThenIShouldSeeTheTimeIndicatorAtTime(DateTime date)
		{
			const int heightOfDay = 668;
			const int timeLineOffset = 203;
			const int timeIndicatorHeight = 2;
		    TimeSpan minTimelineTime;
		    TimeSpan maxTimelineTime;
			
			_page.AnyTimelineLabel.WaitUntilExists();

			var startText = _page.TimelineLabels.First().InnerHtml.Split('<')[0];
			var returnCharAt = startText.IndexOf('\n');
			if (returnCharAt>=0)
			{
				startText = startText.Substring(0, returnCharAt+1);
			}
			if (!TimeHelper.TryParse(startText, out minTimelineTime))
			{
				throw new ValidationException("Could not find timeline start label time.");
			}

			var endText = _page.TimelineLabels[_page.TimelineLabels.Count - 1].InnerHtml.Split('<')[0];
			returnCharAt = endText.IndexOf('\n');
			if (returnCharAt >= 0)
			{
				endText = endText.Substring(0, returnCharAt+1);
			}
			if (!TimeHelper.TryParse(endText, out maxTimelineTime))
			{
				throw new ValidationException("Could not find timeline end label time.");
			}
			var positionPercentage = (decimal)(date.TimeOfDay - minTimelineTime).Ticks /
									 (maxTimelineTime - minTimelineTime).Ticks;
			var heightOfTimeIndicator = Math.Round(positionPercentage * heightOfDay, 0) -
										Math.Round((decimal)(timeIndicatorHeight / 2), 0);

			EventualAssert.That(() => _page.TimeIndicatorForDate(date).Style.GetAttributeValue("Top"), Is.EqualTo(Math.Round(heightOfTimeIndicator, 0).ToString(CultureInfo.InvariantCulture) + "px"));

			EventualAssert.That(() => _page.TimeIndicatorInTimeLine.Style.GetAttributeValue("Top"),
								Is.EqualTo((Math.Round(heightOfTimeIndicator, 0) + timeLineOffset).ToString(CultureInfo.InvariantCulture) + "px"));
		}

		[Then(@"I should not see the time indicator for date '(.*)'")]
		public void ThenIShouldNotSeeTheTimeIndicatorForDate(DateTime date)
		{
			EventualAssert.That(() => _page.TimeIndicatorForDate(date).DisplayVisible(), Is.False);
			EventualAssert.That(() => _page.TimeIndicatorInTimeLine.DisplayVisible(), Is.False);
		}
		
		[Then(@"I should not see the time indicator")]
		public void ThenIShouldNotSeeTheTimeIndicator()
		{
			EventualAssert.That(() => _page.FirstDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SecondDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.ThirdDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.FourthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.FifthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SixthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SeventhDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.TimeIndicatorInTimeLine.Style.GetAttributeValue("display"), Is.EqualTo("none"));
		}
	}
}