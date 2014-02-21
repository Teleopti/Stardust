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
        ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction);
        ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction);
    }

    public class CommandProvider : ICommandProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
	    private readonly IPersonSelectorView _view;
	    private readonly Lazy<CommonNameDescriptionSetting> _commonAgentNameSettings;

        public CommandProvider(IUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository, IGlobalSettingDataRepository globalSettingDataRepository, IPersonSelectorView view)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
	        _view = view;
	        _commonAgentNameSettings = new Lazy<CommonNameDescriptionSetting>(() =>
		        {
			        using (unitOfWorkFactory.CreateAndOpenUnitOfWork())
			        {
				        return globalSettingDataRepository.FindValueByKey("CommonNameDescription",
				                                                          new CommonNameDescriptionSetting());
			        }
		        });
        }

        public ILoadOrganizationCommand GetLoadOrganizationCommand(IApplicationFunction applicationFunction, bool showPersons, bool loadUsers)
        {
            return new LoadOrganizationCommand(_unitOfWorkFactory, _personSelectorReadOnlyRepository, _view, _commonAgentNameSettings.Value, applicationFunction, showPersons, loadUsers);
        }

        public ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction)
        {
            return new LoadBuiltInTabsCommand(loadType, _unitOfWorkFactory, _personSelectorReadOnlyRepository, personSelectorView, rootNodeName, _commonAgentNameSettings.Value, applicationFunction);
        }

        public ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction)
        {
            return new LoadUserDefinedTabsCommand(_unitOfWorkFactory, _personSelectorReadOnlyRepository, personSelectorView, value, _commonAgentNameSettings.Value, applicationFunction);
        }
    }
}