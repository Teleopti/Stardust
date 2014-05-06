using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Teleopti.Support.Code.Tool;
using Rhino.Mocks;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class RefreshConfigFileTest
    {
        private const string OldFile = @"..\..\..\Teleopti.Analytics\Teleopti.Analytics.Etl.ConfigTool\App.config";
        private const string Newfile = @"BuildArtifacts\AppETLTool.config";
        MockRepository _mock = new MockRepository();
        private IConfigFileTagReplacer _replacer;
        RefreshConfigFile _refresher;
        private List<SearchReplace> _lst;
        private IMachineKeyChecker _machineKeyChecker;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _replacer = _mock.DynamicMock<IConfigFileTagReplacer>();
            _machineKeyChecker = _mock.DynamicMock<IMachineKeyChecker>();
            _refresher = new RefreshConfigFile(_replacer, _machineKeyChecker);
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
        public void ShouldGetCurrentDirectoryFromNoPath()
        {
            Assert.That(_refresher.GetDirectories(@"newfile.config"), Is.EqualTo(@"."));
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
			_refresher.SplitAndReplace(files, _lst, true);
        }

        [Test]
        public void ShouldOverWriteTest()
        {
            const string files = @"..\..\..\dummy.config,BuildArtifacts\AppETLTool.config";
            Expect.Call(
                () => _replacer.ReplaceTags(@"..\..\..\dummy.config", _lst)).IgnoreArguments();
            Expect.Call(_machineKeyChecker.CheckForMachineKey(@"..\..\..\dummy.config")).Return(false).IgnoreArguments();
            _mock.ReplayAll();

            _refresher.SplitAndReplace(files, _lst, false);
            _mock.VerifyAll();
        }
    }

    
}

