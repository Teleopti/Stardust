using NUnit.Framework;
using Teleopti.Support.Code.Tool;
using Rhino.Mocks;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class RefreshConfigFileTest
    {
        private const string OldFile = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config";
        private const string Newfile = @"ConfigFiles\AppETLTool.config";
        MockRepository _mock = new MockRepository();
        private IConfigFileTagReplacer _replacer;
        RefreshConfigFile _refresher;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _replacer = _mock.DynamicMock<IConfigFileTagReplacer>();
            _refresher = new RefreshConfigFile(_replacer);
        }

        [Test]
        public void ReplaceFileTest()
        {
            _refresher.ReplaceFile(OldFile, Newfile);
        }

        [Test]
        public void SplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.SplitAndReplace(files);
        }

        [Test]
        public void ReadLinesSplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\App.config,ConfigFiles\AppETLService.config
..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\app.config,ConfigFiles\AppRaptor.config
..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.ReadLinesFromString(files);
        }
    }

    
}

