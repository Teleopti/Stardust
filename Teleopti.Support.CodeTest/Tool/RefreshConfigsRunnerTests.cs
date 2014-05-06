using System.Collections.Generic;
using System.IO;
using Rhino.Mocks;
using NUnit.Framework;
using Teleopti.Support.Code.Tool;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture()]
    public class RefreshConfigsRunnerTests
    {
        private MockRepository _mocks;
        private ISettingsFileManager _fileMan;
        private IRefreshConfigFile _refreshConfig;
        private RefreshConfigsRunner _refresher;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _fileMan = _mocks.DynamicMock<ISettingsFileManager>();
            _refreshConfig = _mocks.DynamicMock<IRefreshConfigFile>();
            _refresher = new RefreshConfigsRunner(_fileMan, _refreshConfig);
        }

        [Test]
        public void RefreshThemTest()
        {
            Expect.Call(_fileMan.GetReplaceList()).Return(new List<SearchReplace>());
            Expect.Call(() => _refreshConfig.SplitAndReplace("", new List<SearchReplace>(), false)).IgnoreArguments();
            _mocks.ReplayAll();
			_refresher.RefreshThem(new ModeFile("DEV"));
            _mocks.VerifyAll();
        }

        [Test]
        public void RefreshThemDeployTest()
        {
            Expect.Call(_fileMan.GetReplaceList()).Return(new List<SearchReplace>());
			Expect.Call(() => _refreshConfig.SplitAndReplace("", new List<SearchReplace>(), false)).IgnoreArguments();
            _mocks.ReplayAll();
			_refresher.RefreshThem(new ModeFile("DEPLOY"));
            _mocks.VerifyAll();
        }

        [Test]
        public void RefreshThemTestTest()
        {
            Expect.Call(_fileMan.GetReplaceList()).Return(new List<SearchReplace>());
			Expect.Call(() => _refreshConfig.SplitAndReplace("", new List<SearchReplace>(), false)).IgnoreArguments();
            _mocks.ReplayAll();
			_refresher.RefreshThem(new ModeFile("TEST"));
            _mocks.VerifyAll();
        }

        [Test]
        public void RefreshThemErrorTest()
        {
            Expect.Call(_fileMan.GetReplaceList()).Return(new List<SearchReplace>());
			Expect.Call(() => _refreshConfig.SplitAndReplace("", new List<SearchReplace>(), false)).IgnoreArguments().Throw(new FileNotFoundException("Borta?"));
            _mocks.ReplayAll();
			_refresher.RefreshThem(new ModeFile("TEST"));
            _mocks.VerifyAll();
        }
    }
}
