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

			var me = new Person();
			me.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HelsinkiTimeZoneInfo());
			var data = new TeamScheduleData
				{
					User = me,
					CanSeePersons = new[] {agent},
					CanSeeConfidentialAbsencesFor = new[] {agent},
					Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = agent.Id.Value,
									Shift = JsonConvert.SerializeObject(new Shift
										{
											Projection = new[]
												{
													new SimpleLayer
														{
															Start = startTime
														}
												}
										})
								}
						}
				};

			var result = target.Map(data);

			var personStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, me.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat();
			result.Single().Projection.Single().Start.Should().Be(personStartTime);
		}
	}
}