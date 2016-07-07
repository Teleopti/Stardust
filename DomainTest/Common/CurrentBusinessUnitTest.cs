﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class CurrentBusinessUnitTest
	{
		[Test]
		public void ShouldReturnCurrentBusinessUnit()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), new NoBusinessUnitForRequest());
			var businessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, businessUnit, null, null), new Person());

			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);

			target.Current()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldConsiderXBusinessUnitFilterIfItsAHttpRequest()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var businessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit();
			businessUnit.SetId(Guid.NewGuid());
			var anotherBusinessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, anotherBusinessUnit, null, null), new Person());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			businessUnitRepository.Stub(x => x.Load(businessUnit.Id.Value)).Return(businessUnit);

			var isHttpRequest = MockRepository.GenerateMock<IBusinessUnitForRequest>();
			isHttpRequest.Stub(x => x.TryGetBusinessUnit()).Return(businessUnit);
			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), isHttpRequest);
			target.Current()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			var currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			var target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal), new NoBusinessUnitForRequest());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(null);

			target.Current()
				.Should().Be.Null();
		}
	}


}