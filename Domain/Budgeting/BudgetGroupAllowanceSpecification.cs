﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Budgeting
{
	public class BudgetGroupAllowanceSpecification : PersonRequestSpecification<IAbsenceRequestAndSchedules>, IBudgetGroupAllowanceSpecification
	{
		private readonly IBudgetGroupAllowanceCalculator _budgetGroupAllowanceCalculator;

		public BudgetGroupAllowanceSpecification(IBudgetGroupAllowanceCalculator budgetGroupAllowanceCalculator)
		{
			_budgetGroupAllowanceCalculator = budgetGroupAllowanceCalculator;
		}

		public override IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestAndSchedules)
		{
			return _budgetGroupAllowanceCalculator.IsSatisfied(absenceRequestAndSchedules);
		}
		
	}

	public interface IBudgetGroupAllowanceCalculator
	{
		IValidatedRequest IsSatisfied(IAbsenceRequestAndSchedules absenceRequestAndSchedules);
	}
	
}