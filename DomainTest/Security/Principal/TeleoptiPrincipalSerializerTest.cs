using System;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class TeleoptiPrincipalSerializerTest
	{

		private static TeleoptiPrincipalSerializable SerializeAndBack(TeleoptiPrincipalSerializable principal, IApplicationData applicationData = null, IBusinessUnitRepository businessUnitRepository = null)
		{
			if (applicationData == null)
				applicationData = MockRepository.GenerateMock<IApplicationData>();
			if (businessUnitRepository == null)
				businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();

			var stream = new MemoryStream();
			var target = new TeleoptiPrincipalSerializer(applicationData, businessUnitRepository);
			target.Serialize(principal, stream);

			stream.Position = 0;
			var deserializedPrincipal = target.Deserialize(stream);
			return deserializedPrincipal;
		}

		[Test]
		public void ShouldSerializePrincipalData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = new TeleoptiPrincipalSerializable(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal);

			deserializedPrincipal.PersonId.Should().Be(principal.PersonId);
		}

		[Test]
		public void ShouldSerializeClaimSets()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = new TeleoptiPrincipalSerializable(identity, person);
			var claim = new Claim("type", "resource", "right");
			var claimSet = new DefaultClaimSet(ClaimSet.System, new[] { claim });
			principal.AddClaimSet(claimSet);

			var deserializedPrincipal = SerializeAndBack(principal);

			deserializedPrincipal.ClaimSets.Single().Single().ClaimType.Should().Be.EqualTo(claim.ClaimType);
			deserializedPrincipal.ClaimSets.Single().Single().Resource.Should().Be.EqualTo(claim.Resource);
			deserializedPrincipal.ClaimSets.Single().Single().Right.Should().Be.EqualTo(claim.Right);
		}

		[Test]
		public void ShouldSerializeRegionalData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("en-US"));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("sv-SE"));
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = new TeleoptiPrincipalSerializable(identity, person);

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
			var principal = new TeleoptiPrincipalSerializable(identity, person);

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
			var principal = new TeleoptiPrincipalSerializable(identity, person);

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
			var principal = new TeleoptiPrincipalSerializable(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal, applicationData);
			var deserializedIdentity = deserializedPrincipal.Identity as ITeleoptiIdentity;

			deserializedIdentity.DataSource.Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void ShouldPopulateIdentityBusinessUnit()
		{
			// someone else is setting up state for me. UGH!
			var businessUnit = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;
			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			businessUnitRepository.Stub(x => x.Get(businessUnit.Id.Value)).Return(businessUnit);

			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, businessUnit, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = new TeleoptiPrincipalSerializable(identity, person);

			var deserializedPrincipal = SerializeAndBack(principal, null, businessUnitRepository);
			var deserializedIdentity = deserializedPrincipal.Identity as ITeleoptiIdentity;

			deserializedIdentity.BusinessUnit.Should().Be.SameInstanceAs(businessUnit);
		}

	}
}