using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class TeleoptiPrincipalSerializerTest
	{

		
		private static TeleoptiPrincipalSerializable SerializeAndBack(TeleoptiPrincipalSerializable principal, IApplicationData applicationData = null)
		{
			var stream = new MemoryStream();
			var target = new TeleoptiPrincipalSerializer(applicationData);
			target.Serialize(principal, stream);

			stream.Position = 0;
			var readPrincipal = target.Deserialize(stream);
			return readPrincipal;
		}

		[Test]
		public void ShouldSerializePrincipalData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			var identity = new TeleoptiIdentity(person.Name.ToString(), null, null, WindowsIdentity.GetCurrent(), AuthenticationTypeOption.Application);
			var principal = new TeleoptiPrincipalSerializable(identity, person);

			var readPrincipal = SerializeAndBack(principal);

			readPrincipal.PersonId.Should().Be(principal.PersonId);
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

			var readPrincipal = SerializeAndBack(principal);

			readPrincipal.ClaimSets.Single().Single().ClaimType.Should().Be.EqualTo(claim.ClaimType);
			readPrincipal.ClaimSets.Single().Single().Resource.Should().Be.EqualTo(claim.Resource);
			readPrincipal.ClaimSets.Single().Single().Right.Should().Be.EqualTo(claim.Right);
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

			var readPrincipal = SerializeAndBack(principal);

			readPrincipal.Regional.TimeZone.StandardName.Should().Be(person.PermissionInformation.DefaultTimeZone().StandardName);
			readPrincipal.Regional.Culture.Should().Be(person.PermissionInformation.Culture());
			readPrincipal.Regional.UICulture.Should().Be(person.PermissionInformation.UICulture());
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

			var readPrincipal = SerializeAndBack(principal);

			readPrincipal.Organisation.IsUser(person).Should().Be.True();
			readPrincipal.Organisation.Periods().Single().StartDate.Should().Be(personPeriod.StartDate);
			readPrincipal.Organisation.Periods().Single().EndDate.Should().Be(personPeriod.EndDate());
			readPrincipal.Organisation.BelongsToBusinessUnit(businessUnit, DateOnly.Today).Should().Be.True();
			readPrincipal.Organisation.BelongsToTeam(team, DateOnly.Today).Should().Be.True();
			readPrincipal.Organisation.BelongsToSite(team.Site, DateOnly.Today).Should().Be.True();
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

			var readPrincipal = SerializeAndBack(principal);
			var readIdentity = readPrincipal.Identity as ITeleoptiIdentity;

			readIdentity.Name.Should().Be(identity.Name);
			readIdentity.WindowsIdentity.Name.Should().Be(WindowsIdentity.GetCurrent().Name); // cheating with this for now..
			readIdentity.TeleoptiAuthenticationType.Should().Be(identity.TeleoptiAuthenticationType);
			readIdentity.Ticket.Should().Be(identity.Ticket);
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

			var readPrincipal = SerializeAndBack(principal, applicationData);
			var readIdentity = readPrincipal.Identity as ITeleoptiIdentity;

			readIdentity.DataSource.Should().Be.SameInstanceAs(dataSource);
		}

	}
}