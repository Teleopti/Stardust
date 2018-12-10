using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Matrix
{
	[TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class TeamResolverForPersonAndApplicationRoleTest
	{
		private TeamResolver _target;
		private IPerson _person;
		private IApplicationRole _applicationRole;
		private ITeam _personsTeam;
		private ITeam _otherTeam;
		private DateOnly _dateOnly;
		private IAvailableData _availableData;
		private readonly IList<ISite> _siteCollection = new List<ISite>();
		MatrixPermissionHolder _item1;
		MatrixPermissionHolder _item2;
		MatrixPermissionHolder _item3;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(2010, 01, 01);
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(1998, 01, 01), new List<ISkill>());
			_personsTeam = TeamFactory.CreateSimpleTeam("TeamPerson");
			_otherTeam = TeamFactory.CreateSimpleTeam("TeamOther");

			_person.PersonPeriodCollection[0].Team = _personsTeam;
			_applicationRole = ApplicationRoleFactory.CreateRole("Hello", "Hello");
			_availableData = new AvailableData();
			_applicationRole.AvailableData = _availableData;
			_applicationRole.AvailableData.AddAvailableTeam(_otherTeam);
			_target = new TeamResolver(_person, _siteCollection);
			_item1 = new MatrixPermissionHolder(_person, _personsTeam, true);
			_item2 = new MatrixPermissionHolder(_person, _personsTeam, false);
			_item3 = new MatrixPermissionHolder(_person, _otherTeam, false);
			CreateSiteAndTeamsInStateHolderBusinessUnit();
		}

		[Test]
		public void VerifyResolveTeamsWithEveryoneAvailable()
		{
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.Everyone;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);
			Assert.AreEqual(3, result.Count);

			Assert.IsTrue(result.Contains(_item1));
			Assert.IsTrue(result.Contains(_item2));
			Assert.IsTrue(result.Contains(_item3));
		}

		[Test]
		public void VerifyResolveTeamsWithPermissionsOnlyToOtherTeam()
		{
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.None;

			Assert.AreEqual(1, _applicationRole.AvailableData.AvailableTeams.Count);
			Assert.IsTrue(_applicationRole.AvailableData.AvailableTeams.Contains(_item3.Team));

			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);

			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Contains(_item3));
		}

		[Test]
		public void VerifyResolveTeamsWithMyBusinessUnitAvailable()
		{
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);
			Assert.AreEqual(3, result.Count);

			Assert.IsTrue(result.Contains(_item1));
			Assert.IsTrue(result.Contains(_item2));
			Assert.IsTrue(result.Contains(_item3));
		}

		[Test]
		public void VerifyResolveMySiteAvailable()
		{
			_applicationRole.AvailableData.DeleteAvailableTeam(_otherTeam);
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);
			Assert.AreEqual(2, result.Count);

			Assert.IsTrue(result.Contains(_item1));
			Assert.IsTrue(result.Contains(_item2));
		}

		[Test]
		public void VerifyResolveMyTeamAvailable()
		{
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyTeam;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);
			Assert.AreEqual(3, result.Count);

			Assert.IsTrue(result.Contains(_item1));
			Assert.IsTrue(result.Contains(_item2));
			Assert.IsTrue(result.Contains(_item3)); // Other team (than MyTeam) that person has permissions for
		}

		[Test]
		public void VerifyResolveMyOwnOrNoneAvailable()
		{
			_applicationRole.AvailableData.DeleteAvailableTeam(_otherTeam);
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);

			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Contains(_item1));
		}

		[Test]
		public void VerifyResolvePersonHasNoTeam()
		{
			_person.PersonPeriodCollection[0].Team = null;
			_applicationRole.AvailableData.AvailableDataRange = AvailableDataRangeOption.MyTeam;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);

			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Contains(_item3)); // Other team (than MyTeam) that person has permissions for
		}

		private void CreateSiteAndTeamsInStateHolderBusinessUnit()
		{
			_siteCollection.Clear();
			_siteCollection.Add(SiteFactory.CreateSimpleSite("PersonsSite"));
			_siteCollection[0].AddTeam(_personsTeam);

			_siteCollection.Add(SiteFactory.CreateSimpleSite("SiteB"));
			_siteCollection[1].AddTeam(_otherTeam);
		}

		[Test]
		public void VerifyResolveTeamsWithNoAvailableData()
		{
			_applicationRole.AvailableData = null;
			HashSet<MatrixPermissionHolder> result = _target.ResolveTeams(_applicationRole, _dateOnly);
			Assert.AreEqual(0, result.Count);
		}
	}
}
