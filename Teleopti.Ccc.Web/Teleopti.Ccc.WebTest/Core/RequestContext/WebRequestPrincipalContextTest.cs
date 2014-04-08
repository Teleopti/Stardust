using System.Security.Principal;
using System.Threading;
using System.Web;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class WebRequestPrincipalContextTest
	{
		private TokenIdentityProvider tokenIdentityProvider;
		[SetUp]
		public void SetUp()
		{
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			tokenIdentityProvider = new TokenIdentityProvider(currentHttpContext);
		}

		[Test]
		public void ShouldMakePrincipal()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();

			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), principalFactory, tokenIdentityProvider);

			target.SetCurrentPrincipal(person, dataSource, businessUnit);

			principalFactory.AssertWasCalled(x => x.MakePrincipal(person, dataSource, businessUnit));
		}

		[Test]
		public void ShouldSetMadePrincipalToCurrentThread()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent()), person);
			principalFactory.Stub(x => x.MakePrincipal(person, dataSource, businessUnit)).Return(principal);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), principalFactory, tokenIdentityProvider);

			target.SetCurrentPrincipal(person, dataSource, businessUnit);

			Thread.CurrentPrincipal.Should().Be(principal);
		}

		[Test]
		public void ShouldSetMadePrincipalToCurrentHttpContext()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent()), person);
			var httpContext = new FakeHttpContext("");
			principalFactory.Stub(x => x.MakePrincipal(person, dataSource, businessUnit)).Return(principal);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(httpContext), principalFactory, tokenIdentityProvider);

			target.SetCurrentPrincipal(person, dataSource, businessUnit);

			httpContext.User.Should().Be(principal);
		}

		[Test]
		public void ShouldSetPrincipalToCurrentThread()
		{
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent()), person);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), null, tokenIdentityProvider);

			target.SetCurrentPrincipal(principal);

			Thread.CurrentPrincipal.Should().Be(principal);
		}

		[Test]
		public void ShouldSetPrincipalToCurrentHttpContext()
		{
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent()), person);
			var httpContext = new FakeHttpContext("");
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(httpContext), null, tokenIdentityProvider);

			target.SetCurrentPrincipal(principal);

			httpContext.User.Should().Be(principal);
		}
	}
}
