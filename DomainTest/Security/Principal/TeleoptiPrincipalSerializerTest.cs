using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Principal;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class TeleoptiPrincipalSerializerTest
	{

		private static TeleoptiPrincipalSerializable SerializeAndBack(TeleoptiPrincipalSerializable principal, IApplicationData applicationData = null, IBusinessUnitRepository businessUnitRepository = null, IPersonRepository personRepository = null)
		{
			if (applicationData == null)
				applicationData = MockRepository.GenerateMock<IApplicationData>();
			if (businessUnitRepository == null)
				businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			if (personRepository == null)
				personRepository = MockRepository.GenerateMock<IPersonRepository>();

			//var target = new TeleoptiPrincipalXmlFileSerializer(applicationData, businessUnitRepository, personRepository, @"C:\test.xml");
			var target = new TeleoptiPrincipalGZipBase64Serializer(applicationData, businessUnitRepository, personRepository);
			var data = target.Serialize(principal);

			Debug.WriteLine(data.Length);
			Debug.WriteLine(data);

			return target.Deserialize(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldLoadUnsafePerson()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Load(person.Id.Value)).Return(person);

			var deserializedPrincipal = SerializeAndBack(principal, personRepository: personRepository);

			deserializedPrincipal.Person.Should().Be(person);
		}

		[Test]
		public void ShouldSerializePrincipalData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal);

			deserializedPrincipal.PersonId.Should().Be(principal.PersonId);
		}

		[Test]
		public void ShouldSerializeRegionalData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("en-US"));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("sv-SE"));
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);
			principal.Regional = Regional.FromPerson(person);

			var deserializedPrincipal = SerializeAndBack(principal);

			deserializedPrincipal.Regional.TimeZone.StandardName.Should().Be(person.PermissionInformation.DefaultTimeZone().StandardName);
			deserializedPrincipal.Regional.Culture.Should().Be(person.PermissionInformation.Culture());
			deserializedPrincipal.Regional.UICulture.Should().Be(person.PermissionInformation.UICulture());
		}

		[Test]
		public void ShouldSerializeOrganisationData()
		{
			// someone else is setting up state for me. UGH!
			var businessUnit = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
			var team = TeamFactory.CreateTeam("team", "site");
			team.SetId(Guid.NewGuid());
			team.Site.SetId(Guid.NewGuid());
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team);
			person.AddPersonPeriod(personPeriod);
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);
			principal.Organisation = OrganisationMembership.FromPerson(person);

			var deserializedPrincipal = SerializeAndBack(principal);

			deserializedPrincipal.Organisation.IsUser(person).Should().Be.True();
			deserializedPrincipal.Organisation.Periods().Single().StartDate.Should().Be(personPeriod.StartDate);
			deserializedPrincipal.Organisation.Periods().Single().EndDate.Should().Be(personPeriod.EndDate());
			deserializedPrincipal.Organisation.BelongsToBusinessUnit(businessUnit, DateOnly.Today).Should().Be.True();
			deserializedPrincipal.Organisation.BelongsToTeam(team, DateOnly.Today).Should().Be.True();
			deserializedPrincipal.Organisation.BelongsToSite(team.Site, DateOnly.Today).Should().Be.True();
		}

		[Test]
		public void ShouldSerializeIdentityData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application)
			               	{
			               		Ticket = "ticket"
			               	};
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal);
			var deserializedIdentity = deserializedPrincipal.Identity as ITeleoptiIdentity;

			deserializedIdentity.Name.Should().Be(identity.Name);
			deserializedIdentity.WindowsIdentity.Name.Should().Be(WindowsIdentity.GetCurrent().Name); // cheating with this for now..
			deserializedIdentity.TeleoptiAuthenticationType.Should().Be(identity.TeleoptiAuthenticationType);
			deserializedIdentity.Ticket.Should().Be(identity.Ticket);
		}

		[Test]
		public void ShouldPopulateIdentityDataSource()
		{
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSource.Stub(x => x.DataSourceName).Return("data");
			var applicationData = MockRepository.GenerateMock<IApplicationData>();
			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(new[] { dataSource });

			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), dataSource, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal, applicationData);
			var deserializedIdentity = deserializedPrincipal.Identity as ITeleoptiIdentity;

			deserializedIdentity.DataSource.Should().Be.SameInstanceAs(dataSource);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldLoadIdentityBusinessUnit()
		{
			// someone else is setting up state for me. UGH!
			var businessUnit = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			businessUnitRepository.Stub(x => x.Load(businessUnit.Id.Value)).Return(businessUnit);

			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal, null, businessUnitRepository);
			var deserializedIdentity = deserializedPrincipal.Identity as ITeleoptiIdentity;

			deserializedIdentity.BusinessUnit.Should().Be.SameInstanceAs(businessUnit);
		}







		[Test]
		public void ShouldSerializeClaimSets()
		{
			var person = MakePersonWithAllPossibleAvailableDataOptions();
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = TeleoptiPrincipalSerializable.Make(identity, person);
			SetupClaimsFromRoles(person, principal);

			var deserializedPrincipal = SerializeAndBack(principal);

			var actualClaimTypes =
				from claimSet in deserializedPrincipal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			var expectedClaimTypes =
				from claimSet in principal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			actualClaimTypes.Should().Have.SameSequenceAs(expectedClaimTypes);

			var actualResources =
				from claimSet in deserializedPrincipal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			var expectedResources =
				from claimSet in principal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			actualResources.Should().Have.SameSequenceAs(expectedResources);

			var actualRights =
				from claimSet in deserializedPrincipal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			var expectedRights =
				from claimSet in principal.ClaimSets
				from claim in claimSet
				select claim.ClaimType;
			actualRights.Should().Have.SameSequenceAs(expectedRights);
		}

		private static IPerson MakePersonWithAllPossibleAvailableDataOptions()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			Enum.GetValues(typeof(AvailableDataRangeOption))
				.Cast<AvailableDataRangeOption>()
				.ForEach(o =>
				{
					var role = ApplicationRoleFactory.CreateRole("A", "Role");
					role.AvailableData = new AvailableData { AvailableDataRange = o };
					person.PermissionInformation.AddApplicationRole(role);
				});
			return person;
		}

		private static void SetupClaimsFromRoles(IPerson person, TeleoptiPrincipalSerializable principal)
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);
			var functionsForRoleProvider = MockRepository.GenerateMock<IFunctionsForRoleProvider>();
			var applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList;
			applicationFunctions.ForEach(a => a.SetId(Guid.NewGuid()));
			functionsForRoleProvider.Stub(x => x.AvailableFunctions(null, null))
				.IgnoreArguments()
				.Return(applicationFunctions)
				;
			var roleToPrincipalCommand = new RoleToPrincipalCommand(new RoleToClaimSetTransformer(functionsForRoleProvider, new ClaimWithId(null)));
			roleToPrincipalCommand.Execute(principal, null, personRepository);
		}

	}
}