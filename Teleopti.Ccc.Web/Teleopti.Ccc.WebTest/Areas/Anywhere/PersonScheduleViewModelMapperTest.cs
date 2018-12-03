using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PersonScheduleViewModelMapperTest
	{
		private TimeZoneInfo _hawaiiTimeZoneInfo;
		private CommonNameDescriptionSetting _commonNameDescriptionSetting;

		[SetUp]
		public void Setup()
		{
			_hawaiiTimeZoneInfo = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			_commonNameDescriptionSetting = MockRepository.GenerateMock<CommonNameDescriptionSetting>();
		}
		
		[Test]
		public void ShouldMapPersonName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var person = new Person().WithName(new Name("Pierra", "B"));

			var result = target.Map(new PersonScheduleData { Person = person, CommonAgentNameSetting = new CommonNameDescriptionSetting() });

			result.Name.Should().Be("Pierra B");
		}

		[Test]
		public void ShouldMapTimeZoneName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var person = new Person();
		    var timeZone = TimeZoneInfoFactory.BrazilTimeZoneInfo();
            person.PermissionInformation.SetDefaultTimeZone(timeZone);

            var result = target.Map(new PersonScheduleData { Person = person, CommonAgentNameSetting = new CommonNameDescriptionSetting() });

			result.TimeZoneName.Should().Be(timeZone.DisplayName);
		}

		[Test]
		public void ShouldMapIanaTimeZoneNameOther()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var person = new Person();

            var result = target.Map(new PersonScheduleData { 
                Person = person, 
                CommonAgentNameSetting = new CommonNameDescriptionSetting(), 
                IanaTimeZoneOther = "America/Washington"
            });

            result.IanaTimeZoneOther.Should().Be("America/Washington");
		}
		[Test]
		public void ShouldMapIanaTimeZoneName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var person = new Person();

			var result = target.Map(new PersonScheduleData
			{
				Person = person,
				CommonAgentNameSetting = new CommonNameDescriptionSetting(),
				IanaTimeZoneLoggedOnUser = "America/Washington"
			});

			result.IanaTimeZoneLoggedOnUser.Should().Be("America/Washington");
		}

		[Test]
		public void ShouldMapDefaultIntradayAbsenceTimesInUserTimeZone()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var startTime = new DateTime(2013, 3, 4, 13, 20, 0, DateTimeKind.Utc);
			var expectedStartTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2013, 3, 4, 13, 20, 0, DateTimeKind.Utc), _hawaiiTimeZoneInfo);
			var shift = new Shift { Projection = new[] { new SimpleLayer { Start = startTime } } };

			var result = target.Map(new PersonScheduleData { Model = new Model { Shift = shift }, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person()});

			result.DefaultIntradayAbsenceData.StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expectedStartTime.TimeOfDay, CultureInfo.CurrentCulture));
			result.DefaultIntradayAbsenceData.EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expectedStartTime.AddHours(1).TimeOfDay, CultureInfo.CurrentCulture));
		}

		private static DateTime roundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks);
		}

		[Test]
		public void ShouldMapDefaultIntradayAbsenceTimesInUserTimeZoneForToday()
		{
			var now1 = new Now();
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), now1);
			var now = now1.UtcDateTime();
			var startTime = now.AddHours(-3);
			var endTime = startTime.AddHours(8);
			var expectedStartTime = TimeZoneInfo.ConvertTimeFromUtc(roundUp(now, TimeSpan.FromMinutes(15)), _hawaiiTimeZoneInfo);
			var expectedEndTime = TimeZoneInfo.ConvertTimeFromUtc(endTime, _hawaiiTimeZoneInfo);
			var shift = new Shift { Projection = new[] { new SimpleLayer { Start = startTime, End = endTime} } };

			var result = target.Map(new PersonScheduleData { Model = new Model { Shift = shift }, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.DefaultIntradayAbsenceData.StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expectedStartTime.TimeOfDay, CultureInfo.CurrentCulture));
			result.DefaultIntradayAbsenceData.EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expectedEndTime.TimeOfDay, CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapActivities()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var activities = new[] { new Activity("test1"), new Activity("test2") };

			var result = target.Map(new PersonScheduleData { Activities = activities, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Activities.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldMapActivityName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var activities = new[] { new Activity("test1")};

			var result = target.Map(new PersonScheduleData { Activities = activities, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Activities.Single().Name.Should().Be("test1");
		}

		[Test]
		public void ShouldMapActivityId()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var activity = new Activity("test1");
			activity.SetId(Guid.NewGuid());
			var activities = new[] { activity };

			var result = target.Map(new PersonScheduleData { Activities = activities, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Activities.Single().Id.Should().Be(activity.Id.Value.ToString());
		}

		[Test]
		public void ShouldMapAbsences()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absences = new[] {new Absence(), new Absence()};

			var result = target.Map(new PersonScheduleData { Absences = absences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Absences.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldMapAbsenceName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absences = new[] { new Absence { Description = new Description("A Name") } };

			var result = target.Map(new PersonScheduleData { Absences = absences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Absences.Single().Name.Should().Be("A Name");
		}

		[Test]
		public void ShouldMapAbsenceId()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absence = new Absence { Description = new Description("a") };
			absence.SetId(Guid.NewGuid());

			var result = target.Map(new PersonScheduleData { Absences = new[] { absence }, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.Absences.Single().Id.Should().Be(absence.Id.Value.ToString());
		}
		
		[Test]
		public void ShouldMapPersonAbsences()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absenceLayer = new AbsenceLayer(new Absence() { DisplayColor = Color.Red },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsence = new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer);
			var personAbsences = new[] { personAbsence };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });
			
			result.PersonAbsences.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapPersonAbsenceColor()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var absenceLayer = new AbsenceLayer(new Absence() {DisplayColor = Color.Red},
			                                    new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] {new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer)};

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo("Red");
		}

		[Test]
		public void ShouldMapPersonAbsenceColorForConfidentialAbsence()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var absenceLayer = new AbsenceLayer(new Absence { DisplayColor = Color.Red, Confidential = true},
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo(ConfidentialPayloadValues.DisplayColorHex);
		}

		[Test]
		public void ShouldMapPersonAbsenceColorForConfidentialAbsenceButHavePermission()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var absenceLayer = new AbsenceLayer(new Absence { DisplayColor = Color.Red, Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, HasViewConfidentialPermission = true, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo("Red");
		}

		[Test]
		public void ShouldMapPersonAbsenceName()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation") },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Name.Should().Be.EqualTo("Vacation");
		}

		[Test]
		public void ShouldMapPersonAbsenceNameForConfidentialAbsence()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation"), Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Name.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
		}

		[Test]
		public void ShouldMapPersonAbsenceNameForConfidentialAbsenceButHavePermission()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation"), Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, HasViewConfidentialPermission = true, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Name.Should().Be.EqualTo("Vacation");
		}

		[Test]
		public void ShouldMapPersonAbsenceStartTimeInUsersTimeZone()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var startTime = new DateTime(2013, 04, 18, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 04, 18, 17, 0, 0, DateTimeKind.Utc);
			var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(startTime, endTime));

			var person = new Person();
			var personAbsences = new[] {new PersonAbsence(person, MockRepository.GenerateMock<IScenario>(), absenceLayer)};

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			var personStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, _hawaiiTimeZoneInfo).ToFixedDateTimeFormat();

			result.PersonAbsences.Single().StartTime.Should().Be(personStartTime);
		}

		[Test]
		public void ShouldMapPersonAbsenceEndTimeInUsersTimeZone()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());

			var startTime = new DateTime(2013, 04, 18, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 04, 18, 17, 0, 0, DateTimeKind.Utc);
			var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(startTime, endTime));

			var person = new Person();

			var personAbsences = new[] { new PersonAbsence(person, MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			var personEndTime = TimeZoneInfo.ConvertTimeFromUtc(endTime, _hawaiiTimeZoneInfo).ToFixedDateTimeFormat();

			result.PersonAbsences.Single().EndTime.Should().Be(personEndTime);
		}

		[Test]
		public void ShouldMapPersonAbsenceId()
		{
			var target = new PersonScheduleViewModelMapper(new FakeUserTimeZone(_hawaiiTimeZoneInfo), new Now());
			var absenceLayer = new AbsenceLayer(new Absence() { DisplayColor = Color.Red },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsence = new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer);
			personAbsence.SetId(Guid.NewGuid());
			var personAbsences = new[] { personAbsence };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, CommonAgentNameSetting = _commonNameDescriptionSetting, Person = new Person() });

			result.PersonAbsences.Single().Id.Should().Be(personAbsence.Id.Value.ToString());
		}

	}
}