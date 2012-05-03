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
	public class CurrentPrincipalProviderTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			savePrincipal();
			target = new PrincipalProvider();
		}

		[TearDown]
		public void Teardown()
		{
			restorePrincipal();
		}

		#endregion

		private IPrincipalProvider target;
		private IPrincipal _savedPrincipal;

		private void savePrincipal()
		{
			_savedPrincipal = Thread.CurrentPrincipal;
		}

		private void restorePrincipal()
		{
			Thread.CurrentPrincipal = _savedPrincipal;
		}


		[Test]
		public void ShouldReturnCurrentPrincipal()
		{
			var person = new Person();
			person.SetId(Guid.NewGuid());
			var principalForTest = new TeleoptiPrincipalForTest(new TeleoptiIdentity(person.Name.ToString(), null, null, null, AuthenticationTypeOption.Unknown),
			                                                    person);

			Thread.CurrentPrincipal = principalForTest;

			target.Current()
				.Should().Be.SameInstanceAs(principalForTest);
		}

		[Test]
		public void ShouldReturnNullWhenTeleoptiPrincipalNoAttached()
		{
			target.Current()
				.Should().Be.Null();
		}
	}
}