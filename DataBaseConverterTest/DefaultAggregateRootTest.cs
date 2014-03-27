using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest
{
    /// <summary>
    /// Tests for DefaultAggregateRootTest
    /// </summary>
    [TestFixture]
    public class DefaultAggregateRootTest
    {
        private DefaultAggregateRoot target;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new DefaultAggregateRoot();
        }

        /// <summary>
        /// Verifies the business unit property.
        /// </summary>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-10-22
        /// </remarks>
        [Test]
        public void VerifyBusinessUnitProperty()
        {
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            target.BusinessUnit = bu;
            Assert.AreSame(bu, target.BusinessUnit);
        }

        [Test]
        public void VerifyApplicationRoleProperty()
        {
            ApplicationRole a = new ApplicationRole();
            target.BusinessUnitAdministratorRole = a;
            Assert.AreSame(a, target.BusinessUnitAdministratorRole);
        }

        [Test]
        public void VerifyAdministratorRoleProperty()
        {
            ApplicationRole a = new ApplicationRole();
            target.AdministratorRole = a;
            Assert.AreSame(a, target.AdministratorRole);
        }

        [Test]
        public void VerifyTeamLeaderRoleProperty()
        {
            ApplicationRole a = new ApplicationRole();
            target.TeamLeaderRole = a;
            Assert.AreSame(a, target.TeamLeaderRole);
        }

        [Test]
        public void VerifySiteManagerRoleProperty()
        {
            ApplicationRole a = new ApplicationRole();
            target.SiteManagerRole = a;
            Assert.AreSame(a, target.SiteManagerRole);
        }

        [Test]
        public void VerifyAgentRoleProperty()
        {
            ApplicationRole a = new ApplicationRole();
            target.AgentRole = a;
            Assert.AreSame(a, target.AgentRole);
        }

        [Test]
        public void VerifySkillTypeInboundTelephony()
        {
            ISkillType skillType = new SkillTypePhone(new Description("aa"), ForecastSource.InboundTelephony);
            target.SkillTypeInboundTelephony = skillType;
            Assert.AreSame(skillType, target.SkillTypeInboundTelephony);
        }

        [Test]
        public void VerifySkillTypeTime()
        {
            ISkillType skillType = new SkillTypePhone(new Description("aa"), ForecastSource.Time);
            target.SkillTypeTime = skillType;
            Assert.AreSame(skillType, target.SkillTypeTime);
        }

        [Test]
        public void VerifySkillTypeEmail()
        {
            ISkillType skillType = new SkillTypeEmail(new Description("aa"), ForecastSource.Email);
            target.SkillTypeEmail = skillType;
            Assert.AreSame(skillType, target.SkillTypeEmail);
        }

        [Test]
        public void VerifySkillTypeBackoffice()
        {
            ISkillType skillType = new SkillTypePhone(new Description("aa"), ForecastSource.Backoffice);
            target.SkillTypeBackoffice = skillType;
            Assert.AreSame(skillType, target.SkillTypeBackoffice);
        }
        
        [Test]
        public void VerifySkillTypeProject()
        {
            ISkillType skillType = new SkillTypeEmail(new Description("aa"), ForecastSource.Time);
            target.SkillTypeProject = skillType;
            Assert.AreSame(skillType, target.SkillTypeProject);
        }

        [Test]
        public void VerifySkillTypeFax()
        {
            ISkillType skillType = new SkillTypeEmail(new Description("aa"), ForecastSource.Facsimile);
            target.SkillTypeFax = skillType;
            Assert.AreSame(skillType, target.SkillTypeFax);
        }

        //[Test]
        //public void VerifyApplicationFunctionProperty()
        //{
        //    IApplicationFunction a = ApplicationFunctionFactory.CreateApplicationFunction("TEST");
        //    target.OpenRaptorApplicationFunction = a;
        //    Assert.AreSame(a, target.OpenRaptorApplicationFunction);
        //}
    }
}