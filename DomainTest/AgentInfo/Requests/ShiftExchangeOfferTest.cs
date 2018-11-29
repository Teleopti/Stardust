using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
	public class ShiftExchangeOfferTest
	{
		[Test]
		public void ShouldCreateShiftExchangeOfferBasedOnMyCurrentShift()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			var activity = ActivityFactory.CreateActivity ("Phone");
			activity.SetId (new Guid ("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift (EditableShiftFactory.CreateEditorShift (activity,
				period.ChangeEndTime (TimeSpan.FromHours (3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));

			var target = new ShiftExchangeOffer (currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);

			target.MyShiftPeriod.Value.Should()
				.Be.EqualTo (new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
					new DateTime (2007, 1, 1, 18, 0, 0, DateTimeKind.Utc)));
			target.Date.Should().Be.EqualTo (currentShift.DateOnlyAsPeriod.DateOnly);
			target.Checksum.Should().Be.EqualTo (-1866394854L);
		}

		[Test]
		public void ShouldCreateShiftExchangeOfferBasedOnMyCurrentDayOff()
		{
			var currentShift = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1), PersonFactory.CreatePerson());
			currentShift.CreateAndAddDayOff (new DayOffTemplate());

			var target = new ShiftExchangeOffer (currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);

			target.MyShiftPeriod.HasValue.Should().Be.False();
			target.Date.Should().Be.EqualTo (currentShift.DateOnlyAsPeriod.DateOnly);
			target.Checksum.Should().Be.EqualTo (317L);
		}

		[Test]
		public void ShouldCreateShiftTradeRequestBasedOnShiftExchangeOffer()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			var activity = ActivityFactory.CreateActivity ("Phone");
			activity.SetId (new Guid ("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift (EditableShiftFactory.CreateEditorShift (activity,
				period.ChangeEndTime (TimeSpan.FromHours (3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));

			var target = new ShiftExchangeOffer (currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);

			var scheduleToTrade = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToTrade.AddMainShift (EditableShiftFactory.CreateEditorShift (ActivityFactory.CreateActivity ("Phone"),
				period.ChangeEndTime (TimeSpan.FromHours (-3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));

			IPersonRequest result = target.MakeShiftTradeRequest (scheduleToTrade);
			result.Person.Should().Be.EqualTo (scheduleToTrade.Person);
			result.IsNew.Should().Be.True();
			var shiftTradeRequest = ((IShiftTradeRequest) result.Request);
			shiftTradeRequest.PersonTo.Should().Be.SameInstanceAs (currentShift.Person);
			shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo.Should().Be.EqualTo (-1866394854L);
			shiftTradeRequest.ShiftTradeSwapDetails[0].PersonTo.Should().Be.EqualTo (currentShift.Person);
			shiftTradeRequest.ShiftTradeSwapDetails[0].PersonFrom.Should().Be.EqualTo (scheduleToTrade.Person);
			shiftTradeRequest.ShiftTradeSwapDetails[0].DateFrom.Should().Be.EqualTo (new DateOnly (2007, 1, 1));
			shiftTradeRequest.ShiftTradeSwapDetails[0].DateTo.Should().Be.EqualTo (new DateOnly (2007, 1, 1));
		}

		[Test]
		public void ShouldMatchMyCriteriaForShiftExchangeWhenShiftTimesInsideBoundary()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var criteria = new ShiftExchangeCriteria (new DateOnly (2026, 12, 25), dayFilterCriteria);

			var target = new ShiftExchangeOffer (ScheduleDayFactory.Create (new DateOnly (2007, 1, 1)), criteria,
				ShiftExchangeOfferStatus.Pending);

			var scheduleToCheck = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToCheck.AddMainShift (EditableShiftFactory.CreateEditorShift (ActivityFactory.CreateActivity ("Phone"),
				period.ChangeEndTime (TimeSpan.FromHours (-3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));
			target.IsWantedSchedule (scheduleToCheck).Should().Be.True();
		}

		[Test]
		public void ShouldMatchMyDayOffCriteriaForShiftExchangeWhenDayOffChecked()
		{
			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.DayOff, null);
			var criteria = new ShiftExchangeCriteria(new DateOnly(2026, 12, 25), dayFilterCriteria);

			var target = new ShiftExchangeOffer (ScheduleDayFactory.Create (new DateOnly (2007, 1, 1)), criteria,
				ShiftExchangeOfferStatus.Pending);

			var scheduleToCheck = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToCheck.CreateAndAddDayOff (new DayOffTemplate());
			target.IsWantedSchedule (scheduleToCheck).Should().Be.True();
		}

		[Test]
		public void ShouldNotMatchMyCriteriaForShiftExchangeWhenShiftTimesOutsideBoundary()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var criteria = new ShiftExchangeCriteria (new DateOnly (2026, 12, 25), dayFilterCriteria);

			var target = new ShiftExchangeOffer (ScheduleDayFactory.Create (new DateOnly (2007, 1, 1)), criteria,
				ShiftExchangeOfferStatus.Pending);

			var scheduleToCheck = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToCheck.AddMainShift (EditableShiftFactory.CreateEditorShift (ActivityFactory.CreateActivity ("Phone"),
				period.ChangeEndTime (TimeSpan.FromHours (3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));
			target.IsWantedSchedule (scheduleToCheck).Should().Be.False();
		}

		[Test]
		public void ShouldNotMatchMyDayOffCriteriaForShiftExchangeWhenShiftTimesAreSet()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var criteria = new ShiftExchangeCriteria (new DateOnly (2026, 12, 25), dayFilterCriteria);

			var target = new ShiftExchangeOffer (ScheduleDayFactory.Create (new DateOnly (2007, 1, 1)), criteria,
				ShiftExchangeOfferStatus.Pending);

			var scheduleToCheck = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToCheck.CreateAndAddDayOff (new DayOffTemplate());
			target.IsWantedSchedule (scheduleToCheck).Should().Be.False();
		}

		[Test]
		public void ShouldNotMatchMyCriteriaForShiftExchangeWhenOfferExpired()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var dayFilterCriteria = new ScheduleDayFilterCriteria(ShiftExchangeLookingForDay.WorkingShift, period);
			var criteria = new ShiftExchangeCriteria (new DateOnly (2006, 12, 25), dayFilterCriteria);

			var target = new ShiftExchangeOffer (ScheduleDayFactory.Create (new DateOnly (2007, 1, 1)), criteria,
				ShiftExchangeOfferStatus.Pending);

			var scheduleToCheck = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			scheduleToCheck.AddMainShift (EditableShiftFactory.CreateEditorShift (ActivityFactory.CreateActivity ("Phone"),
				period.ChangeEndTime (TimeSpan.FromHours (-3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));
			target.IsWantedSchedule (scheduleToCheck).Should().Be.False();
		}

		[Test]
		public void ShouldChangeShiftExchangeOfferStatus()
		{
			var period = new DateTimePeriod (new DateTime (2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime (2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			var activity = ActivityFactory.CreateActivity ("Phone");
			activity.SetId (new Guid ("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift (EditableShiftFactory.CreateEditorShift (activity,
				period.ChangeEndTime (TimeSpan.FromHours (3)),
				ShiftCategoryFactory.CreateShiftCategory ("Early")));

			var target = new ShiftExchangeOffer (currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			target.Status = ShiftExchangeOfferStatus.Completed;

			target.Status.Should().Be.EqualTo (ShiftExchangeOfferStatus.Completed);
		}

		[Test]
		public void ShouldStatusTextBeExpired()
		{
			var ret = createOfferAndReturnStatusText(ShiftExchangeOfferStatus.Pending);

			ret.Should().Be.EqualTo (Resources.Expired);
		}

		[Test]
		public void ShouldStatusTextBeCompleted()
		{
			var ret = createOfferAndReturnStatusText(ShiftExchangeOfferStatus.Completed);

			ret.Should().Be.EqualTo (Resources.Completed);
		}

		[Test]
		public void ShouldStatusTextBeInvalid()
		{
			var ret = createOfferAndReturnStatusText(ShiftExchangeOfferStatus.Invalid);

			ret.Should().Be.EqualTo (Resources.Invalid);
		}

		private static string createOfferAndReturnStatusText (ShiftExchangeOfferStatus status)
		{
			var currentShift = ScheduleDayFactory.Create (new DateOnly (2007, 1, 1));
			currentShift.Person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.StockholmTimeZoneInfo());

			var target = new ShiftExchangeOffer (currentShift, new ShiftExchangeCriteria(), status);
			var ret = target.GetStatusText();
			return ret;
		}
	}
}
