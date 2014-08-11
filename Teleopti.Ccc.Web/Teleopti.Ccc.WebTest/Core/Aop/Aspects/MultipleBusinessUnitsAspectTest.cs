using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Core.RequestContext;

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
            request.Stub(x => x.Headers).Return(new NameValueCollection() { { "x-override-business-unit-filter", guid.ToString() } });
            httpContext.SetRequest(request);
            var currentHttpContext = new FakeCurrentHttpContext(httpContext);
            var target = new MultipleBusinessUnitsAspect(businessUnitFilterOverrider, currentHttpContext);

            target.OnBeforeInvokation();

            businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(guid));
        }
    }
}
