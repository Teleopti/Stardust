using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SaveAgentPortalSettingsCommandHandlerTest
    {
        private MockRepository _mock;
        private IPersonalSettingDataRepository _personalSettingDataRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private SaveAgentPortalSettingsCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personalSettingDataRepository = _mock.StrictMock<IPersonalSettingDataRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _target = new SaveAgentPortalSettingsCommandHandler(_personalSettingDataRepository,_unitOfWorkFactory);
        }

        [Test]
        public void AgentPortalSettingShouldSaveSuccessfully()
        {
            var saveAgentPortalSettingsCommandDto = new SaveAgentPortalSettingsCommandDto();
            saveAgentPortalSettingsCommandDto.Resolution = 15;

            var untiOfWork = _mock.StrictMock<IUnitOfWork>();
            var agentPortalSettings = new AgentPortalSettings();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(untiOfWork);
                Expect.Call(_personalSettingDataRepository.FindValueByKey("AgentPortalSettings",
                                                                          new AgentPortalSettings())).IgnoreArguments().
                    Return(agentPortalSettings);
                Expect.Call(()=>_personalSettingDataRepository.PersistSettingValue(null)).IgnoreArguments().Return(null);
                Expect.Call(() => untiOfWork.PersistAll());
                Expect.Call(untiOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(saveAgentPortalSettingsCommandDto);
            }
        }
    }
}
