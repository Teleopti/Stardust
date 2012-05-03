using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class PrincipalProviderTest
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
		public void ShouldReturnCurrentPrincipal()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var principal = new TeleoptiPrincipal(
				new TeleoptiIdentity(person.Name.ToString(), null, null, null, AuthenticationTypeOption.Unknown),
				person);
			Thread.CurrentPrincipal = principal;

			var target = new PrincipalProvider();

			target.Current().Should().Be.SameInstanceAs(principal);
		}

		[Test]
		public void ShouldReturnNullWhenTeleoptiPrincipalNoAttached()
		{
			var target = new PrincipalProvider();
			target.Current().Should().Be.Null();
		}
	}
}