using System;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class TeleoptiPrincipalSerializerTest
	{
		[Test]
		public void ShouldSerializeRequiredData()
		{
			var person = PersonFactory.CreatePerson("A", "Person");
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("en-US"));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("sv-SE"));
			var principal = new TeleoptiPrincipalSerializable(new TeleoptiIdentity(person.Name.ToString(), null, null, null, AuthenticationTypeOption.Application), person);
			var claim = new Claim("type", "resource", "right");
			var claimSet = new DefaultClaimSet(ClaimSet.System, new[] {claim});
			principal.AddClaimSet(claimSet);

			var stream = new MemoryStream();
			var target = new TeleoptiPrincipalSerializer();
			target.Serialize(principal, stream);

			stream.Position = 0;
			var readPrincipal = target.Deserialize(stream);

			readPrincipal.PersonId.Should().Be(principal.PersonId);
			readPrincipal.ClaimSets.Single().Single().ClaimType.Should().Be.EqualTo(claim.ClaimType);
			readPrincipal.ClaimSets.Single().Single().Resource.Should().Be.EqualTo(claim.Resource);
			readPrincipal.ClaimSets.Single().Single().Right.Should().Be.EqualTo(claim.Right);
			readPrincipal.Regional.TimeZone.StandardName.Should().Be(person.PermissionInformation.DefaultTimeZone().StandardName);
			readPrincipal.Regional.Culture.Should().Be(person.PermissionInformation.Culture());
			readPrincipal.Regional.UICulture.Should().Be(person.PermissionInformation.UICulture());
			readPrincipal.Organisation.IsUser(person).Should().Be.True();
		}

	}
}