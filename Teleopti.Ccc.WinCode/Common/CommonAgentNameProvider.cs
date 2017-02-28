using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ICommonAgentNameProvider
    {
        ICommonNameDescriptionSetting CommonAgentNameSettings { get; }
    }

    public class CommonAgentNameProvider : ICommonAgentNameProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private CommonNameDescriptionSetting _commonNameDescription;

        public CommonAgentNameProvider(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
        }
        public ICommonNameDescriptionSetting CommonAgentNameSettings
        {
            get
            {
                if (_commonNameDescription == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        ISettingDataRepository settingDataRepository = _repositoryFactory.CreateGlobalSettingDataRepository(uow);

                        _commonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
                    }
                }
                return _commonNameDescription;
            }

        }
    }
}