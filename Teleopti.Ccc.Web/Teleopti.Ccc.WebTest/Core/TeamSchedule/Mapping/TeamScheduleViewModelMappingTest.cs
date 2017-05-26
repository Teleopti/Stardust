using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	public class TeamScheduleViewModelMappingTest
	{
		private TeamScheduleDomainData data;
		private FakeUserTimeZone userTimeZone;
		private TeamScheduleViewModelMapper target;
		private PersonNameProvider personNameProvider;

		[SetUp]
		public void SetUp()
		{
			data = new TeamScheduleDomainData
					{
						Date = DateOnly.Today,
						DisplayTimePeriod = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(24)),
						Days = new TeamScheduleDayDomainData[] { }
					};
			
			userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Utc);

			personNameProvider = new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()));
			target = new TeamScheduleViewModelMapper(userTimeZone, new CreateHourText(userTimeZone, new SwedishCulture()), personNameProvider);
		}
		
		[Test]
		public void ShouldMapTeamSelection()
		{
			data.TeamOrGroupId = Guid.NewGuid();

			var result = target.Map(data);

			result.TeamSelection.Should().Be(data.TeamOrGroupId);
		}		
		
		[Test]
		public void ShouldMapShiftTradePermission()
		{
			data.TeamOrGroupId = Guid.NewGuid();

			var result = target.Map(data);

			result.ShiftTradePermisssion.Should().Be.False();
		}		
		
		[Test]
		public void ShouldMapShiftTradeBulletinBoardPermission()
		{
			data.TeamOrGroupId = Guid.NewGuid();

			var result = target.Map(data);

			result.ShiftTradeBulletinBoardPermission.Should().Be.False();
		}

		[Test]
		public void ShouldMapPeriodSelectionDate()
		{
			var result = target.Map(data);

			result.PeriodSelection.Date.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			var result = target.Map(data);

			result.PeriodSelection.Display.Should().Be.EqualTo(data.Date.ToShortDateString());
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			var result = target.Map(data);

			result.PeriodSelection.PeriodNavigation.CanPickPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasNextPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.HasPrevPeriod.Should().Be.True();
			result.PeriodSelection.PeriodNavigation.NextPeriod.Should().Be.EqualTo(data.Date.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(data.Date.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectaleDateRange()
		{
			var result = target.Map(data);

			result.PeriodSelection.SelectableDateRange.MinDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectableDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var result = target.Map(data);

			result.PeriodSelection.SelectedDateRange.MinDate.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectedDateRange.MaxDate.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapAgentNames()
		{
			var person = new Person().WithName(new Name("a", "person"));

			data.Days = new[]
			{
				new TeamScheduleDayDomainData
				{
					DisplayTimePeriod = data.DisplayTimePeriod,
					Person = person
				}
			};
			var result = target.Map(data);

			result.AgentSchedules[0].AgentName.Should().Be.EqualTo(personNameProvider.BuildNameFromSetting(person.Name));
		}

		[Test]
		public void ShouldMapLayerEndPositionPercent()
		{
			var periodStart = new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc);
			var periodEnd = periodStart.AddHours(5);
			var period = new DateTimePeriod(periodStart, periodEnd);
			var periodEndPosition = period.EndDateTime.TimeOfDay;
			var displayStart = new DateTime(2011, 12, 19, 0, 0, 0, DateTimeKind.Utc);
			var displayPeriod = new DateTimePeriod(displayStart, displayStart.AddHours(24));

			data.Days = new []
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
										DisplayTimePeriod = displayPeriod,
										Projection = new TeamScheduleProjection(new[]
										         	{
										         		new TeamScheduleLayer
										         			{
										         				Period = period,
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.Single().Layers.First().EndPositionPercent.Should().Be.EqualTo(periodEndPosition.Ticks / ((decimal)displayPeriod.ElapsedTime().Ticks));
		}

		[Test]
		public void ShouldMapLayerPositionPercent()
		{
			var periodStart = new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(periodStart, periodStart.Add(TimeSpan.FromHours(5)));
			var periodPosition = period.StartDateTime.TimeOfDay;
			var displayStart = new DateTime(2011, 12, 19, 0, 0, 0, DateTimeKind.Utc);
			var displayPeriod = new DateTimePeriod(displayStart, displayStart.AddHours(24));

			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
			            				DisplayTimePeriod = displayPeriod,
			            				Projection = new TeamScheduleProjection(new[]
			            				         	{
			            				         		new TeamScheduleLayer
			            				         			{
			            				         				Period = period
			            				         			}
			            				         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.Single().Layers.First().PositionPercent.Should().Be.EqualTo(periodPosition.Ticks / ((decimal)displayPeriod.ElapsedTime().Ticks));
		}

		[Test]
		public void ShouldMapLayerStartTime()
		{
			userTimeZone.IsSweden();
			var period = new DateTimePeriod(new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 19, 13, 0, 0, DateTimeKind.Utc));
			data.Days = new []
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				Period = period
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.First().Layers.Single().StartTime.Should().Be("09:00");
		}

		[Test]
		public void ShouldMapLayerEndTime()
		{
			userTimeZone.IsSweden();
			var period = new DateTimePeriod(new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 19, 13, 0, 0, DateTimeKind.Utc));
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				Period = period
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.First().Layers.Single().EndTime.Should().Be("14:00");
		}

		[Test]
		public void ShouldMapLayerActivityName()
		{
			userTimeZone.IsSweden();
			const string activityName = "Phone";
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				ActivityName = activityName
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.First().Layers.Single().ActivityName.Should().Be(activityName);
		}

		[Test]
		public void ShouldMapLayerColor()
		{
			data.Days = new []
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
																DisplayColor = Color.Red
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = target.Map(data);
			result.AgentSchedules.First().Layers.First().Color.Should().Be.EqualTo(Color.Red.ToHtml());
		}

		[Test]
		public void ShouldMapDayOffText()
		{
			var person = new Person();
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person,
			                                                                new Scenario("s"),
			                                                                new DateOnly(2000, 1, 1), new DayOffTemplate(new Description("long", "short")));

			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = person,
			            				Projection = new TeamScheduleProjection
			            				             	{
			            				             		DayOff = dayOff.DayOff(),
			            				             		SortDate = DateTime.MaxValue
			            				             	}
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.First().DayOffText.Should().Be.EqualTo("long");
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTimeLineShortTimeForSwedishCulture()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = target.Map(data);

			var expected = new[] {"08:45", "09:00", "10:00", "11:00", "11:15"};
			result.TimeLine.Select(t => t.ShortTime).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapTimeLineShortTimeForUsaCulture()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 12, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			target = new TeamScheduleViewModelMapper(userTimeZone, new CreateHourText(userTimeZone, new FakeUserCulture(CultureInfo.GetCultureInfo("en-US"))), null);
			var result = target.Map(data);

			var expected = new[] { "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM" };
			var actual = result.TimeLine.Select(t => t.ShortTime);

			CollectionAssert.IsSubsetOf(expected, actual);
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTimeLineUsingCurrentUsersTimeZone()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);
			userTimeZone.IsSweden();

			var result = target.Map(data);

			var expected = new[] { "09:45", "10:00", "11:00", "12:00", "12:15" };
			result.TimeLine.Select(t => t.ShortTime).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShowLabel_WhenFullHour_ShouldBeTrue()
		{
			var start = new DateTime(2012, 1, 3, 8, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 9, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = target.Map(data);

			result.TimeLine.First().IsFullHour.Should().Be(true);
		}

		[Test]
		public void ShowLabel_WhenNotFullHour_ShouldBeFalse()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 9, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = target.Map(data);

			result.TimeLine.First().IsFullHour.Should().Be(false);
		}


		[Test]
		public void ShouldMapTimeLinePositionPercent()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = target.Map(data);

			var assertTime = new DateTime(2012, 1, 3, 10, 0, 0);
			var timeRange = (decimal)data.DisplayTimePeriod.EndDateTime.Ticks - data.DisplayTimePeriod.StartDateTime.Ticks;
			var timePosition = (decimal)assertTime.Ticks - data.DisplayTimePeriod.StartDateTime.Ticks;
			result.TimeLine.Single(t => t.ShortTime == "10:00").PositionPercent.Should().Be(timePosition / timeRange);
		}

		[Test]
		public void ShouldMapHasDayOffUnder()
		{
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
									Person = new Person(),
			            				HasDayOffUnder = true
			            			}
			            	};

			var result = target.Map(data);

			result.AgentSchedules.First().HasDayOffUnder.Should().Be.True();
		}
	}
}
