using System;
using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class CurrentBusinessUnitTest
	{
		[Test]
		public void ShouldReturnCurrentBusinessUnit()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), null, null);
			var businessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, businessUnit, null), new Person());

			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);

			target.Current()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldConsiderXBusinessUnitFilterFirst()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var httpContext = new MutableFakeCurrentHttpContext();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			businessUnit.SetId(Guid.NewGuid());
			var headers = new NameValueCollection { { "X-Business-Unit-Filter", businessUnit.Id.Value.ToString() } };
			httpContext.SetContext(new FakeHttpContext(null, null, null, null, null, null, null, headers));
			var anotherBusinessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, anotherBusinessUnit, null), new Person());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			businessUnitRepository.Stub(x => x.Load(businessUnit.Id.Value)).Return(businessUnit);

			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), httpContext, businessUnitRepository);
			target.Current()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), null, null);
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(null);

			target.Current()
				.Should().Be.Null();
		}
	}
}