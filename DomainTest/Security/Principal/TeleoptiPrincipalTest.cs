﻿using System;
using System.Linq;
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
	[TestWithStaticDependenciesAvoidUse]
	public class TeleoptiPrincipalTest
    {
        private IPerson person;
        private ITeleoptiIdentity identity;
        private TeleoptiPrincipal target;

        [SetUp]
        public void Setup()
        {
            person = PersonFactory.CreatePerson();
			identity = new TeleoptiIdentity("test", null, null, null, null);

            target = new TeleoptiPrincipal(identity, person);
        }

        [Test]
        public void ShouldChangePrincipalContents()
        {
            var newPersonId = Guid.NewGuid();
            var newPerson = PersonFactory.CreatePerson();
            newPerson.SetId(newPersonId);
            
            var mocks = new MockRepository();
            var personRepository = mocks.DynamicMock<IPersonRepository>();
            using (mocks.Record())
            {
                Expect.Call(personRepository.Get(newPersonId)).Return(newPerson);
            }
            using (mocks.Playback())
            {
				var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
                var newTarget = new TeleoptiPrincipal(newIdentity, newPerson);

                target.ChangePrincipal(newTarget);

                Assert.AreEqual(newPerson, target.GetPerson(personRepository));
                Assert.AreEqual(newIdentity, target.Identity);
            }
        }

        [Test]
        public void ShouldHaveRegionalInformationFromPerson()
        {
            target.Regional.TimeZone.Should().Be.EqualTo(person.PermissionInformation.DefaultTimeZone());
            target.Regional.UICulture.LCID.Should().Be.EqualTo(person.PermissionInformation.UICulture().LCID);
            target.Regional.Culture.LCID.Should().Be.EqualTo(person.PermissionInformation.Culture().LCID);
        }

        [Test]
        public void ShouldHaveOrganisationBelongingsFromPerson()
        {
            ISite site = SiteFactory.CreateSiteWithOneTeam();
            site.SetId(Guid.NewGuid());
            site.TeamCollection[0].SetId(Guid.NewGuid());
            person.SetId(Guid.NewGuid());

            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2011, 5, 1), site.TeamCollection[0]);
            person.AddPersonPeriod(personPeriod);
            
            target = new TeleoptiPrincipal(identity,person);

            target.Organisation.BelongsToBusinessUnit(personPeriod.Team.BusinessUnitExplicit,new DateOnly(2011,5,1)).Should().Be.True();
            target.Organisation.BelongsToSite(personPeriod.Team.Site,new DateOnly(2011,5,1)).Should().Be.True();
            target.Organisation.BelongsToTeam(personPeriod.Team,new DateOnly(2011,5,1)).Should().Be.True();
            target.Organisation.IsUser(person).Should().Be.True();
            target.Organisation.Periods().Count().Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldNotHaveOrganisationBelongingsFromPersonIfNoPersonPeriods()
        {
            target.Organisation.BelongsToBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest, new DateOnly(2011,5,2)).Should().Be.False();
        }
    }
}
