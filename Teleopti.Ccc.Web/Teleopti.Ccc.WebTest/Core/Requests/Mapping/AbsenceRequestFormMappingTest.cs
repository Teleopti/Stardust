using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	public class AbsenceRequestFormMappingTest : IIsolateSystem
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserTimeZone UserTimeZone;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;
		public AbsenceRequestModelMapper Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
		}

		[Test]
		public void ShouldMapPerson()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var result = Target.MapNewAbsenceRequest(new AbsenceRequestForm().ToModel(UserTimeZone, LoggedOnUser));

			result.Person.Should().Be(LoggedOnUser.CurrentUser());
		}

		[Test]
		public void ShouldMapSubject()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var form = new AbsenceRequestForm { Subject = "Test" };

			var result = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToAbsenceRequest()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var result = Target.MapNewAbsenceRequest(new AbsenceRequestForm().ToModel(UserTimeZone, LoggedOnUser));

			result.Request.Should().Be.OfType<AbsenceRequest>();
		}

		[Test]
		public void ShouldMapMessage()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var form = new AbsenceRequestForm { Message = "Message" };

			var result = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}

		[Test]
		public void ShouldMapAbsence()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var form = new AbsenceRequestForm { AbsenceId = absence.Id.GetValueOrDefault() };

			var result = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));

			((AbsenceRequest)result.Request).Absence.Should().Be(absence);
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			UserTimeZone.Is(timeZone);
			var form = new AbsenceRequestForm
			{
				FullDay = true,
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(2012, 5, 11),
					EndDate = new DateOnly(2012, 5, 11),
					StartTime = new TimeOfDay(TimeSpan.Zero),
					EndTime = new TimeOfDay(TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1)))
				}
			};

			var result = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));

			var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), timeZone);
			var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), timeZone);

			var expected = new DateTimePeriod(startTime, endTime);

			result.Request.Period.Should().Be(expected);
			(result.Request as IAbsenceRequest).FullDay.Should().Be(true);
		}

		[Test]
		public void ShouldMapPeriod()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			UserTimeZone.Is(timeZone);
			var form = new AbsenceRequestForm
			{
				Period = new DateTimePeriodForm
				{
					StartDate = DateOnly.Today,
					StartTime = new TimeOfDay(TimeSpan.FromHours(12)),
					EndDate = DateOnly.Today,
					EndTime = new TimeOfDay(TimeSpan.FromHours(13))
				}
			};
			var result = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));

			var expected = form.Period.Map(UserTimeZone);
			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapExistingRequestForSubject()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var form = new AbsenceRequestForm { Subject = "Test" };

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));
			form.Subject = "Test1";
			Target.MapExistingAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser), existingAbsenceRequest);

			existingAbsenceRequest.GetSubject(new NoFormatting()).Should().Be("Test1");
		}

		[Test]
		public void ShouldMapExistingRequestForMessage()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var form = new AbsenceRequestForm { Message = "Test" };

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));
			form.Message = "Test1";
			Target.MapExistingAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser), existingAbsenceRequest);

			existingAbsenceRequest.GetMessage(new NoFormatting()).Should().Be("Test1");
		}

		[Test]
		public void ShouldMapExistingRequestForAbsenceType()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var absence1 = new Absence().WithId();
			var absence2 = new Absence().WithId();
			AbsenceRepository.Has(absence1);
			AbsenceRepository.Has(absence2);

			var form = new AbsenceRequestForm { AbsenceId = absence1.Id.GetValueOrDefault()};

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));
			form.AbsenceId = absence2.Id.GetValueOrDefault();
			Target.MapExistingAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser), existingAbsenceRequest);

			((AbsenceRequest)existingAbsenceRequest.Request).Absence.Should().Be(absence2);
		}

		[Test]
		public void ShouldMapExistingRequestForPeriod()
		{
			PersonRepository.Add(LoggedOnUser.CurrentUser());
			var form = new AbsenceRequestForm
			{
				FullDay = true,
				Period = new DateTimePeriodForm
				{
					StartDate = new DateOnly(2012, 5, 11),
					EndDate = new DateOnly(2012, 5, 11),
					StartTime = new TimeOfDay(TimeSpan.Zero),
					EndTime = new TimeOfDay(TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))),
				}
			};

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser));
			form.FullDay = false;
			form.Period = new DateTimePeriodForm
			{
				StartDate = new DateOnly(2012, 5, 11),
				EndDate = new DateOnly(2012, 5, 11),
				StartTime = new TimeOfDay(TimeSpan.FromHours(1)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(2))
			};
			Target.MapExistingAbsenceRequest(form.ToModel(UserTimeZone, LoggedOnUser), existingAbsenceRequest);

			var expected = form.Period.Map(UserTimeZone);
			existingAbsenceRequest.Request.Period.Should().Be(expected);
		}
	}
}
