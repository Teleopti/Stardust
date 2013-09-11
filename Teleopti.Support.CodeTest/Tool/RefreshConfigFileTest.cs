using System.Collections.Generic;
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
        private List<SearchReplace> _lst;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _replacer = _mock.DynamicMock<IConfigFileTagReplacer>();
            _refresher = new RefreshConfigFile(_replacer);
            _lst = new List<SearchReplace>();
        }

        [Test]
        public void ReplaceFileTest()
        {
            _refresher.ReplaceFile(OldFile, Newfile, _lst, true);
        }

        [Test]
        public void ShouldGetDirectoriesFromFullPath()
        {
           Assert.That(_refresher.GetDirectories(@"..\TeleoptiCCC\ETL\newfile.config"),Is.EqualTo(@"..\TeleoptiCCC\ETL"));
        }

        [Test]
        public void ShouldCreateDirectoryIfNotExists()
        {
            _refresher.ReplaceFile(@"..\TeleoptiCCC\ETL\newfile.config", Newfile, _lst, true);
        }

        
        [Test]
        public void SplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.SplitAndReplace(files, _lst, true);
        }

        [Test]
        public void ReadLinesSplitAndReplaceTest()
        {
            const string files = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ServiceHost\App.config,ConfigFiles\AppETLService.config
..\..\..\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\app.config,ConfigFiles\AppRaptor.config
..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config,ConfigFiles\AppETLTool.config";
            _refresher.ReadLinesFromString(files, _lst, true);
        }

        [Test]
        public void ShouldOverWriteTest()
        {
            const string files = @"..\..\..\dummy.config,ConfigFiles\AppETLTool.config";
            _refresher.SplitAndReplace(files, _lst, false);
            Expect.Call(
                () => _replacer.ReplaceTags(@"c:\dummy.config", _lst));
        }
    }

    
}

