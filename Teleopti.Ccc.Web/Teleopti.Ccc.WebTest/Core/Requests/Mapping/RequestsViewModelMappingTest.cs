using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class RequestsViewModelMappingTest
	{
		private IUserTimeZone _userTimeZone;
		private ILinkProvider _linkProvider;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_linkProvider = MockRepository.GenerateMock<ILinkProvider>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new RequestsViewModelMappingProfile(
				() => _userTimeZone, 
				() => _linkProvider
				)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapLink()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());

			_linkProvider.Stub(x => x.RequestDetailLink(request.Id.Value)).Return("aLink");

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.rel.Should().Be("self");
			result.Link.href.Should().Be("aLink");
			result.Link.Methods.Should().Contain("GET");
			result.Link.Methods.Should().Contain("DELETE");
			result.Link.Methods.Should().Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldMapPayload()
		{
			const string payLoadName = "this is the one";
			var abs = new Absence {Description = new Description(payLoadName)};
			var request = new PersonRequest(new Person(), new AbsenceRequest(abs, new DateTimePeriod(1900, 1, 1, 1900, 1, 2)));
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);
			result.Payload.Should().Be.EqualTo(payLoadName);
		}

		[Test]
		public void ShouldNotMapPayload()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);
			result.Payload.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotMapLinksDeleteMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("DELETE");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateApproved()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Approve(MockRepository.GenerateMock<IRequestApprovalService>(), MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldNotMapLinksPutMethodIfStateDenied()
		{
			var request = new PersonRequest(new Person());
			request.SetId(Guid.NewGuid());
			request.Pending();
			request.Deny(null, null, MockRepository.GenerateMock<IPersonRequestCheckAuthorization>());
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Link.Methods.Should().Not.Contain("PUT");
		}

		[Test]
		public void ShouldMapId()
		{
			var request = new PersonRequest(new Person()) { Subject = "Test" };
			request.SetId(Guid.NewGuid());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Id.Should().Be(request.Id.ToString());
		}

		[Test]
		public void ShouldMapSubject()
		{
			var request = new PersonRequest(new Person()){Subject = "Test"};

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Subject.Should().Be("Test");
		}

		[Test]
		public void ShouldMapDate()
		{
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(5));
			
			var request = new PersonRequest(new Person(), new TextRequest(period));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Dates.Should().Be.EqualTo(period.ToShortDateTimeString(timeZone));
		}

		[Test]
		public void ShouldMapType()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Type.Should().Be(request.Request.RequestTypeDescription);
		}

		[Test]
		public void ShouldMapRequestTypeEnum()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.TypeEnum.Should().Be(RequestType.TextRequest);
		}

		[Test]
		public void ShouldMapUpdatedOn()
		{
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod())) {UpdatedOn = DateTime.UtcNow};

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.UpdatedOn.Should().Be(timeZone.ConvertTimeFromUtc(request.UpdatedOn.Value).ToShortDateTimeString());
		}

		[Test]
		public void ShouldMapText()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			request.TrySetMessage("Message");
			
			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Text.Should().Be.EqualTo("Message");
		}

		[Test]
		public void ShouldMapStatus()
		{
			var request = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.Status.Should().Be.EqualTo(request.StatusText);
		}

		[Test]
		public void ShouldMapRawDateInfo()
		{
			var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var request = new PersonRequest(new Person(), new TextRequest(period));
			var startInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(start, _userTimeZone.TimeZone());
			var endInCorrectTimezone = TimeZoneHelper.ConvertFromUtc(end, _userTimeZone.TimeZone());

			var result = Mapper.Map<IPersonRequest, RequestViewModel>(request);

			result.RawDateFrom.Should().Be.EqualTo(startInCorrectTimezone.ToShortDateString());
			result.RawDateTo.Should().Be.EqualTo(endInCorrectTimezone.ToShortDateString());
			result.RawTimeFrom.Should().Be.EqualTo(startInCorrectTimezone.ToShortTimeString());
			result.RawTimeTo.Should().Be.EqualTo(endInCorrectTimezone.ToShortTimeString());
		}
	}
}
