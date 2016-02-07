﻿using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Logon
{
    public interface IRoleToPrincipalCommand
    {
		void Execute(ITeleoptiPrincipal principalToFillWithClaimSets, IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository);
    }
}