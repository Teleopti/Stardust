using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	class AbsenceRequestFormMappingTest
	{
		private ILoggedOnUser _loggedOnUser;
		private Person _person;
		private IUserTimeZone _userTimeZone;
		private AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest _absenceRequestFormToPersonRequest;
		
		[SetUp]
		public void Setup()
		{
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_person = new Person();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);

			_absenceRequestFormToPersonRequest =
				new AbsenceRequestFormMappingProfile.AbsenceRequestFormToPersonRequest(() => Mapper.Engine, () => _loggedOnUser);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new AbsenceRequestFormMappingProfile(
					() => Mapper.Engine,
					() => _loggedOnUser,
					() => _userTimeZone,
					() => _absenceRequestFormToPersonRequest
					)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPerson()
		{
			var result = Mapper.Map<AbsenceRequestForm, IPersonRequest>(new AbsenceRequestForm());

			result.Person.Should().Be.SameInstanceAs(_person);
		}

		[Test]
		public void ShouldMapSubject()
		{
			var form = new AbsenceRequestForm { Subject = "Test" };

			var result = Mapper.Map<AbsenceRequestForm, IPersonRequest>(form);

			result.GetSubject(new NoFormatting()).Should().Be("Test");
		}

		[Test]
		public void ShouldMapToAbsenceRequest()
		{
			var result = Mapper.Map<AbsenceRequestForm, IPersonRequest>(new AbsenceRequestForm());

			result.Request.Should().Be.OfType<AbsenceRequest>();
		}

		[Test]
		public void ShouldMapMessage()
		{
			var form = new AbsenceRequestForm { Message = "Message" };

			var result = Mapper.Map<AbsenceRequestForm, IPersonRequest>(form);

			result.GetMessage(new NoFormatting()).Should().Be("Message");
		}
	}
}
