﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
    public interface IRoleToPrincipalCommand
    {
		void Execute(ITeleoptiPrincipal principalToFillWithClaimSets, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository);
    }
}