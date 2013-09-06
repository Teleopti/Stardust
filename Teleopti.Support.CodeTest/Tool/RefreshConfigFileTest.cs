using NUnit.Framework;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture()]
    public class RefreshConfigFileTest
    {
        private const string OldFile = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config";
        private const string Newfile = @"ConfigFiles\AppETLTool.config";
        readonly RefreshConfigFile _refresher = new RefreshConfigFile();

        [Test()]
        public void ReplaceFileTest()
        {
            _refresher.ReplaceFile(OldFile, Newfile);
        }

        [Test()]
        public void SplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.SplitAndReplace(files);
        }

        [Test()]
        public void ReadLinesSplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\App.config,ConfigFiles\AppETLService.config
..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\app.config,ConfigFiles\AppRaptor.config
..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.ReadLinesFromString(files);
        }
    }

    
}

