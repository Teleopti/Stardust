﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory
{
	public class RequestAllowanceViewModelFactory : IRequestAllowanceViewModelFactory
	{
		private readonly IRequestAllowanceProvider _requestAllowanceProvider;
		private readonly IBudgetGroupRepository _budgetGroupRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public RequestAllowanceViewModelFactory(IRequestAllowanceProvider requestAllowanceProvider, IBudgetGroupRepository budgetGroupRepository, ILoggedOnUser loggedOnUser)
		{
			_requestAllowanceProvider = requestAllowanceProvider;
			_budgetGroupRepository = budgetGroupRepository;
			_loggedOnUser = loggedOnUser;
		}

		public IList<BudgetAbsenceAllowanceDetailViewModel> CreateBudgetAbsenceAllowanceDetailViewModels(
			DateOnly date, Guid? budgetGroupId)
		{
			var period = DateHelper.GetWeekPeriod(date, _loggedOnUser.CurrentUser().PermissionInformation.Culture());
			var selectedBudgetGroup = getSelectedBudgetGroup(budgetGroupId);
			var absencesInBudgetGroup = getAbsencesInBudgetGroup(selectedBudgetGroup);
			var budgetAbsenceAllowanceDetails = _requestAllowanceProvider.GetBudgetAbsenceAllowanceDetails(period,
				selectedBudgetGroup, absencesInBudgetGroup);
			return budgetAbsenceAllowanceDetails.Select(budgetAbsenceAllowanceDetail => new BudgetAbsenceAllowanceDetailViewModel
			{
				AbsoluteDifference = budgetAbsenceAllowanceDetail.AbsoluteDifference,
				Allowance = budgetAbsenceAllowanceDetail.Allowance,
				TotalAllowance = budgetAbsenceAllowanceDetail.TotalAllowance,
				Date = budgetAbsenceAllowanceDetail.Date,
				RelativeDifference =
					double.IsNaN(budgetAbsenceAllowanceDetail.RelativeDifference.Value)
						? new double?()
						: budgetAbsenceAllowanceDetail.RelativeDifference.Value,
				TotalHeadCounts = budgetAbsenceAllowanceDetail.TotalHeadCounts,
				UsedTotalAbsences = budgetAbsenceAllowanceDetail.UsedTotalAbsences,
				UsedAbsencesDictionary =
					budgetAbsenceAllowanceDetail.UsedAbsencesDictionary.ToDictionary(
						item => item.Key.Name, item => item.Value)
			}).ToList();
		}

		public IList<BudgetGroupViewModel> CreateBudgetGroupViewModels()
		{
			var list = getBudgetGroupList();
			return list.Select(b => new BudgetGroupViewModel
			{
				Id = b.Id.GetValueOrDefault(),
				Name = b.Name
			}).ToList();
		}

		private IBudgetGroup getSelectedBudgetGroup(Guid? budgetGroupId)
		{
			if (budgetGroupId.HasValue)
			{
				return _budgetGroupRepository.Get(budgetGroupId.Value);
			}
			return getBudgetGroupList().FirstOrDefault();
		}

		private IList<IBudgetGroup> getBudgetGroupList()
		{
			var list = _budgetGroupRepository.LoadAll();
			if (list != null)
			{
				return list.OrderBy(b => b.Name).ToList();
			}
			return new List<IBudgetGroup>();
		}

		private IEnumerable<IAbsence> getAbsencesInBudgetGroup(IBudgetGroup selectedBudgetGroup)
		{
			if (selectedBudgetGroup == null) return new IAbsence[] {};
			var absencesInBudgetGroup = new HashSet<IAbsence>();
			foreach (
				var budgetAbsence in
					selectedBudgetGroup.CustomShrinkages.SelectMany(customShrinkage => customShrinkage.BudgetAbsenceCollection))
			{
				absencesInBudgetGroup.Add(budgetAbsence);
			}
			return absencesInBudgetGroup;
		}
	}
}