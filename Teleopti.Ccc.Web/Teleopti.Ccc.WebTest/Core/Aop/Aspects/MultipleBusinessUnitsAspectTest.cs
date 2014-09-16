﻿using System;
using System.Collections.Specialized;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.WebTest.Core.Aop.Aspects
{
    [TestFixture]
    public class MultipleBusinessUnitsAspectTest
    {
        [Test]
        public void ShouldHaveAttribute()
        {
            var target = new MultipleBusinessUnitsAttribute();
            target.Should().Be.AssignableTo<Attribute>();
            target.AspectType.Should().Be(typeof (MultipleBusinessUnitsAspect));
        }

        [Test]
        public void ShouldChangeBusinessUnitIdFromHttpContextBeforeInvokation()
        {
            var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
            var httpContext = new FakeHttpContext("http://example.com", null);
            var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
            var guid = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection() { { "X-Business-Unit-Filter", guid.ToString() } });
            httpContext.SetRequest(request);
            var currentHttpContext = new FakeCurrentHttpContext(httpContext);
            var target = new MultipleBusinessUnitsAspect(businessUnitFilterOverrider, currentHttpContext);

            target.OnBeforeInvokation();

            businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(guid));
        }

        [Test]
        public void ShouldChangeBusinessUnitIdFromQueryStringBeforeInvokation()
        {
            var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
            var httpContext = new FakeHttpContext("http://example.com", null);
            var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var guid = Guid.NewGuid();
	        request.Stub(x => x.QueryString).Return(new NameValueCollection() {{"BusinessUnitId", guid.ToString()}});
			httpContext.SetRequest(request);
            var currentHttpContext = new FakeCurrentHttpContext(httpContext);
            var target = new MultipleBusinessUnitsAspect(businessUnitFilterOverrider, currentHttpContext);

            target.OnBeforeInvokation();

            businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(guid));
        }

	    [Test]
	    public void ShouldChangeBusinessUnitIdAccordingToPriority()
	    {
			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var idFromCustomHeader = Guid.NewGuid();
			var idFromQueryString = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection() { { "X-Business-Unit-Filter", idFromCustomHeader.ToString() } });
			request.Stub(x => x.QueryString).Return(new NameValueCollection() { { "BusinessUnitId", idFromQueryString.ToString() } });
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var target = new MultipleBusinessUnitsAspect(businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvokation();

			businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(idFromCustomHeader));
	    }

        [Test]
        public void ShouldNotChangeBusinessUnitIdFromHttpContextBeforeInvokationIfNotRequested()
        {
            var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
            var httpContext = new FakeHttpContext("http://example.com", null);
            var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			request.Stub(x => x.Headers).Return(new NameValueCollection() { { "X-Business-Unit-Filter", "" } });
            httpContext.SetRequest(request);
            var currentHttpContext = new FakeCurrentHttpContext(httpContext);
            var target = new MultipleBusinessUnitsAspect(businessUnitFilterOverrider, currentHttpContext);

            target.OnBeforeInvokation();
        }
    }
}
