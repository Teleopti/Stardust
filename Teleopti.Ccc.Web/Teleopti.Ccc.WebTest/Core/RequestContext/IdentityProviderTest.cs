using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class IdentityProviderTest
	{
		private IPrincipal _savedPrincipal;

		[SetUp]
		public void Setup()
		{
			_savedPrincipal = Thread.CurrentPrincipal;
		}

		[TearDown]
		public void Teardown()
		{
			Thread.CurrentPrincipal = _savedPrincipal;
		}

		[Test]
		public void ShouldReturnCurrentIdentity()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, null, AuthenticationTypeOption.Unknown);
			var principal = new TeleoptiPrincipalForTest(identity, person);

			Thread.CurrentPrincipal = principal;

			var target = new IdentityProvider();

			target.Current().Should().Be.SameInstanceAs(identity);
		}
	}
}