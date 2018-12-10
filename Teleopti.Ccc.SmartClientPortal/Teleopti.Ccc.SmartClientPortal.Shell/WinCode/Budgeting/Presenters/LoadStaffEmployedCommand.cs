using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
	public interface ILoadStaffEmployedCommand : IExecutableCommand
	{
	}

	public class LoadStaffEmployedCommand : ILoadStaffEmployedCommand
	{
		private readonly IBudgetPeopleProvider _budgetPeopleProvider;
		private readonly BudgetGroupMainModel _mainModel;
		private readonly IBudgetDaySource _budgetDaySource;

		public LoadStaffEmployedCommand(IBudgetPeopleProvider budgetPeopleProvider, BudgetGroupMainModel mainModel, IBudgetDaySource budgetDaySource)
		{
			_budgetPeopleProvider = budgetPeopleProvider;
			_mainModel = mainModel;
			_budgetDaySource = budgetDaySource;
		}

		public void Execute()
		{
			var firstBudgetDetailDayOfSelection = _budgetDaySource.Find().FirstOrDefault();
			if (firstBudgetDetailDayOfSelection == null) return;

			var fulltimeEquivalentHoursPerDay = _budgetDaySource.GetFulltimeEquivalentHoursPerDay(firstBudgetDetailDayOfSelection.FulltimeEquivalentHours);
			if (isConsideredEmpty(fulltimeEquivalentHoursPerDay)) return;

			var firstDay = firstBudgetDetailDayOfSelection.BudgetDay.Day;
			var people = _budgetPeopleProvider.FindPeopleWithBudgetGroup(_mainModel.BudgetGroup, firstDay);
			
			var totalNumberOfWorktimeHours = getTotalNumberOfWorktimeHours(firstDay, people);

			firstBudgetDetailDayOfSelection.StaffEmployed = totalNumberOfWorktimeHours.TotalHours / fulltimeEquivalentHoursPerDay.Value;
		}

		private static bool isConsideredEmpty(double? fulltimeEquivalentHoursPerDay)
		{
			return !fulltimeEquivalentHoursPerDay.HasValue || fulltimeEquivalentHoursPerDay.Value==0;
		}

		private static TimeSpan getTotalNumberOfWorktimeHours(DateOnly firstDay, IEnumerable<IPerson> people)
		{
			var totalNumberOfWorktimeHours = TimeSpan.Zero;
			foreach (IPerson person in people)
			{
				var personPeriod = person.Period(firstDay);
				totalNumberOfWorktimeHours = totalNumberOfWorktimeHours.Add(personPeriod.PersonContract.AverageWorkTimePerDay);
			}
			return totalNumberOfWorktimeHours;
		}
	}
}