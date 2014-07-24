using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class CommonAgentNameProviderTest
	{
		private MockRepository _mocks;
		private ICommonAgentNameProvider _target;
		private IGlobalSettingDataRepository _settingDataRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_settingDataRepository = _mocks.StrictMock<IGlobalSettingDataRepository>();
			_target = new CommonAgentNameProvider(_settingDataRepository);
		}

		[Test]
		public void ShouldGetSettingFromRepositoryOnetime()
		{
			Expect.Call(_settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).Return(
					new CommonNameDescriptionSetting()).IgnoreArguments();
			_mocks.ReplayAll();
			var sett1 = _target.CommonAgentNameSettings;
			var sett2 = _target.CommonAgentNameSettings;
			Assert.That(sett1, Is.EqualTo(sett2));
			_mocks.VerifyAll();
		}
	}
}
