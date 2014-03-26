using System;
using System.Globalization;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Creators
{
    [TestFixture]
    [Category("LongRunning")]
    public class DefaultDataCreatorTest
    {
        private DefaultDataCreator _target;

        [SetUp]
        public void Setup()
        {
            CommandLineArgument commandLineArgument = createFakeArguments();
            string businessUnitName = commandLineArgument.BusinessUnit;
            CultureInfo cultureInfo = commandLineArgument.CultureInfo;

            ISessionFactory sessionFactory = SetupFixtureForAssembly.SessionFactory;
            _target = new DefaultDataCreator(businessUnitName, cultureInfo, (TimeZoneInfo.Local), "username", "password", sessionFactory);
        }

        [Test]
        public void VerifyCanCreateDefaultData()
        {
            //DefaultAggregateRoot might be moved to a better namespace
            DefaultAggregateRoot defaultAggregateRoot =  _target.Create();
            // set a fake user translator to avoid implicite text translation
            IUserTextTranslator fakeUserTextTranslator = new FakeUserTextTranslator();

            Assert.AreEqual("Administrator",defaultAggregateRoot.AdministratorRole.Name);
            ((ApplicationRole) defaultAggregateRoot.AdministratorRole).UserTextTranslator = fakeUserTextTranslator;
            Assert.AreEqual("xxBuiltInAdministratorRole", defaultAggregateRoot.AdministratorRole.DescriptionText);

            Assert.AreEqual("Business Unit Administrator",defaultAggregateRoot.BusinessUnitAdministratorRole.Name);
            ((ApplicationRole)defaultAggregateRoot.BusinessUnitAdministratorRole).UserTextTranslator = fakeUserTextTranslator;
            Assert.AreEqual("xxBuiltInBusinessUnitAdministratorRole", defaultAggregateRoot.BusinessUnitAdministratorRole.DescriptionText);

            Assert.AreEqual("Site Manager",defaultAggregateRoot.SiteManagerRole.Name);
            ((ApplicationRole)defaultAggregateRoot.SiteManagerRole).UserTextTranslator = fakeUserTextTranslator;
            Assert.AreEqual("xxBuiltInSiteManagerRole", defaultAggregateRoot.SiteManagerRole.DescriptionText);

            Assert.AreEqual("Team Leader", defaultAggregateRoot.TeamLeaderRole.Name);
            ((ApplicationRole)defaultAggregateRoot.TeamLeaderRole).UserTextTranslator = fakeUserTextTranslator;
            Assert.AreEqual("xxBuiltInTeamLeaderRole", defaultAggregateRoot.TeamLeaderRole.DescriptionText);

            Assert.AreEqual("Agent", defaultAggregateRoot.AgentRole.Name);
            ((ApplicationRole)defaultAggregateRoot.AgentRole).UserTextTranslator = fakeUserTextTranslator;
            Assert.AreEqual("xxBuildInStandardAgentRole", defaultAggregateRoot.AgentRole.DescriptionText);

            Assert.AreEqual("BusinessUnit", defaultAggregateRoot.BusinessUnit.Name);

            Assert.AreEqual(new Description("SkillTypeInboundTelephony"), defaultAggregateRoot.SkillTypeInboundTelephony.Description);
            Assert.AreEqual(ForecastSource.InboundTelephony,defaultAggregateRoot.SkillTypeInboundTelephony.ForecastSource);

            Assert.AreEqual(new Description("SkillTypeTime"), defaultAggregateRoot.SkillTypeTime.Description);
            Assert.AreEqual(ForecastSource.Time, defaultAggregateRoot.SkillTypeTime.ForecastSource);
            
            Assert.AreEqual(new Description("SkillTypeEmail"), defaultAggregateRoot.SkillTypeEmail.Description);
            Assert.AreEqual(ForecastSource.Email, defaultAggregateRoot.SkillTypeEmail.ForecastSource);

            Assert.AreEqual(new Description("SkillTypeBackoffice"), defaultAggregateRoot.SkillTypeBackoffice.Description);
            Assert.AreEqual(ForecastSource.Backoffice, defaultAggregateRoot.SkillTypeBackoffice.ForecastSource);

            Assert.AreEqual(new Description("SkillTypeProject"), defaultAggregateRoot.SkillTypeProject.Description);
            Assert.AreEqual(ForecastSource.Time, defaultAggregateRoot.SkillTypeProject.ForecastSource);

            Assert.AreEqual(new Description("SkillTypeFax"), defaultAggregateRoot.SkillTypeFax.Description);
            Assert.AreEqual(ForecastSource.Facsimile, defaultAggregateRoot.SkillTypeFax.ForecastSource);
            
        }

        [Test]
        public void VerifyCanSave()
        {
            DefaultAggregateRoot defaultAggregateRoot =  _target.Create();
            _target.Save(defaultAggregateRoot);
        }

        private static CommandLineArgument createFakeArguments()
        {
            var arguments = new string[16];
            arguments[0] = "-SSPeterWe";   // Source Server Name.
            arguments[1] = "-SDTPS_REPORT";   // Source Database Name.
            arguments[2] = "-SUUserName";   // Source User Name.
            arguments[3] = "-SPPassWord";   // Source Password.

            arguments[4] = "-DSDestServer";   // Destination Server Name.
            arguments[5] = "-DDDestDatabase";   // Destination Database Name.
            arguments[6] = "-DUDestUser";   // Destination User Name.
            arguments[7] = "-DPDestPassWord";   // Destination Password.

            arguments[8] = "-TZW. Europe Standard Time";   // TimeZone.
            arguments[9] = "-FD2005-12-01";   // Date From.
            arguments[10] = "-TD2008-12-02";  // Date To.
            arguments[11] = "-BUBusinessUnit";  // BusinessUnit Name.
            arguments[12] = "-CO";  // Convert.
            arguments[13] = "-CUkn-IN";  // Culture.

        	arguments[14] = "-NAuser2";
        	arguments[15] = "-NPpass2";

            return new CommandLineArgument(arguments);
        }

    }
}