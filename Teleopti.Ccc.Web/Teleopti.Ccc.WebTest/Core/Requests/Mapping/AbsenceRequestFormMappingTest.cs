using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	public class AbsenceRequestFormMappingTest : ISetup
	{
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserTimeZone UserTimeZone;
		public FakeAbsenceRepository AbsenceRepository;
		public AbsenceRequestFormMapper Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
		}

		[Test]
		public void ShouldMapPerson()
		{
			var result = Target.MapNewAbsenceRequest(new AbsenceRequestForm());

			result.Person.Should().Be(LoggedOnUser.CurrentUser());
		}

		[Test]
		public void ShouldMapSubject()
		{
			var form = new AbsenceRequestForm { Subject = "Test" };

			var result = Target.MapNewAbsenceRequest(form);

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToAbsenceRequest()
		{
			var result = Target.MapNewAbsenceRequest(new AbsenceRequestForm());

			result.Request.Should().Be.OfType<AbsenceRequest>();
		}

		[Test]
		public void ShouldMapMessage()
		{
			var form = new AbsenceRequestForm { Message = "Message" };

			var result = Target.MapNewAbsenceRequest(form);

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence().WithId();
			AbsenceRepository.Has(absence);

			var form = new AbsenceRequestForm { AbsenceId = absence.Id.GetValueOrDefault() };

			var result = Target.MapNewAbsenceRequest(form);

			((AbsenceRequest)result.Request).Absence.Should().Be(absence);
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
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
					EndTime = new TimeOfDay(TimeSpan.FromDays(1).Subtract(TimeSpan.FromMinutes(1))),
				}
			};

			var result = Target.MapNewAbsenceRequest(form);

			var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), timeZone);
			var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), timeZone);

			var expected = new DateTimePeriod(startTime, endTime);

			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapPeriod()
		{
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
			var result = Target.MapNewAbsenceRequest(form);

			var expected = new DateTimePeriodFormMapper(UserTimeZone).Map(form.Period);
			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapExistingRequestForSubject()
		{
			var form = new AbsenceRequestForm { Subject = "Test" };

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form);
			form.Subject = "Test1";
			Target.MapExistingAbsenceRequest(form, existingAbsenceRequest);

			existingAbsenceRequest.GetSubject(new NoFormatting()).Should().Be("Test1");
		}

		[Test]
		public void ShouldMapExistingRequestForMessage()
		{
			var form = new AbsenceRequestForm { Message = "Test" };

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form);
			form.Message = "Test1";
			Target.MapExistingAbsenceRequest(form, existingAbsenceRequest);

			existingAbsenceRequest.GetMessage(new NoFormatting()).Should().Be("Test1");
		}

		[Test]
		public void ShouldMapExistingRequestForAbsenceType()
		{
			var absence1 = new Absence().WithId();
			var absence2 = new Absence().WithId();
			AbsenceRepository.Has(absence1);
			AbsenceRepository.Has(absence2);

			var form = new AbsenceRequestForm { AbsenceId = absence1.Id.GetValueOrDefault()};

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form);
			form.AbsenceId = absence2.Id.GetValueOrDefault();
			Target.MapExistingAbsenceRequest(form, existingAbsenceRequest);

			((AbsenceRequest)existingAbsenceRequest.Request).Absence.Should().Be(absence2);
		}

		[Test]
		public void ShouldMapExistingRequestForPeriod()
		{
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

			var existingAbsenceRequest = Target.MapNewAbsenceRequest(form);
			form.FullDay = false;
			form.Period = new DateTimePeriodForm
			{
				StartDate = new DateOnly(2012, 5, 11),
				EndDate = new DateOnly(2012, 5, 11),
				StartTime = new TimeOfDay(TimeSpan.FromHours(1)),
				EndTime = new TimeOfDay(TimeSpan.FromHours(2))
			};
			Target.MapExistingAbsenceRequest(form, existingAbsenceRequest);

			var expected = new DateTimePeriodFormMapper(UserTimeZone).Map(form.Period);
			existingAbsenceRequest.Request.Period.Should().Be(expected);
		}
	}
}
