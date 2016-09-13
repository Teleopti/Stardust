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
using Teleopti.Interfaces.Domain;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class TeamControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnTeamsAndOrGroupingssAsJson()
		{
			var teams = new[] { new SelectGroup() };
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			viewModelFactory.Stub(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today)).Return(teams);

			var target = new TeamController(viewModelFactory, new Now(), null);

			var result = target.TeamsAndOrGroupings(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectGroup>;
			data.Should().Have.SameValuesAs(teams);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeamsAndOrGroupings()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			var target = new TeamController(viewModelFactory, new Now(), null);

			target.TeamsAndOrGroupings(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOrGroupOptionsViewModel(DateOnly.Today));
		}

		[Test]
		public void ShouldUseTodayWhenDateNotSpecifiedForTeams()
		{
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			var target = new TeamController(viewModelFactory, new Now(), null);

			target.TeamsForShiftTrade(null);

			viewModelFactory.AssertWasCalled(x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
		}

		[Test]
		public void ShouldReturnTeamOptionsAsJsonForShiftTrade()
		{
			var teams = new[] { new SelectGroup() };
			var viewModelFactory = MockRepository.GenerateMock<ITeamViewModelFactory>();
			viewModelFactory.Stub(
				x => x.CreateTeamOptionsViewModel(DateOnly.Today, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb))
			                .Return(teams);

			var target = new TeamController(viewModelFactory, new Now(), null);

			var result = target.TeamsForShiftTrade(DateOnly.Today);

			var data = result.Data as IEnumerable<SelectGroup>;
			data.Should().Have.SameValuesAs(teams);
		}

		[Test]
		public void ShouldReturnTeamOptionsAsJsonForShiftTradeBoard()
		{
			var teamRepository = new FakeTeamRepository();
			var site = new Domain.Common.Site("mysite");
			var team = new Team {Site = site, Description = new Description("myteam")};
			teamRepository.Add(team);

			var person = PersonFactory.CreatePerson();
			var identity = new TeleoptiIdentity("test", null, null, null, null);

			var mockAuthorize = MockRepository.GenerateMock<IAuthorizeAvailableData>();
			mockAuthorize.Stub(m => m.Check(new OrganisationMembership(), DateOnly.Today, team)).IgnoreArguments().Return(true);

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

			var teamProvider = new TeamProvider(teamRepository, permissionProvider);

			var viewModelFactory = new TeamViewModelFactory(teamProvider, null, null, null, null);
			var target = new TeamController(viewModelFactory, new Now(), null);

			var result = target.TeamsForShiftTradeBoard(DateOnly.Today);
			var data = result.Data as IEnumerable<ISelectOption>;
			var selectOption = data.FirstOrDefault();
			selectOption.text.Should().Equals("mysite/myteam");
		}

		[Test]
		public void ShouldReturnSitOptionsAsJsonForShiftTradeBoard()
		{
			var siteRepository = new FakeSiteRepository();
			var businessUnit = new BusinessUnit("myBU");
			var site = new Site("mySite");
			site.SetBusinessUnit(businessUnit);
			siteRepository.Add(site);

			var person = PersonFactory.CreatePerson();
			var identity = new TeleoptiIdentity("test", null, null, null, null);

			var mockAuthorize = MockRepository.GenerateMock<IAuthorizeAvailableData>();
			mockAuthorize.Stub(m => m.Check(new OrganisationMembership(), DateOnly.Today, site)).IgnoreArguments().Return(true);

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

			var siteProvider = new SiteProvider(siteRepository, permissionProvider, null);

			var viewModelFactory = new SiteViewModelFactory(siteProvider);
			var target = new TeamController(null, new Now(), viewModelFactory);

			var result = target.SitesForShiftTradeBoard(DateOnly.Today);
			var data = result.Data as IEnumerable<ISelectOption>;
			var selectOption = data.FirstOrDefault();
			selectOption.text.Should().Equals("mysite");
		}

		[Test]
		public void ShouldGetTeamIdsWithGivenSiteIds()
		{
			var expectedResult = new List<Guid> {new Guid()};
			var viewModelFactory = MockRepository.GenerateMock<ISiteViewModelFactory>();
			viewModelFactory.Stub(x => x.GetTeamIds(new List<Guid> {new Guid()})).IgnoreArguments().Return(expectedResult);

			var target = new TeamController(null, null, viewModelFactory);
			var result = target.GetTeamIds("00000000-0000-0000-0000-000000000000");

			result.Data.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldGetTeamsUnderGivenSite()
		{
			var expectedId = Guid.NewGuid();
			var expectedResult = new List<ISelectOption> { new SelectOptionItem {id = expectedId.ToString(), text = "myTeam"} };
			var viewModelFactory = MockRepository.GenerateMock<ISiteViewModelFactory>();
			viewModelFactory.Stub(x => x.GetTeams(new List<Guid> { new Guid() })).IgnoreArguments().Return(expectedResult);

			var target = new TeamController(null, null, viewModelFactory);
			var result = target.TeamsUnderSiteForShiftTrade("00000000-0000-0000-0000-000000000000");

			result.Data.Should().Be.EqualTo(expectedResult);
		}
	}
}
