using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Permissions
{
    public class PermissionsExplorerHelper
    {
	    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IApplicationFunctionsToggleFilter _applicationFunctionsToggleFilter;
	    private readonly IApplicationRoleRepository _applicationRoleRepository;

	    public PermissionsExplorerHelper(IUnitOfWorkFactory unitOfWorkFactory,
			IApplicationFunctionsToggleFilter applicationFunctionsToggleFilter, IApplicationRoleRepository applicationRoleRepository)
	    {
		    _unitOfWorkFactory = unitOfWorkFactory;
		    _applicationFunctionsToggleFilter = applicationFunctionsToggleFilter;
		    _applicationRoleRepository = applicationRoleRepository;
	    }

	    public AllFunctions LoadAllToggledApplicationFunctions()
        {
                return _applicationFunctionsToggleFilter.FilteredFunctions();
        }

        public ICollection<IPersonInRole> LoadPeopleByApplicationRole(IApplicationRole selectedRole)
        {
        	using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
        	{
                var personRepository = new ApplicationRolePersonRepository(uow);
            	return personRepository.GetPersonsInRole(selectedRole.Id.GetValueOrDefault());
            }
        }

        public ICollection<IPersonInRole> LoadPeopleNotInApplicationRole(IApplicationRole selectedRole, ICollection<Guid> personsIds)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork())
            {
                var personRepository = new ApplicationRolePersonRepository(uow);
                return personRepository.GetPersonsNotInRole(selectedRole.Id.GetValueOrDefault(), personsIds);
            }
        }

        public ICollection<IApplicationRole> LoadAllApplicationRoles()
        {
                return _applicationRoleRepository.LoadAllApplicationRolesSortedByName();
        }
    }
}
