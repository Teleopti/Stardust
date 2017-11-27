﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests
{
	[TestFixture]
	[RequestsTest]
	public class RespondToShiftTradeTest : ISetup
	{
		public IRespondToShiftTrade Target;
		public ILoggedOnUser LoggedOnUser;
		public FakePersonRequestRepository PersonRequestRepository;

		[Test]
		public void ShouldDenyShiftTradeOnRequest()
		{
			var shiftTradeId = Guid.NewGuid();
			var personRequest = new PersonRequest(LoggedOnUser.CurrentUser(),
				new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail(LoggedOnUser.CurrentUser(), LoggedOnUser.CurrentUser(), DateOnly.Today, DateOnly.Today)
				})).WithId(shiftTradeId);
			
			PersonRequestRepository.Add(personRequest);
			
			Target.Deny(shiftTradeId, "");

			personRequest.DenyReason.Should().Be.EqualTo("RequestDenyReasonOtherPart");
		}

		[Test]
		public void ApproveShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = Guid.Empty;
			Assert.That(Target.OkByMe(id, ""),Is.Not.Null);
		}

		[Test]
		public void DenyShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = Guid.Empty;
			Assert.That(Target.Deny(id, ""), Is.Not.Null);
		}
		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			system.UseTestDouble<FakeLicensedFunctionProvider>().For<ILicensedFunctionsProvider>();

			var currentBusinessUnit = new SpecificBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			system.UseTestDouble(currentBusinessUnit).For<ICurrentBusinessUnit>();
			var dataSource = new FakeCurrentDatasource("Test");
			system.UseTestDouble(dataSource).For<ICurrentDataSource>();
		}
	}
}
