using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Common
{
    [TestFixture]
    [Category("LongRunning")]
    public class DatabaseHandlerTest
    {
        private DatabaseHandler _target;
        private MockRepository _mocks;
        private ICommandLineArgument _commandLineArgument;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _commandLineArgument = _mocks.StrictMock<ICommandLineArgument>();
 
            using (_mocks.Record())
            {
                Expect.Call(_commandLineArgument.DestinationConnectionString)
					.Return(IniFileInfo.ConnectionString)
                    .Repeat.Any();
                
                Expect.Call(_commandLineArgument.DestinationServer)
					.Return(IniFileInfo.SQL_SERVER_NAME);

                Expect.Call(_commandLineArgument.DestinationDatabase)
					.Return(IniFileInfo.DB_CCC7)
                    .Repeat.Any();

                Expect.Call(_commandLineArgument.DestinationUserName)
				   .Return(IniFileInfo.SQL_LOGIN);
                
                Expect.Call(_commandLineArgument.DestinationPassword)
				   .Return(IniFileInfo.SQL_PASSWORD);
                    
                      
            }
            _target = new DatabaseHandler(_commandLineArgument);
        }

        [Test]
        public void VerifyCanGetProperties()
        {
            Assert.IsNotNull(DatabaseHandler.DataSourceSettings(_commandLineArgument.DestinationConnectionString));
            Assert.IsNotNull(_target.SessionFactory);
        }
    }
}
