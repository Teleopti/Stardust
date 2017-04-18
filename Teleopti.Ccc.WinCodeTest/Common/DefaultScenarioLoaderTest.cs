using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class DefaultScenarioLoaderTest
	{
		private DefaultScenarioLoader _defaultScenarioLoader;
		private IScenarioRepository _scenarioRepository;
		private MockRepository _mocks;
		private IScenario _scenario;

		[SetUp]
		public void Setup()
		{	
			_mocks = new MockRepository();
			_defaultScenarioLoader = new DefaultScenarioLoader();
			_scenarioRepository = _mocks.StrictMock<IScenarioRepository>();
			_scenario = _mocks.StrictMock<IScenario>();
		}

		[Test]
		public void ShouldLoad()
		{
			using(_mocks.Record())
			{
				Expect.Call(_defaultScenarioLoader.Load(_scenarioRepository)).Return(_scenario);
			}

			using(_mocks.Playback())
			{
				_defaultScenarioLoader.Load(_scenarioRepository);
			}
		}

        [Test]
        public void ShouldThrowIfRepositoryIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _defaultScenarioLoader.Load(null));
        }
	}
}
