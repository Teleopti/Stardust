using System;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class TeamScheduleViewModelMapperTest
	{

		[Test] public void ShouldMapLayerStartTimeToMyTimeZone()
		{
			var target = new TeamScheduleViewModelMapper();
			var startTime = new DateTime(2013, 3, 4, 8, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithId();
			var timeZone = TimeZoneInfoFactory.HelsinkiTimeZoneInfo();
			var data = new TeamScheduleData
				{
					UserTimeZone = timeZone,
					CanSeePersons = new[] {person},
					CanSeeConfidentialAbsencesFor = new[] {person},
					Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value,
									Model = JsonConvert.SerializeObject(new Model
										{
											Shift =	new Shift
											{
											Projection = new[]
												{
													new SimpleLayer
														{
															Start = startTime
														}
												}
											}
										})
								}
						}
				};

			var result = target.Map(data);

			var layerStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, timeZone).ToFixedDateTimeFormat();
			result.Single().Projection.Single().Start.Should().Be(layerStartTime);
		}

		[Test]
		public void ShouldMapDayOffStartAndEndInUserTimeZone()
		{
			var startTime = new DateTime(2013, 10, 07, 22, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 10, 08, 22, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithId();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var target = new TeamScheduleViewModelMapper();

			var data = new TeamScheduleData
			{
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { person },
				CanSeeConfidentialAbsencesFor = new[] { person },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									Model = JsonConvert.SerializeObject(new Model
										{
											DayOff = new DayOff
												{
													Start = startTime,
													End = endTime,
													Title = "Day off"
												}
										})
								}
						}
			};

			var result = target.Map(data);

			var startTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(startTime, userTimeZone).ToFixedDateTimeFormat();
			var endTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(endTime, userTimeZone).ToFixedDateTimeFormat();
			result.Single().DayOffStartTime.Should().Be(startTimeInUserTimeZone);
			result.Single().DayOffEndTime.Should().Be(endTimeInUserTimeZone);
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new TeamScheduleViewModelMapper();

			var data = new TeamScheduleData
				{
					CanSeePersons = new[] {person},
					CanSeeConfidentialAbsencesFor = new[] {person},
					Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									Model = JsonConvert.SerializeObject(new Model
										{
											Shift = new Shift
												{
													IsFullDayAbsence = true
												}
										})
								}
						}
				};

			var result = target.Map(data);

			result.Single().IsFullDayAbsence.Should().Be.True();
		}
	}
}