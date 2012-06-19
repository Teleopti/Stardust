using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class WebRequestPrincipalContextTest
	{
		[Test]
		public void ShouldMakePrincipal()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), principalFactory);

			target.SetCurrentPrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows);

			principalFactory.AssertWasCalled(x => x.MakePrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows));
		}

		[Test]
		public void ShouldSetMadePrincipalToCurrentThread()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Windows), person);
			principalFactory.Stub(x => x.MakePrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows)).Return(principal);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), principalFactory);

			target.SetCurrentPrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows);

			Thread.CurrentPrincipal.Should().Be(principal);
		}

		[Test]
		public void ShouldSetMadePrincipalToCurrentHttpContext()
		{
			var principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Windows), person);
			var httpContext = new FakeHttpContext("");
			principalFactory.Stub(x => x.MakePrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows)).Return(principal);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(httpContext), principalFactory);

			target.SetCurrentPrincipal(person, dataSource, businessUnit, AuthenticationTypeOption.Windows);

			httpContext.User.Should().Be(principal);
		}

		[Test]
		public void ShouldSetPrincipalToCurrentThread()
		{
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Windows), person);
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(new FakeHttpContext("")), null);

			target.SetCurrentPrincipal(principal);

			Thread.CurrentPrincipal.Should().Be(principal);
		}

		[Test]
		public void ShouldSetPrincipalToCurrentHttpContext()
		{
			var person = PersonFactory.CreatePerson();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			var principal = TeleoptiPrincipalCacheable.Make(new TeleoptiIdentity("", dataSource, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Windows), person);
			var httpContext = new FakeHttpContext("");
			var target = new WebRequestPrincipalContext(new FakeCurrentHttpContext(httpContext), null);

			target.SetCurrentPrincipal(principal);

			httpContext.User.Should().Be(principal);
		}
	}
}
