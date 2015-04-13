using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Common
{
    [TestFixture]
    [Category("LongRunning")]
    public class CommandLineArgumentTest
    {
        private CommandLineArgument _target;

        [SetUp]
        public void Setup()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("-SSPeterWe,");                 // Source Server Name.
            stringBuilder.Append("-SDTPS_REPORT,");              // Source Database Name.
            stringBuilder.Append("-SUUserName,");                // Source User Name.
            stringBuilder.Append("-SPPassWord,");                // Source Password.
            stringBuilder.Append("-DSDestServer,");              // Destination Server Name.
            stringBuilder.Append("-DDDestDatabase,");            // Destination Database Name.
            stringBuilder.Append("-DUDestUser,");                // Destination User Name.
            stringBuilder.Append("-DPDestPassWord,");            // Destination Password.
            stringBuilder.Append("-TZW. Europe Standard Time,"); // TimeZone.
            stringBuilder.Append("-FD2004-12-01,");              // Date From.
            stringBuilder.Append("-TD2006-12-01,");              // Date To.
            stringBuilder.Append("-BUBusinessUnit,");            // BusinessUnit Name.
            stringBuilder.Append("-CO,");                        // Convert.
            stringBuilder.Append("-CUkn-IN,");                   // Culture.
            stringBuilder.Append("-DR15");                      // Force merge of Default Resolution to n.
            //stringBuilder.Append("-EE");                         // Use Integrated security
            var arguments = stringBuilder.ToString().Split(",");
            _target = new CommandLineArgument(arguments);
        }

        [Test]
        public void VerifyCanCreateInstanceAndInjectArray()
        {
            CommandLineArgument commandLineArgument = new CommandLineArgument(new string[] { "-SSPeterWe" });
            Assert.IsNotNull(commandLineArgument);
        }

        [Test]
        public void VerifyCanGetProperties()
        {
            Assert.AreEqual("PeterWe", _target.SourceServer);
            Assert.AreEqual("TPS_REPORT", _target.SourceDatabase);
            Assert.AreEqual("UserName", _target.SourceUserName);
            Assert.AreEqual("PassWord", _target.SourcePassword);
            Assert.AreEqual("DestServer", _target.DestinationServer);
            Assert.AreEqual("DestDatabase", _target.DestinationDatabase);
            Assert.AreEqual("DestUser", _target.DestinationUserName);
            Assert.AreEqual("DestPassWord", _target.DestinationPassword);
            Assert.AreEqual((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")).Id, _target.TimeZone.Id);
            Assert.AreEqual(DateTime.Parse("2004-12-01", CultureInfo.CurrentCulture), _target.FromDate);
            Assert.AreEqual(DateTime.Parse("2006-12-01", CultureInfo.CurrentCulture), _target.ToDate);
            Assert.AreEqual("BusinessUnit", _target.BusinessUnit);
            Assert.AreEqual("kn-IN", _target.CultureInfo.ToString());
            Assert.AreEqual(15, _target.DefaultResolution);
            Assert.IsFalse(_target.UseIntegratedSecurity);
        }

        [Test]
        public void CanUseHyphenAndBlanksInArguments()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(@"-SSTRG-TECH-SSOMAIR\TeleoptiForecast,");                 // Source Server Name.
            stringBuilder.Append("-SDTPS_REPORT-2,");              // Source Database Name.
            stringBuilder.Append("-SUUser-Name,");                // Source User Name.
            stringBuilder.Append("-SPPass-Word,");                // Source Password.
            stringBuilder.Append("-DSDest-DServer-3,");              // Destination Server Name.
            stringBuilder.Append("-DDDest-DDatabase-5,");            // Destination Database Name.
            stringBuilder.Append("-DUDest-User,");                // Destination User Name.
            stringBuilder.Append("-DPDest-PassWord,");            // Destination Password.
            stringBuilder.Append("-TZW. Europe Standard Time,"); // TimeZone.
            stringBuilder.Append("-FD2004-12-01,");              // Date From.
            stringBuilder.Append("-TD2006-12-01,");              // Date To.
            stringBuilder.Append("-BUBusi ness-4-Unit,");            // BusinessUnit Name.
            stringBuilder.Append("-CO,");                        // Convert.
            stringBuilder.Append("-CUkn-IN,");                   // Culture.
            stringBuilder.Append("-DR15");                      // Force merge of Default Resolution to n.
            //stringBuilder.Append("-EE");                         // Use Integrated security
            var arguments = stringBuilder.ToString().Split(",");
            _target = new CommandLineArgument(arguments);
                    
            Assert.AreEqual(@"TRG-TECH-SSOMAIR\TeleoptiForecast",_target.SourceServer);              
            Assert.AreEqual("TPS_REPORT-2", _target.SourceDatabase);          
            Assert.AreEqual("User-Name", _target.SourceUserName);              
            Assert.AreEqual("Pass-Word", _target.SourcePassword);              
            Assert.AreEqual("Dest-DServer-3", _target.DestinationServer);          
            Assert.AreEqual("Dest-DDatabase-5", _target.DestinationDatabase);        
            Assert.AreEqual("Dest-User", _target.DestinationUserName);             
            Assert.AreEqual("Dest-PassWord", _target.DestinationPassword);         
            Assert.AreEqual("Busi ness-4-Unit", _target.BusinessUnit);       
        }

        [Test]
        public void CanGetBothConnectionStrings()
        {
            string connectionStringSource = "Data Source=PeterWe;Initial Catalog=TPS_REPORT;User ID=UserName;Password=PassWord";
            Assert.AreEqual(connectionStringSource, _target.SourceConnectionString);

            string connectionStringDestination = "Data Source=DestServer;Initial Catalog=DestDatabase;User ID=DestUser;Password=DestPassWord";
            Assert.AreEqual(connectionStringDestination, _target.DestinationConnectionString);
        }

        [Test]
        public void CanGetIntegratedSecurityConnectionString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("-SSPeterWe,");                 // Source Server Name.
            stringBuilder.Append("-SDTPS_REPORT,");              // Source Database Name.
            stringBuilder.Append("-SUUserName,");                // Source User Name.
            stringBuilder.Append("-SPPassWord,");                // Source Password.
            stringBuilder.Append("-DSDestServer,");              // Destination Server Name.
            stringBuilder.Append("-DDDestDatabase,");            // Destination Database Name.
            stringBuilder.Append("-DUDestUser,");                // Destination User Name.
            stringBuilder.Append("-DPDestPassWord,");            // Destination Password.
            stringBuilder.Append("-TZW. Europe Standard Time,"); // TimeZone.
            stringBuilder.Append("-FD2004-12-01,");              // Date From.
            stringBuilder.Append("-TD2006-12-01,");              // Date To.
            stringBuilder.Append("-BUBusinessUnit,");            // BusinessUnit Name.
            stringBuilder.Append("-CO,");                        // Convert.
            stringBuilder.Append("-CUkn-IN,");                   // Culture.
            stringBuilder.Append("-DR15,");                      // Force merge of Default Resolution to n.
            stringBuilder.Append("-EE");                         // Use Integrated security
			var arguments = stringBuilder.ToString().Split(",");
            ICommandLineArgument commandLineArgument = new CommandLineArgument(arguments);

            string connectionStringSource = "Data Source=PeterWe;Initial Catalog=TPS_REPORT;Integrated Security=True";
            Assert.AreEqual(connectionStringSource, commandLineArgument.SourceConnectionString);

            string connectionStringDestination = "Data Source=DestServer;Initial Catalog=DestDatabase;Integrated Security=True";
            Assert.AreEqual(connectionStringDestination, commandLineArgument.DestinationConnectionString);
        }

        [Test]
        public void VerifyDefaultsAreSet()
        {
            var arguments = new string[12];
            arguments[0] = "-SSPeterWe";   // Source Server Name.
            arguments[1] = "-SDTPS_REPORT";   // Source Database Name.
            arguments[2] = "-SUUserName";   // Source User Name.
            arguments[3] = "-SPPassWord";   // Source Password.

            arguments[4] = "-DSDestServer";   // Destination Server Name.
            arguments[5] = "-DDDestDatabase";   // Destination Database Name.
            arguments[6] = "-DUDestUser";   // Destination User Name.
            arguments[7] = "-DPDestPassWord";   // Destination Password.

            arguments[8] = "-TZW. Europe Standard Time";   // TimeZone.
            arguments[9] = "-FD2004-12-01";   // Date From.
            arguments[10] = "-TD2004-12-01";  // Date To.
            arguments[11] = "-BUBusinessUnit";  // BusinessUnit Name.

            CommandLineArgument commandLineArgument = new CommandLineArgument(arguments);

            //Defaults
            Assert.AreEqual(CultureInfo.GetCultureInfo("en-US"), commandLineArgument.CultureInfo);
        }
    }
}
