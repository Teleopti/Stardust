using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SaveAgentPortalSettingsCommandHandlerTest
    {
        private MockRepository _mock;
        private IPersonalSettingDataRepository _personalSettingDataRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private SaveAgentPortalSettingsCommandHandler _target;
        private SaveAgentPortalSettingsCommandDto _saveAgentPortalSettingsCommandDto;
        private AgentPortalSettings _agentPortalSettings;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personalSettingDataRepository = _mock.StrictMock<IPersonalSettingDataRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.StrictMock<ICurrentUnitOfWorkFactory>();
            _target = new SaveAgentPortalSettingsCommandHandler(_personalSettingDataRepository,_currentUnitOfWorkFactory);
            _saveAgentPortalSettingsCommandDto = new SaveAgentPortalSettingsCommandDto {Resolution = 15};
            _agentPortalSettings = new AgentPortalSettings();
        }

        [Test]
        public void AgentPortalSettingShouldSaveSuccessfully()
        {
            var untiOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using(_mock.Record())
            {
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(_personalSettingDataRepository.FindValueByKey("AgentPortalSettings",
                                                                          new AgentPortalSettings())).IgnoreArguments().
                    Return(_agentPortalSettings);
                Expect.Call(()=>_personalSettingDataRepository.PersistSettingValue(null)).IgnoreArguments().Return(null);
                Expect.Call(() => untiOfWork.PersistAll());
                Expect.Call(untiOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(_saveAgentPortalSettingsCommandDto);
            }
        }
    }
}
