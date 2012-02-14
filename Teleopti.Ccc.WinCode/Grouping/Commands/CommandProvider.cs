using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
{
    public interface ICommandProvider
    {
        ILoadOrganizationCommand GetLoadOrganizationCommand(IApplicationFunction applicationFunction, bool showPersons, bool loadUsers);
        ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction, bool showPersons);
        ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction, bool showPersons);
    }

    public class CommandProvider : ICommandProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPersonSelectorView _view;
        private CommonNameDescriptionSetting _commonNameDescription;

        public CommandProvider(IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, 
            IPersonSelectorView view )
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _repositoryFactory = repositoryFactory;
            _view = view;
        }

        public ILoadOrganizationCommand GetLoadOrganizationCommand(IApplicationFunction applicationFunction, bool showPersons, bool loadUsers)
        {
            return new LoadOrganizationCommand(_unitOfWorkFactory, _repositoryFactory, _view, CommonAgentNameSettings, applicationFunction, showPersons, loadUsers);
        }

        public ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction, bool showPersons)
        {
            return new LoadBuiltInTabsCommand(loadType, _unitOfWorkFactory, _repositoryFactory, personSelectorView, rootNodeName, CommonAgentNameSettings, applicationFunction, showPersons);
        }

        public ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction, bool showPersons)
        {
            return new LoadUserDefinedTabsCommand(_unitOfWorkFactory, _repositoryFactory, personSelectorView, value, CommonAgentNameSettings, applicationFunction, showPersons);
        }

        private CommonNameDescriptionSetting CommonAgentNameSettings
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