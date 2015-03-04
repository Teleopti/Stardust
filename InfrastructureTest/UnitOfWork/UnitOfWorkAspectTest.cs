using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[PrincipalAndStateTest]
	public class UnitOfWorkAspectTest2 : IRegisterInContainer
	{

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<TheService>().AsSelf().SingleInstance().ApplyAspects();
		}

		public TheService TheService;
		public IPersonRepository PersonRepository;

		[Test, Ignore]
		public void ShouldBeAbleToQueryARepository()
		{
			IEnumerable<IPerson> persons = null;

			TheService.Does(uow =>
			{
				persons = PersonRepository.LoadAll();
			});

			persons.Should().Not.Be.Null();
		}

	}

	public class TheService
	{
		private readonly ICurrentUnitOfWork _uow;

		public TheService(ICurrentUnitOfWork uow)
		{
			_uow = uow;
		}

		//[UnitOfWork]
		public virtual void Does(Action<IUnitOfWork> action)
		{
			action(_uow.Current());
		}

	}

	[TestFixture]
	public class UnitOfWorkAspectTest
	{
		[Test]
		public void ShouldHaveAttribute()
		{
			var target = new UnitOfWorkAttribute();
			target.Should().Be.AssignableTo<Attribute>();
			target.AspectType.Should().Be(typeof(IUnitOfWorkAspect));
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
