using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class GamificationSettingProviderTest
    {
        private MockRepository _mocks;
        private IGamificationSettingProvider _target;
        private IGamificationSettingRepository _gamificationSettingRepository;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_gamificationSettingRepository = _mocks.StrictMock<IGamificationSettingRepository>();
			_target = new GamificationSettingProvider(_gamificationSettingRepository);
        }

        [Test]
        public void VerifyCanGetAllGamificationSettingsWithNullSettingIncluded()
        {
            var result = new List<IGamificationSetting> { _mocks.StrictMock<IGamificationSetting>() };
            using (_mocks.Record())
            {
                Expect.Call(_gamificationSettingRepository.LoadAll()).Return(result);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.GetGamificationSettingsEmptyIncluded().Contains(GamificationSettingProvider.NullGamificationSetting));
            }
        }
    }
}
