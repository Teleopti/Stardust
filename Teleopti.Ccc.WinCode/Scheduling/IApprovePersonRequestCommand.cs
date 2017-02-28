﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IApprovePersonRequestCommand : IHandlePersonRequestCommand
	{
		IList<IBusinessRuleResponse> Approve(INewBusinessRuleCollection newBusinessRules);
	}
}