﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class AbsenceRequestFormMappingTest
	{
		private ILoggedOnUser _loggedOnUser;
		private Person _person;
		private IUserTimeZone _userTimeZone;
		private IAbsenceRepository _absenceRepository;
		private AbsenceRequestFormMapper target;

		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_person = new Person();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);
			_absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();

			target = new AbsenceRequestFormMapper(_loggedOnUser, _absenceRepository, _userTimeZone);
		}
		
		[Test]
		public void ShouldMapPerson()
		{
			var result = target.Map(new AbsenceRequestForm());

			result.Person.Should().Be.SameInstanceAs(_person);
		}

		[Test]
		public void ShouldMapSubject()
		{
			var form = new AbsenceRequestForm { Subject = "Test" };

			var result = target.Map(form);

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToAbsenceRequest()
		{
			var result = target.Map(new AbsenceRequestForm());

			result.Request.Should().Be.OfType<AbsenceRequest>();
		}

		[Test]
		public void ShouldMapMessage()
		{
			var form = new AbsenceRequestForm { Message = "Message" };

			var result = target.Map(form);

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}

		[Test]
		public void ShouldMapAbsence()
		{
			var absence = new Absence();
			var id = Guid.NewGuid();
			_absenceRepository.Stub(x => x.Load(id)).Return(absence);

			var form = new AbsenceRequestForm {AbsenceId = id};

			var result = target.Map(form);

			((AbsenceRequest) result.Request).Absence.Should().Be.SameInstanceAs(absence);
		}

		[Test]
		public void ShouldMapFullDayAbsence()
		{
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
			var form = new AbsenceRequestForm {FullDay = true};

			var result = target.Map(form);

			var startTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 0, 0, 0), timeZone);
			var endTime = TimeZoneHelper.ConvertToUtc(new DateTime(2012, 5, 11, 23, 59, 0), timeZone);

			var expected = new DateTimePeriod(startTime, endTime);

			result.Request.Period.Should().Be(expected);
		}

		[Test]
		public void ShouldMapPeriod()
		{
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);
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
			var result = target.Map(form);

			var expected = new DateTimePeriodFormMapper(_userTimeZone).Map(form.Period);
			result.Request.Period.Should().Be(expected);
		}
	}
}
