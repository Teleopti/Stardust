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
			var agent = PersonFactory.CreatePersonWithId();
			var timeZone = TimeZoneInfoFactory.HelsinkiTimeZoneInfo();
			var data = new TeamScheduleData
				{
					UserTimeZone = timeZone,
					CanSeePersons = new[] {agent},
					CanSeeConfidentialAbsencesFor = new[] {agent},
					Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = agent.Id.Value,
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

			var personStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, timeZone).ToFixedDateTimeFormat();
			result.Single().Projection.Single().Start.Should().Be(personStartTime);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var agent = PersonFactory.CreatePersonWithId();
			var target = new TeamScheduleViewModelMapper();

			var data = new TeamScheduleData
				{
					CanSeePersons = new[] { agent },
					CanSeeConfidentialAbsencesFor = new[] { agent },
					Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									Model = JsonConvert.SerializeObject(new Model
										{
											DayOff = new DayOff
												{
													Title = "Day off"
												}
										})
								}
						}
				};

			var result = target.Map(data);

			result.Single().IsDayOff.Should().Be.True();
		}
	}
}