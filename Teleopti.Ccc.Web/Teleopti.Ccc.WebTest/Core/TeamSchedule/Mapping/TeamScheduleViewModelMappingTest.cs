﻿using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.Mapping
{
	[TestFixture]
	public class TeamScheduleViewModelMappingTest
	{
		private TeamScheduleDomainData data;
		private IUserTimeZone userTimeZone;
		private CccTimeZoneInfo timeZone;

		[SetUp]
		public void SetUp()
		{
			data = new TeamScheduleDomainData
					{
						Date = DateOnly.Today,
						DisplayTimePeriod = new DateTimePeriod(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(24)),
						Days = new TeamScheduleDayDomainData[] { }
					};

			timeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
			userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Do((Func<CccTimeZoneInfo>)(() => timeZone));

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new TeamScheduleViewModelMappingProfile(() => userTimeZone)));
		}
		
		[Test]
		public void ShouldConfigure()
		{
			Mapper.AssertConfigurationIsValid(); 
		}

		[Test]
		public void ShouldMapTeamSelection()
		{
			data.TeamId = Guid.NewGuid();

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.TeamSelection.Should().Be(data.TeamId);
		}

		[Test]
		public void ShouldMapPeriodSelectionDate()
		{
			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.PeriodSelection.Date.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectionDisplay()
		{
			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.PeriodSelection.Display.Should().Be.EqualTo(data.Date.ToShortDateString());
		}

		[Test]
		public void ShouldFillPeriodSelectionNavigation()
		{
			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.PeriodSelection.Navigation.CanPickPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.HasNextPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.HasPrevPeriod.Should().Be.True();
			result.PeriodSelection.Navigation.FirstDateNextPeriod.Should().Be.EqualTo(data.Date.AddDays(1).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.Navigation.LastDatePreviousPeriod.Should().Be.EqualTo(data.Date.AddDays(-1).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldFillPeriodSelectaleDateRange()
		{
			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.PeriodSelection.SelectableDateRange.MinDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectableDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPeriodSelectedDateRange()
		{
			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.PeriodSelection.SelectedDateRange.MinDate.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
			result.PeriodSelection.SelectedDateRange.MaxDate.Should().Be.EqualTo(data.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapAgentNames()
		{
			var person = new Person {Name = new Name("a", "person")};

			var result = Mapper.Map<TeamScheduleDayDomainData, AgentScheduleViewModel>(new TeamScheduleDayDomainData
			                                                                           	{
			                                                                           		DisplayTimePeriod = data.DisplayTimePeriod,
			                                                                           		Person = person
			                                                                           	});

			result.AgentName.Should().Be.EqualTo(person.Name.ToString());
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

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

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

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.Single().Layers.First().PositionPercent.Should().Be.EqualTo(periodPosition.Ticks / ((decimal)displayPeriod.ElapsedTime().Ticks));
		}

		[Test]
		public void ShouldMapLayerStartTime()
		{
			timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var period = new DateTimePeriod(new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 19, 13, 0, 0, DateTimeKind.Utc));
			data.Days = new []
			            	{
			            		new TeamScheduleDayDomainData
			            			{
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				Period = period
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.First().Layers.Single().StartTime.Should().Be(period.StartDateTimeLocal(timeZone).ToShortTimeString());
		}

		[Test]
		public void ShouldMapLayerEndTime()
		{
			timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var period = new DateTimePeriod(new DateTime(2011, 12, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2011, 12, 19, 13, 0, 0, DateTimeKind.Utc));
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				Period = period
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.First().Layers.Single().EndTime.Should().Be(period.EndDateTimeLocal(timeZone).ToShortTimeString());
		}

		[Test]
		public void ShouldMapLayerActivityName()
		{
			timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			const string activityName = "Phone";
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
										         				ActivityName = activityName
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.First().Layers.Single().ActivityName.Should().Be(activityName);
		}

		[Test]
		public void ShouldMapLayerColor()
		{
			data.Days = new []
			            	{
			            		new TeamScheduleDayDomainData
			            			{
										Projection = new TeamScheduleProjection(new []
										         	{
										         		new TeamScheduleLayer
										         			{
																DisplayColor = Color.Red
										         			}
										         	}, DateTime.MaxValue)
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);
			result.AgentSchedules.First().Layers.First().Color.Should().Be.EqualTo(Color.Red.ToHtml());
		}

		[Test]
		public void ShouldMapDayOffText()
		{
			var dayOff = new PersonDayOff(new Person(), new Scenario("   "), new DayOffTemplate(new Description("long", "short")), new DateOnly(2000, 1, 1));

			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
			            				Projection = new TeamScheduleProjection
			            				             	{
			            				             		DayOff = dayOff,
			            				             		SortDate = DateTime.MaxValue
			            				             	}
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.First().DayOffText.Should().Be.EqualTo("long");
		}

		[Test]
		public void ShouldMapTimeLineShortTime()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			var sep = CultureInfo.CurrentUICulture.DateTimeFormat.TimeSeparator;
			var expected = new[] {sep + "45", "09", "10", "11", sep + "15"};
			result.TimeLine.Select(t => t.ShortTime).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapTimeLineLongTime()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			var expected = new[] { "08:45", "09:00", "10:00", "11:00", "11:15" };
			result.TimeLine.Select(t => t.LongTime).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapTimeLineUsingCurrentUsersTimeZone()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);
			timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			var expected = new[] { "09:45", "10:00", "11:00", "12:00", "12:15" };
			result.TimeLine.Select(t => t.LongTime).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapTimeLinePositionPercent()
		{
			var start = new DateTime(2012, 1, 3, 8, 45, 0, DateTimeKind.Utc);
			var end = new DateTime(2012, 1, 3, 11, 15, 0, DateTimeKind.Utc);
			data.DisplayTimePeriod = new DateTimePeriod(start, end);

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			var assertTime = new DateTime(2012, 1, 3, 10, 0, 0);
			var timeRange = (decimal)data.DisplayTimePeriod.EndDateTime.Ticks - data.DisplayTimePeriod.StartDateTime.Ticks;
			var timePosition = (decimal)assertTime.Ticks - data.DisplayTimePeriod.StartDateTime.Ticks;
			result.TimeLine.Single(t => t.ShortTime == "10").PositionPercent.Should().Be(timePosition / timeRange);
		}

		[Test]
		public void ShouldMapHasDayOffUnder()
		{
			data.Days = new[]
			            	{
			            		new TeamScheduleDayDomainData
			            			{
			            				HasDayOffUnder = true
			            			}
			            	};

			var result = Mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(data);

			result.AgentSchedules.First().HasDayOffUnder.Should().Be.True();
		}
	}
}
