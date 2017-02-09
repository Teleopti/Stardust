using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersonToPermissionValidatorTest
	{
		private DateTime baseDate = new DateTime(2016, 01, 01);
		private ITeam teamFrom;
		private ITeam teamTo;
		private IPerson personFrom;
		private IPerson personTo;
		private ITeleoptiPrincipal principal;

		private IPrincipalFactory principalFactory;
		private ICurrentDataSource currentDataSource;
		private IPersonRepository personRepository;
		private IRoleToPrincipalCommand roleToPrincipalCommand;
		private IPrincipalAuthorizationFactory principalAuthorizationFactory;

		private ShiftTradeRequestPersonToPermissionValidator target;

		[SetUp]
		public void SetupTest()
		{
			teamFrom = TeamFactory.CreateSimpleTeam("TeamFrom");
			teamTo = TeamFactory.CreateSimpleTeam("TeamTo");

			var baseDateOnly = new DateOnly(baseDate);
			personFrom = PersonFactory.CreatePersonWithPersonPeriodFromTeam(baseDateOnly, teamFrom);
			personTo = PersonFactory.CreatePersonWithPersonPeriodFromTeam(baseDateOnly, teamTo);

			var dataSource = MockRepository.GenerateMock<IDataSource>();
			currentDataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			currentDataSource.Stub(x => x.Current()).Return(dataSource);

			principal = new TeleoptiPrincipal(new TeleoptiIdentity("PersonTo", dataSource, null, null, null), personTo);
			principalFactory = MockRepository.GenerateMock<IPrincipalFactory>();
			principalFactory.Stub(x => x.MakePrincipal(personTo, dataSource, null, null)).Return(principal);

			personRepository = new FakePersonRepositoryLegacy();
			roleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();

			var authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(principal));
			principalAuthorizationFactory = MockRepository.GenerateMock<IPrincipalAuthorizationFactory>();
			principalAuthorizationFactory.Stub(x => x.FromPrincipal(principal)).Return(authorization);

			target = new ShiftTradeRequestPersonToPermissionValidator(principalFactory, currentDataSource,
				personRepository, roleToPrincipalCommand, principalAuthorizationFactory);
		}

		[Test]
		public void ShouldNotBeSatisfiedIfRecipientHasNoPermissionForShiftTrade()
		{
			var applicationFunctions = new Dictionary<string, bool>
			{
				{DefinedRaptorApplicationFunctionPaths.ViewSchedules, true },
				{DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, true },
				{DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, false}
			};

			var result = validateRequestWithPermission(applicationFunctions);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldNotBeSatisfiedIfRecipientHasNoPermissionForBothViewSchedulesAndViewUnpublishedSchedules()
		{
			var applicationFunctions = new Dictionary<string, bool>
			{
				{DefinedRaptorApplicationFunctionPaths.ViewSchedules, false },
				{DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, false },
				{DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, true}
			};

			var result = validateRequestWithPermission(applicationFunctions);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldBeSatisfiedIfRecipientHasPermissionForShiftTradeAndViewSchedules()
		{
			var applicationFunctions = new Dictionary<string, bool>
			{
				{DefinedRaptorApplicationFunctionPaths.ViewSchedules, true },
				{DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, false },
				{DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, true}
			};

			var result = validateRequestWithPermission(applicationFunctions);
			Assert.AreEqual(true, result);
		}

		[Test]
		public void ShouldBeSatisfiedIfRecipientHasPermissionForShiftTradeAndViewUnpublishedSchedules()
		{
			var applicationFunctions = new Dictionary<string, bool>
			{
				{DefinedRaptorApplicationFunctionPaths.ViewSchedules, false },
				{DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, true },
				{DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, true}
			};

			var result = validateRequestWithPermission(applicationFunctions);
			Assert.AreEqual(true, result);
		}

		private static DefaultClaimSet createtClaimSet(Dictionary<string, bool> applicationFunctions,
			IAuthorizeAvailableData authorizeAvailableData)
		{
			var claimType = TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace
				+ "/AvailableData";
			var availableDataClaim = new Claim(claimType, authorizeAvailableData, Rights.PossessProperty);
			var claims = new List<Claim> {availableDataClaim};

			foreach (var appFunctionPath in applicationFunctions)
			{
				if (!appFunctionPath.Value)
				{
					continue;
				}

				claimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
					"/", appFunctionPath.Key);
				var appFunctionPathClaim = new Claim(claimType, "", Rights.PossessProperty);
				claims.Add(appFunctionPathClaim);
			}

			return new DefaultClaimSet(claims.ToArray());
		}

		private bool validateRequestWithPermission(Dictionary<string, bool> applicationFunctions)
		{
			var requestDateOnly = new DateOnly(baseDate.AddDays(1));
			var shiftTradeSwapDetails = new IShiftTradeSwapDetail[]
			{
				new ShiftTradeSwapDetail(personFrom, personTo, requestDateOnly, requestDateOnly)
			};
			var request = new ShiftTradeRequest(shiftTradeSwapDetails);

			var claimSet = createtClaimSet(applicationFunctions, new AuthorizeMyTeam());
			principal.AddClaimSet(claimSet);

			var result = target.IsSatisfied(request);
			return result;
		}
	}
}