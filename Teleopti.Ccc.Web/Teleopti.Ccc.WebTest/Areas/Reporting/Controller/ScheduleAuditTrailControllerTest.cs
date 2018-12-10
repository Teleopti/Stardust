using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Reports.Controllers;


namespace Teleopti.Ccc.WebTest.Areas.Reporting.Controller
{

	[TestFixture, DomainTest]
	public class ScheduleAuditTrailControllerTest : IIsolateSystem, IExtendSystem
	{
		public ScheduleAuditTrailController Target;
		public FakeTeamRepository TeamRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public Global.FakePermissionProvider PermissionProvider;
	
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ScheduleAuditTrailController>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
		}

		[Test]
		public void ShouldReturnPermittedSitesAndTeams()
		{

			var mainPage = new ReadOnlyGroupPage
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};
			var businessUnitId = Guid.NewGuid();
			var team = TeamFactory.CreateTeam("Team", "Site").WithId();
			team.Site.WithId();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team.SiteAndTeam,
					SiteId = team.Site.Id,
					TeamId =  team.Id,
					GroupId = team.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team);
			GroupingReadOnlyRepository.Has(new[] { mainPage }, groupDetails);
			var dateOnly = new DateOnly(2017, 11, 01);
			PermissionProvider.Enable();
			PermissionProvider.PermitGroup(DefinedRaptorApplicationFunctionPaths.ScheduleAuditTrailWebReport,
				dateOnly, new PersonAuthorization
				{
					SiteId = team.Site.Id,
					TeamId = team.Id,
					BusinessUnitId = businessUnitId
				});

			var result = Target.OrganizationSelectionAuditTrail(new ValidPeriod
			{
				StartDate = dateOnly.Date,
				EndDate = dateOnly.Date
			});

			result.Length.Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo("Site");
			result.First().Id.Should().Be.EqualTo(team.Site.Id);
			result.First().Children.Count.Should().Be.EqualTo(1);
			result.First().Children.First().Name.Should().Be.EqualTo("Team");
			result.First().Children.First().Id.Should().Be.EqualTo(team.Id);
		}

		[Test]
		public void ShouldNotReturnNotPermittedSitesAndTeams()
		{
			var mainPage = new ReadOnlyGroupPage
			{
				PageName = "Main",
				PageId = Group.PageMainId
			};
			var businessUnitId = Guid.NewGuid();
			var team = TeamFactory.CreateTeam("Team", "Site").WithId();
			team.Site.WithId();
			var groupDetails = new List<ReadOnlyGroupDetail>
			{
				new ReadOnlyGroupDetail
				{
					PageId = Group.PageMainId,
					GroupName = team.SiteAndTeam,
					SiteId = team.Site.Id,
					TeamId =  team.Id,
					GroupId = team.Id.GetValueOrDefault(),
					BusinessUnitId = businessUnitId
				}
			};
			TeamRepository.Add(team);
			GroupingReadOnlyRepository.Has(new[] { mainPage }, groupDetails);
			PermissionProvider.Enable();

			var result = Target.OrganizationSelectionAuditTrail(new ValidPeriod
			{
				StartDate = DateTime.Now,
				EndDate = DateTime.Now
			});

			result.Length.Should().Be.EqualTo(0);
		}
	}
}
