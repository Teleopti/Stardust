using System;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class GroupScheduleViewModelMapperTest
	{
		[Test]
		public void ShouldMapLayerStartTimeToMyTimeZone()
		{
			var target = new GroupScheduleViewModelMapper();
			var startTime = new DateTime(2013, 3, 4, 8, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithId();
			var timeZone = TimeZoneInfoFactory.HelsinkiTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = timeZone,
				CanSeePersons = new[] { person },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value,
									Model = JsonConvert.SerializeObject(new Model
										{
											Shift = new Shift
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
		public void ShouldMapDayOffTimes()
		{
			var startTime = new DateTime(2013, 10, 07, 22, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 10, 08, 22, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePersonWithId();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var target = new GroupScheduleViewModelMapper();

			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { person },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value,
									Model = JsonConvert.SerializeObject(new Model
										{
											DayOff = new DayOff
												{
													Start = startTime,
													End = endTime,
													Title = "Day off1"
												}
										})
								}
						}
			};

			var result = target.Map(data);

			var startTimeInUserTimeZone = TimeZoneInfo.ConvertTimeFromUtc(startTime, userTimeZone).ToFixedDateTimeFormat();
			result.Single().DayOff.DayOffName.Should().Be("Day off1");
			result.Single().DayOff.Start.Should().Be(startTimeInUserTimeZone);
			result.Single().DayOff.Minutes.Should().Be(endTime.Subtract(startTime).TotalMinutes);
		}


		[Test]
		public void ShouldMapFullDayAbsence()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { person },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value,
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

		[Test]
		public void ShouldMapPersonName()
		{
			var person = PersonFactory.CreatePersonWithId();
			person.Name = new Name("f", "l");
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { person },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value,
									Model = JsonConvert.SerializeObject(new Model
										{
											FirstName = person.Name.FirstName,
											LastName = person.Name.LastName
										})
								}
						}
			};

			var result = target.Map(data);

			result.Single().Name.Should().Be(person.Name.ToString());
		}

		[Test]
		public void ShouldOnlyMapPermittedPersons()
		{
			var personPermitted = PersonFactory.CreatePersonWithId();
			var personNotPermitted = PersonFactory.CreatePersonWithId();
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new  CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { personPermitted },
				Schedules = new[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = personPermitted.Id.Value
								},
							new PersonScheduleDayReadModel
								{
									PersonId = personNotPermitted.Id.Value
								}
						}
			};

			var result = target.Map(data);

			result.Single().PersonId.Should().Be(personPermitted.Id.Value.ToString());
		}

		[Test]
		public void ShouldIncludePersonsWithoutSchedules()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeePersons = new[] { person },
				Schedules = new PersonScheduleDayReadModel[] { }
			};

			var result = target.Map(data);

			result.Single().PersonId.Should().Be(person.Id.Value.ToString());
		}

		[Test]
		public void ShouldExcludeUnpublishedSchedulesButIncludePerson()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
				CanSeeUnpublishedSchedules = false,
				CanSeePersons = new[] { person },
				CanSeeConfidentialAbsencesFor = new IPerson[] { },
				Schedules = new PersonScheduleDayReadModel[]
						{
							new PersonScheduleDayReadModel
								{
									PersonId = person.Id.Value
								},
						}
			};

			var result = target.Map(data);

			result.Single().PersonId.Should().Be(person.Id.Value.ToString());
			result.Single().Projection.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldMapShiftDate()
		{
			var person = PersonFactory.CreatePersonWithId();
			var target = new GroupScheduleViewModelMapper();
			var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			var data = new GroupScheduleData
			{
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				UserTimeZone = userTimeZone,
					CanSeePersons = new[] {person},
					Schedules = new PersonScheduleDayReadModel[]
						{
							new PersonScheduleDayReadModel
								{
									Date = new DateTime(2013, 12, 18),
									PersonId = person.Id.Value
								},
						}
				};

			var result = target.Map(data);

			result.Single().Date.Should().Be(new DateTime(2013, 12, 18).ToFixedDateFormat());
		}
	}
}