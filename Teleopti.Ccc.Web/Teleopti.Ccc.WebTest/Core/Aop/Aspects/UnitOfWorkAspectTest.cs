using System;
using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Aop.Aspects
{
	[TestFixture]
	public class UnitOfWorkAspectTest
	{
		[Test]
		public void ShouldHaveAttribute()
		{
			var target = new UnitOfWorkAttribute();
			target.Should().Be.AssignableTo<Attribute>();
			target.AspectType.Should().Be(typeof(UnitOfWorkAspect));
		}

		[Test]
		public void ShouldPersistAndDisposeUnitOfWorkAfterInvocation()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			
			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var businessUnitOverriderScope = MockRepository.GenerateMock<IDisposable>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var guid = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection { { "X-Business-Unit-Filter", guid.ToString() } });
			businessUnitFilterOverrider.Expect(x => x.OverrideWith(guid)).Return(businessUnitOverriderScope);
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvocation();
			target.OnAfterInvocation(null);

			unitOfWork.AssertWasCalled(x => x.PersistAll());
			businessUnitOverriderScope.AssertWasCalled(x => x.Dispose());
			unitOfWork.AssertWasCalled(x => x.Dispose());
		}

		[Test]
		public void ShouldOnlyDisposeUnitOfWorkAfterInvocationWithException()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);

			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var businessUnitOverriderScope = MockRepository.GenerateMock<IDisposable>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var guid = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection { { "X-Business-Unit-Filter", guid.ToString() } });
			businessUnitFilterOverrider.Expect(x => x.OverrideWith(guid)).Return(businessUnitOverriderScope);
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvocation();
			target.OnAfterInvocation(new Exception());

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
			businessUnitOverriderScope.AssertWasCalled(x => x.Dispose());
			unitOfWork.AssertWasCalled(x => x.Dispose());
		}

		[Test]
		public void ShouldChangeBusinessUnitIdFromHttpContextBeforeInvokation()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var guid = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection { { "X-Business-Unit-Filter", guid.ToString() } });
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvocation();

			businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(guid));
		}

		[Test]
		public void ShouldChangeBusinessUnitIdFromQueryStringBeforeInvokation()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var guid = Guid.NewGuid();
			request.Stub(x => x.QueryString).Return(new NameValueCollection { { "BusinessUnitId", guid.ToString() } });
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvocation();

			businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(guid));
		}

		[Test]
		public void ShouldChangeBusinessUnitIdAccordingToPriority()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var businessUnitFilterOverrider = MockRepository.GenerateMock<IBusinessUnitFilterOverrider>();
			var httpContext = new FakeHttpContext("http://example.com", null);
			var request = MockRepository.GenerateStub<FakeHttpRequest>("/", new Uri("http://localhost/"), new Uri("http://localhost/"));
			var idFromCustomHeader = Guid.NewGuid();
			var idFromQueryString = Guid.NewGuid();
			request.Stub(x => x.Headers).Return(new NameValueCollection { { "X-Business-Unit-Filter", idFromCustomHeader.ToString() } });
			request.Stub(x => x.QueryString).Return(new NameValueCollection { { "BusinessUnitId", idFromQueryString.ToString() } });
			httpContext.SetRequest(request);
			var currentHttpContext = new FakeCurrentHttpContext(httpContext);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, businessUnitFilterOverrider, currentHttpContext);

			target.OnBeforeInvocation();

			businessUnitFilterOverrider.AssertWasCalled(x => x.OverrideWith(idFromCustomHeader));
		}

		[Test]
		public void ShouldSkipChangingBusinessUnitIdIfNoHttpContext()
		{
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var currentHttpContext = new FakeCurrentHttpContext(null);
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			unitOfWorkFactoryProvider.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			var target = new UnitOfWorkAspect(unitOfWorkFactoryProvider, null, currentHttpContext);
			target.OnBeforeInvocation();
		}
	}
}
