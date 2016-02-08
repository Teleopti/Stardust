using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class CurrentIdentityTest
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
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, null, null);
			var principal = new TeleoptiPrincipal(identity, person);

			Thread.CurrentPrincipal = principal;

			var target = new CurrentIdentity(new CurrentTeleoptiPrincipal());

			target.Current().Should().Be.SameInstanceAs(identity);
		}
	}
}