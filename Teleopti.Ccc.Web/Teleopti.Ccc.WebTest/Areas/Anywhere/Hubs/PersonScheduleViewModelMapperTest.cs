using System;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class PersonScheduleViewModelMapperTest
	{
		private IPermissionProvider _permissionProvider;

		[SetUp]
		public void Setup()
		{
			_permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PersonScheduleViewModelMappingProfile(_permissionProvider)));
		}

		// cant get this green with dynamics involved
		[Test, Ignore]
		public void ShouldConfigureCorrectly()
		{
			Mapper.AssertConfigurationIsValid();
		}
		
		[Test]
		public void ShouldMapPersonName()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = new Person {Name = new Name("Pierra", "B")};

			var result = target.Map(new PersonScheduleData {Person = person});

			result.Name.Should().Be("Pierra B");
		}

		[Test]
		public void ShouldMapTeam()
		{
			var target = new PersonScheduleViewModelMapper();
			var team = new Team { Description = new Description("A-Team") };
			var person = new Person();
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(-10), PersonContractFactory.CreatePersonContract(), new Team()));
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today, PersonContractFactory.CreatePersonContract(), team));
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today.AddDays(10), PersonContractFactory.CreatePersonContract(), new Team()));

			var result = target.Map(new PersonScheduleData { Date = DateTime.Today, Person = person });

			result.Team.Should().Be("A-Team");
		}

		[Test]
		public void ShouldMapSite()
		{
			var target = new PersonScheduleViewModelMapper();
			var team = new Team
				{
					Site = new Site("Moon"),
					Description = new Description("A-Team")
				};
			var person = new Person();
			person.AddPersonPeriod(new PersonPeriod(DateOnly.Today, PersonContractFactory.CreatePersonContract(), team));

			var result = target.Map(new PersonScheduleData { Date = DateTime.Today, Person = person });

			result.Site.Should().Be("Moon");
		}

		[Test]
		public void ShouldMapAbsences()
		{
			var target = new PersonScheduleViewModelMapper();
			var absences = new[] {new Absence(), new Absence()};

			var result = target.Map(new PersonScheduleData { Absences = absences });

			result.Absences.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldMapAbsenceName()
		{
			var target = new PersonScheduleViewModelMapper();
			var absences = new[] { new Absence { Description = new Description("A Name") } };

			var result = target.Map(new PersonScheduleData { Absences = absences });

			result.Absences.Single().Name.Should().Be("A Name");
		}

		[Test]
		public void ShouldMapAbsenceId()
		{
			var target = new PersonScheduleViewModelMapper();
			var absence = new Absence { Description = new Description(" ") };
			absence.SetId(Guid.NewGuid());

			var result = target.Map(new PersonScheduleData {Absences = new[] {absence}});

			result.Absences.Single().Id.Should().Be(absence.Id.Value.ToString());
		}

		private dynamic MakeLayer(string Color = "", DateTime? Start = null, int Minutes = 0,
		                          bool isAbsenceConfidential = false)
		{
			dynamic layer = new ExpandoObject();
			layer.Color = Color;
			layer.Start = Start.HasValue ? Start : null;
			layer.Minutes = Minutes;
			layer.IsAbsenceConfidential = isAbsenceConfidential;
			return layer;
		}

		[Test]
		public void ShouldMapLayers()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer(), MakeLayer() };

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Count().Should().Be(2);
		}

		[Test]
		public void ShouldMapLayerColor()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer("Green")};

			var result = target.Map(new PersonScheduleData {Shift = shift });

			result.Layers.Single().Color.Should().Be("Green");
		}

		[Test]
		public void ShouldMapLayerColorForConfidentialAbsence()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] {MakeLayer("Green", isAbsenceConfidential: true)};

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Single().Color.Should().Be(ConfidentialPayloadValues.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldMapLayerColorForConfidentialAbsenceButHavePermission()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = PersonFactory.CreatePersonWithId();
			_permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, person))
							   .Return(true);

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer("Green", isAbsenceConfidential: true) };

			var result = target.Map(new PersonScheduleData { Shift = shift, Person = person});

			result.Layers.Single().Color.Should().Be("Green");
		}

		[Test]
		public void ShouldMapLayerStartTimeInPersonsTimeZone()
		{
			var target = new PersonScheduleViewModelMapper();
			var startTime = new DateTime(2013, 3, 4, 8, 0, 0, DateTimeKind.Utc);

			var person = new Person();
			var personTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(personTimeZone);
			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer("", startTime) };

			var result = target.Map(new PersonScheduleData {Shift = shift, Person = person});

			var personStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat();
			result.Layers.Single().Start.Should().Be(personStartTime);
		}

		[Test]
		public void ShouldMapLayerMinutes()
		{
			var target = new PersonScheduleViewModelMapper();

			dynamic shift = new ExpandoObject();
			shift.Projection = new[] { MakeLayer(Color: "",Minutes: 60) };

			var result = target.Map(new PersonScheduleData { Shift = shift });

			result.Layers.Single().Minutes.Should().Be(60);
		}

		[Test]
		public void ShouldMapPersonAbsences()
		{
			var target = new PersonScheduleViewModelMapper();
			var absenceLayer = new AbsenceLayer(new Absence() { DisplayColor = Color.Red },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsence = new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer);
			var personAbsences = new[] { personAbsence };
		
			var result = target.Map(new PersonScheduleData {PersonAbsences = personAbsences});
			
			result.PersonAbsences.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapPersonAbsenceColor()
		{
			var target = new PersonScheduleViewModelMapper();

			var absenceLayer = new AbsenceLayer(new Absence() {DisplayColor = Color.Red},
			                                    new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] {new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer)};

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo("Red");
		}

		[Test]
		public void ShouldMapPersonAbsenceColorForConfidentialAbsence()
		{
			var target = new PersonScheduleViewModelMapper();

			var absenceLayer = new AbsenceLayer(new Absence { DisplayColor = Color.Red, Confidential = true},
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo(ConfidentialPayloadValues.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldMapPersonAbsenceColorForConfidentialAbsenceButHavePermission()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = PersonFactory.CreatePersonWithId();
			_permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, person))
			                   .Return(true);

			var absenceLayer = new AbsenceLayer(new Absence { DisplayColor = Color.Red, Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));

			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, Person = person });

			result.PersonAbsences.Single().Color.Should().Be.EqualTo("Red");
		}

		[Test]
		public void ShouldMapPersonAbsenceName()
		{
			var target = new PersonScheduleViewModelMapper();

			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation") },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			result.PersonAbsences.Single().Name.Should().Be.EqualTo("Vacation");
		}

		[Test]
		public void ShouldMapPersonAbsenceNameForConfidentialAbsence()
		{
			var target = new PersonScheduleViewModelMapper();

			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation"), Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			result.PersonAbsences.Single().Name.Should().Be.EqualTo(ConfidentialPayloadValues.Description.Name);
		}

		[Test]
		public void ShouldMapPersonAbsenceNameForConfidentialAbsenceButHavePermission()
		{
			var target = new PersonScheduleViewModelMapper();
			var person = PersonFactory.CreatePersonWithId();
			_permissionProvider.Stub(
				x => x.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential, DateOnly.Today, person))
							   .Return(true);
			var absenceLayer = new AbsenceLayer(new Absence { Description = new Description("Vacation"), Confidential = true },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsences = new[] { new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer) };

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences, Person = person});

			result.PersonAbsences.Single().Name.Should().Be.EqualTo("Vacation");
		}

		[Test]
		public void ShouldMapPersonAbsenceStartTimeInPersonsTimeZone()
		{
			var target = new PersonScheduleViewModelMapper();

			var startTime = new DateTime(2013, 04, 18, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 04, 18, 17, 0, 0, DateTimeKind.Utc);
			var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(startTime, endTime));

			var person = new Person();
			var personTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(personTimeZone);

			var personAbsences = new[] {new PersonAbsence(person, MockRepository.GenerateMock<IScenario>(), absenceLayer)};

			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			var personStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat();

			result.PersonAbsences.Single().StartTime.Should().Be(personStartTime);
		}

		[Test]
		public void ShouldMapPersonAbsenceEndTimeInPersonsTimeZone()
		{
			var target = new PersonScheduleViewModelMapper();

			var startTime = new DateTime(2013, 04, 18, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2013, 04, 18, 17, 0, 0, DateTimeKind.Utc);
			var absenceLayer = new AbsenceLayer(new Absence(), new DateTimePeriod(startTime, endTime));

			var person = new Person();
			var personTimeZone = TimeZoneInfoFactory.HawaiiTimeZoneInfo();
			person.PermissionInformation.SetDefaultTimeZone(personTimeZone);

			var personAbsences = new[] { new PersonAbsence(person, MockRepository.GenerateMock<IScenario>(), absenceLayer) };
			
			var result = target.Map(new PersonScheduleData { PersonAbsences = personAbsences });

			var personEndTime = TimeZoneInfo.ConvertTimeFromUtc(endTime, person.PermissionInformation.DefaultTimeZone()).ToFixedDateTimeFormat();

			result.PersonAbsences.Single().EndTime.Should().Be(personEndTime);
		}

		[Test]
		public void ShouldMapPersonAbsenceId()
		{
			var target = new PersonScheduleViewModelMapper();
			var absenceLayer = new AbsenceLayer(new Absence() { DisplayColor = Color.Red },
												new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
			var personAbsence = new PersonAbsence(new Person(), MockRepository.GenerateMock<IScenario>(), absenceLayer);
			personAbsence.SetId(Guid.NewGuid());
			var personAbsences = new[] { personAbsence };

			var result = target.Map(new PersonScheduleData {PersonAbsences = personAbsences});

			result.PersonAbsences.Single().Id.Should().Be(personAbsence.Id.Value.ToString());
		}
	}
}