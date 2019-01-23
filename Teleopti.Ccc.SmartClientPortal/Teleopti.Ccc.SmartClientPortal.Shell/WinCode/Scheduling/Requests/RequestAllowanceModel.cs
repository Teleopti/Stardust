using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public interface IRequestAllowanceModel
	{
		string WeekName { get; set; }
		DateOnly SelectedDate { get; }
		DateOnlyPeriod VisibleWeek { get; }
		IScenario DefaultScenario { get; }
		bool FullAllowanceSelected { get; set; }
		bool ShrinkedAllowanceSelected { get; set; }
		IBudgetGroup SelectedBudgetGroup { get; set; }
		HashSet<IAbsence> AbsencesInBudgetGroup { get; }
		IList<IBudgetGroup> BudgetGroups { get; }
		IList<BudgetAbsenceAllowanceDetailModel> VisibleModel { get; }

		void Initialize(IBudgetGroup budgetGroup, DateOnly defaultDate);

		void ReloadModel(DateOnlyPeriod visibleWeek, bool reloadAllowance);

		void MoveToPreviousWeek();

		void MoveToNextWeek();
	}

	public class RequestAllowanceModel : IRequestAllowanceModel
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IBudgetGroupRepository _budgetGroupRepository;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IRequestAllowanceProvider _requestAllowanceProvider;
		private DateOnly _selectedDate;

		public RequestAllowanceModel(IUnitOfWorkFactory unitOfWorkFactory,
									IBudgetDayRepository budgetDayRepository,
									IBudgetGroupRepository budgetGroupRepository,
									ICurrentScenario scenarioRepository,
									IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_budgetGroupRepository = budgetGroupRepository;
			_scenarioRepository = scenarioRepository;
			_requestAllowanceProvider = new RequestAllowanceProvider(scheduleProjectionReadOnlyPersister, _scenarioRepository, budgetDayRepository);
			VisibleModel = new List<BudgetAbsenceAllowanceDetailModel>();
			BudgetGroups = new List<IBudgetGroup>();
			AbsencesInBudgetGroup = new HashSet<IAbsence>();
		}

		public string WeekName { get; set; }

		public DateOnly SelectedDate { get { return _selectedDate; } }

		public DateOnlyPeriod VisibleWeek { get; private set; }

		public IScenario DefaultScenario { get; private set; }

		public bool FullAllowanceSelected { get; set; }

		public bool ShrinkedAllowanceSelected { get; set; }

		public IBudgetGroup SelectedBudgetGroup { get; set; }

		public HashSet<IAbsence> AbsencesInBudgetGroup { get; private set; }

		public IList<IBudgetGroup> BudgetGroups { get; private set; }

		public IList<BudgetAbsenceAllowanceDetailModel> VisibleModel { get; private set; }

		public void Initialize(IBudgetGroup budgetGroup, DateOnly defaultDate)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_selectedDate = defaultDate;
				calculateVisibleWeek();
				loadDefaultScenario();
				loadBudgetGroups();
				initializeSelectedBudgetGroup(budgetGroup);
			}
		}

		private void calculateVisibleWeek()
		{
			VisibleWeek = DateHelper.GetWeekPeriod(SelectedDate, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture);
		}

		private void initializeSelectedBudgetGroup(IBudgetGroup budgetGroup)
		{
			if (budgetGroup != null)
			{
				SelectedBudgetGroup = budgetGroup;
				return;
			}
			SelectedBudgetGroup = BudgetGroups.FirstOrDefault() ?? new EmptyBudgetGroup();
		}

		public void ReloadModel(DateOnlyPeriod visibleWeek, bool reloadAllowance)
		{
			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				clearModels();
				WeekName = getWeekHeader(SelectedDate, VisibleWeek);
				loadAbsencesInBudgetGroup();
				var budgetAbsenceAllowanceDetails = _requestAllowanceProvider.GetBudgetAbsenceAllowanceDetails(visibleWeek, SelectedBudgetGroup,
					AbsencesInBudgetGroup);
				foreach (var budgetAbsenceAllowanceDetail in budgetAbsenceAllowanceDetails)
				{
					VisibleModel.Add(new BudgetAbsenceAllowanceDetailModel
					{
						AbsoluteDifference = budgetAbsenceAllowanceDetail.AbsoluteDifference,
						Allowance = ShrinkedAllowanceSelected ? budgetAbsenceAllowanceDetail.ShrinkedAllowance : budgetAbsenceAllowanceDetail.FullAllowance,
						Date = new DateDayModel(budgetAbsenceAllowanceDetail.Date),
						RelativeDifference = budgetAbsenceAllowanceDetail.RelativeDifference,
						TotalHeadCounts = budgetAbsenceAllowanceDetail.TotalHeadCounts,
						UsedTotalAbsences = budgetAbsenceAllowanceDetail.UsedTotalAbsences,
						UsedAbsencesDictionary =
							budgetAbsenceAllowanceDetail.UsedAbsencesDictionary.ToDictionary(
								item => Convert.ToString(item.Key.Id, CultureInfo.CurrentCulture), item => item.Value)
					});
				}
			}
		}

		private void clearModels()
		{
			VisibleModel.Clear();
			AbsencesInBudgetGroup.Clear();
		}

		private void loadBudgetGroups()
		{
			var list = _budgetGroupRepository.LoadAll();

			if (list != null)
			{
				var sorted = list.OrderBy(n2 => n2.Name);
				list = sorted.ToList();

				foreach (var budgetGroup in list)
				{
					BudgetGroups.Add(budgetGroup);
				}
			}
		}

		private void loadAbsencesInBudgetGroup()
		{
			if (SelectedBudgetGroup is EmptyBudgetGroup) return;
			if (!LazyLoadingManager.IsInitialized(SelectedBudgetGroup) || !LazyLoadingManager.IsInitialized(SelectedBudgetGroup.CustomShrinkages))
				_unitOfWorkFactory.CurrentUnitOfWork().Reassociate(SelectedBudgetGroup);
			foreach (var budgetAbsence in SelectedBudgetGroup.CustomShrinkages.SelectMany(customShrinkage => customShrinkage.BudgetAbsenceCollection))
			{
				AbsencesInBudgetGroup.Add(budgetAbsence);
			}
		}

		private void loadDefaultScenario()
		{
			DefaultScenario = _scenarioRepository.Current();
		}

		private static string getWeekHeader(DateOnly date, DateOnlyPeriod week)
		{
			var weekNumber = DateHelper.WeekNumber(date.Date, CultureInfo.CurrentCulture);
			return string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.WeekAbbreviationDot, weekNumber,
				week.StartDate.ToShortDateString());
		}

		public void MoveToPreviousWeek()
		{
			_selectedDate = _selectedDate.AddDays(-7);
			calculateVisibleWeek();
		}

		public void MoveToNextWeek()
		{
			_selectedDate = _selectedDate.AddDays(7);
			calculateVisibleWeek();
		}
	}

	public class EmptyBudgetGroup : BudgetGroup
	{
		public override string Name
		{
			get { return Name = UserTexts.Resources.Empty; }
		}
	}
}