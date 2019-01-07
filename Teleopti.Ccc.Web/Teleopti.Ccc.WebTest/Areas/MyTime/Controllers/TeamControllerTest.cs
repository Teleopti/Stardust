using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class TeamControllerTest
	{
		[Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeams()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			var target = new TeamController(viewModelFactory, new Now(), null);

			viewModelFactory.Stub(x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)).Return(new List<SelectOptionItem>());

			target.TeamsForShiftTrade(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
		}

		[Test]
		public void ShouldReturnTeamOptionsAsJsonForShiftTrade()
		{
			var teams = new[] { new SelectOptionItem() };
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			viewModelFactory.Stub(
				x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			                .Return(teams);

			var target = new TeamController(viewModelFactory, new Now(), null);

			var result = target.TeamsForShiftTrade(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectOptionItem>;
			data.Should().Have.SameValuesAs(teams);
		}

		[Test]
		public void ShouldReturnTeamOptionsAsJsonForShiftTradeBoard()
		{
			var teamRepository = new FakeTeamRepository();
			var site = new Domain.Common.Site("mysite");
			var team = new Team {Site = site }.WithDescription(new Description("myteam")).WithId();
			teamRepository.Add(team);

			var person = PersonFactory.CreatePerson().WithId();
			var identity = new TeleoptiIdentity("test", null, null, null, null);

			var mockAuthorize = MockRepository.GenerateMock<IAuthorizeAvailableData>();
			mockAuthorize.Stub(m => m.Check(new OrganisationMembership(), DateOnly.Today, team)).IgnoreArguments().Return(true);

			var searchProvider = MockRepository.GenerateMock<IPeopleSearchProvider>();
			searchProvider.Stub(m => m.FindPersonIds(DateOnly.Today,new Guid[]{}, null)).IgnoreArguments()
				.Return(new List<Guid> {person.Id.Value});

			var claimSet =
				new DefaultClaimSet(
					new System.IdentityModel.Claims.Claim(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace +
						"/Raptor/MyTimeWeb/ShiftTradeBulletinBoard", "true", Rights.PossessProperty),
					new System.IdentityModel.Claims.Claim(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace +
						"/AvailableData", mockAuthorize, Rights.PossessProperty)
					);

			var teleoptiPrincipal = new TeleoptiPrincipal(identity, person);
			teleoptiPrincipal.AddClaimSet(claimSet);

			var authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(teleoptiPrincipal));
			var permissionProvider = new PermissionProvider(authorization);

			var teamProvider = new TeamProvider(teamRepository, permissionProvider, searchProvider);

			var viewModelFactory = new TeamViewModelFactory(teamProvider, null, null, null);
			var target = new TeamController(viewModelFactory, new Now(), null);


			var result = target.TeamsForShiftTradeBoard(DateOnly.Today);
			var data = result.Data as IEnumerable<SelectOptionItem>;
			var selectOption = data.FirstOrDefault();
			selectOption.text.Should().Equals("mysite/myteam");
		}

		[Test]
		public void ShouldReturnSiteOptionsAsJsonForShiftTradeBoardWithoutBU()
		{
			var viewModelFactory = createSiteViewModelFactory("mysite");
			var target = new TeamController(null, new Now(), viewModelFactory);

			var result = target.SitesForShiftTrade(DateOnly.Today);
			var data = result.Data as IEnumerable<SelectOptionItem>;
			var selectOption = data.FirstOrDefault();
			selectOption.text.Should().Be.EqualTo("mysite");
		}

		private ISiteViewModelFactory createSiteViewModelFactory(string siteName)
		{
			var businessUnit = new BusinessUnit("myBU");
			var site = new Site(siteName);
			site.SetId(Guid.NewGuid());
			site.SetBusinessUnit(businessUnit);
			var identity = new TeleoptiIdentity("test", null, null, null, null);
			var person = PersonFactory.CreatePerson();
			var team1 = new Team();
			team1.Site = site;
			var teamRepository = new FakeTeamRepository(team1);
			var mockAuthorize = MockRepository.GenerateMock<IAuthorizeAvailableData>();
			mockAuthorize.Stub(m => m.Check(new OrganisationMembership(), DateOnly.Today, team1)).IgnoreArguments().Return(true);
			var claimSet =
				new DefaultClaimSet(
					new System.IdentityModel.Claims.Claim(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace +
						"/Raptor/MyTimeWeb/ShiftTradeRequests", "true", Rights.PossessProperty),
					new System.IdentityModel.Claims.Claim(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace +
						"/AvailableData", mockAuthorize, Rights.PossessProperty)
					);

			var teleoptiPrincipal = new TeleoptiPrincipal(identity, person);
			teleoptiPrincipal.AddClaimSet(claimSet);
			var authorization = new PrincipalAuthorization(new FakeCurrentTeleoptiPrincipal(teleoptiPrincipal));
			var permissionProvider = new PermissionProvider(authorization);
			var siteProvider = new SiteProvider(permissionProvider, teamRepository);
			var viewModelFactory = new SiteViewModelFactory(siteProvider);
			return viewModelFactory;
		}

		[Test]
		public void ShouldGetTeamsUnderGivenSite()
		{
			var expectedId = Guid.NewGuid();
			var expectedResult = new List<SelectOptionItem> { new SelectOptionItem {id = expectedId.ToString(), text = "myTeam"} };
			var viewModelFactory = MockRepository.GenerateMock<ISiteViewModelFactory>();
			viewModelFactory.Stub(x => x.GetTeams(new List<Guid> { new Guid() }, DateOnly.Today, DefinedRaptorApplicationFunctionPaths.TeamSchedule)).IgnoreArguments().Return(expectedResult);

			var target = new TeamController(null, null, viewModelFactory);
			var result = target.TeamsUnderSiteForShiftTrade("00000000-0000-0000-0000-000000000000", DateOnly.Today);

			var data = result.Data as IEnumerable<SelectOptionItem>;
			data.Should().Have.SameValuesAs(expectedResult);
		}
}
}
