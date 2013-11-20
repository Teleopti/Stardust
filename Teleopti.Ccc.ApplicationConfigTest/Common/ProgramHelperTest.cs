using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Common
{
    [Category("LongRunning")]
    [TestFixture]
    public class ProgramHelperTest
    {
        private DefaultAggregateRoot _defaultAggregateRoot;
        private DatabaseHandler _databaseHandler;
        private ICommandLineArgument _commandLineArgument;
        private ProgramHelper target;
        
        [SetUp]
        public void Setup()
        {
            target = new ProgramHelper();
            logonForTest();
        }

        [Test]
        public void VerifyCanShowUsage()
        {
            Assert.IsNotEmpty(ProgramHelper.UsageInfo);
        }

        [Test]
        public void VerifyCanGetVersion()
        {
            Assert.IsNotEmpty(ProgramHelper.VersionInfo);
        }

        [Test]
        public void CanLogOn()
        {
            target.LogOn(_commandLineArgument, _databaseHandler, _defaultAggregateRoot.BusinessUnit);
            Assert.IsNotNull(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource);
        }

        [Test]
        public void CanCreateNew()
        {
            target.LogOn(_commandLineArgument, _databaseHandler, _defaultAggregateRoot.BusinessUnit);
            target.CreateNewEmptyCcc7(_defaultAggregateRoot);
        }

        private void logonForTest()
        {
            string[] strings = new string[6];
            strings[0] = "-DS" + IniFileInfo.ServerName;
            strings[1] = "-DU" + IniFileInfo.UserName;
            strings[2] = "-DP" + IniFileInfo.Password;
            strings[3] = "-DD" + IniFileInfo.Database;
            strings[4] = "-BUTestBU";
            strings[5] = "-CUkn-IN";

            _commandLineArgument = new CommandLineArgument(strings);

            ISessionFactory factory = SetupFixtureForAssembly.SessionFactory;

            DefaultDataCreator defaultDataCreator = new DefaultDataCreator("Test", _commandLineArgument.CultureInfo, _commandLineArgument.TimeZone, _commandLineArgument.NewUserName, _commandLineArgument.NewUserPassword, factory);

            _defaultAggregateRoot = defaultDataCreator.Create();
            defaultDataCreator.Save(_defaultAggregateRoot);

            _databaseHandler = new DatabaseHandler(_commandLineArgument);
        }

        [Test]
        public void CanGetDefaultAggregateRoot()
        {
            DefaultAggregateRoot defaultAggregateRoot = target.GetDefaultAggregateRoot(_commandLineArgument);
            Assert.IsNotNull(defaultAggregateRoot);
        }

        [TearDown]
        public void Teardown()
        {
            StateHolderProxyHelper.ClearStateHolder();
        }
    }
}