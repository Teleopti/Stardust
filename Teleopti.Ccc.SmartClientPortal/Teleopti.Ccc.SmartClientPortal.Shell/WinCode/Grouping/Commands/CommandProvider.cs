using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands
{
	public interface ICommandProvider:  IDisposable
	{
        ILoadOrganizationCommand GetLoadOrganizationCommand(IApplicationFunction applicationFunction, bool showPersons, bool loadUsers);
        ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction, Guid optionalColumnId);
        ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction);
    }

    public class CommandProvider : ICommandProvider
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
	    private IPersonSelectorView _view;
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

        public ILoadBuiltInTabsCommand GetLoadBuiltInTabsCommand(PersonSelectorField loadType, IPersonSelectorView personSelectorView, string rootNodeName, IApplicationFunction applicationFunction, Guid optionalColumnId)
        {
            return new LoadBuiltInTabsCommand(loadType, _unitOfWorkFactory, _personSelectorReadOnlyRepository, personSelectorView, rootNodeName, _commonAgentNameSettings.Value, applicationFunction, optionalColumnId);
        }

        public ILoadUserDefinedTabsCommand GetLoadUserDefinedTabsCommand(IPersonSelectorView personSelectorView, Guid value, IApplicationFunction applicationFunction)
        {
            return new LoadUserDefinedTabsCommand(_unitOfWorkFactory, _personSelectorReadOnlyRepository, personSelectorView, value, _commonAgentNameSettings.Value, applicationFunction);
        }

	    public void Dispose()
	    {
		    _view = null;
	    }
    }
}