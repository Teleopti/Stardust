using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class WorkflowControlSetPresenter
	{
		private IWorkflowControlSetView _view;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private IList<IActivity> _activityCollection;
		private readonly IList<IWorkflowControlSetModel> _workflowControlSetModelCollection;
		private IList<IShiftCategory> _shiftCategories;
		private IList<IAbsence> _absences;
		private IList<IDayOffTemplate> _dayOffTemplates;
		private IList<ISkillType> _supportedSkillTypes;
		private readonly IToggleManager _toggleManager;
		private IWorkflowControlSetModel _selectedModel;
		private IList<IAbsence> _requestableAbsenceCollection;
		private IList<ISkill> _skillCollection;
		private bool _isUsingPrimarySkill;

		public WorkflowControlSetPresenter(IWorkflowControlSetView view,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRepositoryFactory repositoryFactory, IToggleManager toggleManager)
		{
			_view = view;
			_unitOfWorkFactory = unitOfWorkFactory;
			_repositoryFactory = repositoryFactory;
			_toggleManager = toggleManager;
			_workflowControlSetModelCollection = new List<IWorkflowControlSetModel>();

			var startDate = DateHelper.GetFirstDateInMonth(DateTime.Today, CultureInfo.CurrentCulture);
			var endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 3).AddDays(-1);
			ProjectionPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
		}

		public void SetParentView(IWorkflowControlSetView view)
		{
			_view = view;
		}

		public IEnumerable<IWorkflowControlSetModel> WorkflowControlSetModelCollection
		{
			get { return _workflowControlSetModelCollection.Where(w => !w.ToBeDeleted); }
		}

		public bool DoRequestableAbsencesExist
		{
			get
			{
				return _requestableAbsenceCollection != null && _requestableAbsenceCollection.Count != 0;
			}
		}

		public void SetSelectedWorkflowControlSetModel(IWorkflowControlSetModel value)
		{
			_selectedModel = value;

			_view.EnableAllAuthorized();
			_view.SetName(_selectedModel.Name);
			_view.SetUpdatedInfo(_selectedModel.UpdatedInfo);
			_view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
			_view.SetOvertimeOpenPeriodsGridRowCount(_selectedModel.OvertimeRequestPeriodModels.Count);
			_view.SetWriteProtectedDays(_selectedModel.WriteProtection);
			_view.LoadDateOnlyVisualizer();
			_view.SetMatchingSkills(_selectedModel);
			_view.SetAllowedDayOffs(_selectedModel);
			_view.SetAllowedShiftCategories(_selectedModel);
			_view.SetAllowedAbsences(_selectedModel);
			_view.SetAllowedAbsencesForReport(_selectedModel);
			_view.SetShiftTradeTargetTimeFlexibility(_selectedModel.ShiftTradeTargetTimeFlexibility);
			_view.SetAutoGrant(_selectedModel.AutoGrantShiftTradeRequest);
			_view.SetAnonymousTrading(_selectedModel.AnonymousTrading);
			_view.SetLockTrading(_selectedModel.LockTrading);
			_view.SetFairnessType(_selectedModel.GetFairnessType());
			_view.SetAbsenceRequestWaitlisting(_selectedModel.AbsenceRequestWaitlistingEnabled,
				_selectedModel.AbsenceRequestWaitlistingProcessOrder);
			_view.SetAbsenceRequestCancellation(_selectedModel);
			_view.SetAbsenceRequestExpiration(_selectedModel);
			_view.SetAbsenceProbability(_selectedModel.AbsenceProbabilityEnabled);
			_view.SetOvertimeProbability(_selectedModel.IsOvertimeProbabilityEnabled);
			_view.SetOvertimeRequestUsePrimarySkill(_selectedModel.OvertimeRequestUsePrimarySkill);
			_view.SetOverTimeRequestMaximumTimeHandleType(_selectedModel.OvertimeRequestMaximumOvertimeValidationHandleOptionView);
			_view.SetOverTimeRequestStaffingCheckMethod(_selectedModel.OvertimeRequestStaffingCheckMethodOptionView);
			_view.SetOverTimeRequestMaximumTime(_selectedModel.OvertimeRequestMaximumTime);
			_view.SetOvertimeRequestMaximumTimeEnabled(_selectedModel.OvertimeRequestMaximumTimeEnabled);

			_view.SetOverTimeRequestMaximumContinuousWorkTimeHandleType(_selectedModel.OvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView);
			_view.SetOverTimeRequestMaximumContinuousWorkTime(_selectedModel.OvertimeRequestMaximumContinuousWorkTime);
			_view.SetOverTimeRequestMinimumRestTimeThreshold(_selectedModel.OvertimeRequestMinimumRestTimeThreshold);
			_view.SetOvertimeRequestMaximumContinuousWorkTimeEnabled(_selectedModel.OvertimeRequestMaximumContinuousWorkTimeEnabled);
			_view.SetMaxConsecutiveWorkingDays(_selectedModel.MaxConsecutiveWorkingDays);
		}

		public IWorkflowControlSetModel SelectedModel
		{
			get { return _selectedModel; }
			set { _selectedModel = value; }
		}

		public IList<IAbsence> RequestableAbsenceCollection
		{
			get { return _requestableAbsenceCollection; }
		}

		public DateOnlyPeriod ProjectionPeriod { get; private set; }

		public IList<IActivity> ActivityCollection
		{
			get { return _activityCollection; }
		}

		public IList<ISkill> SkillCollection()
		{
			return _skillCollection;
		}

		public void Initialize()
		{
			IList<IWorkflowControlSet> workflowControlSetCollection;
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					var activityRepository = _repositoryFactory.CreateActivityRepository(uow);
					_activityCollection = activityRepository.LoadAllSortByName().Where(a => a.IsDeleted == false).ToList();
				}

				var repository = _repositoryFactory.CreateWorkflowControlSetRepository(uow);
				workflowControlSetCollection = repository.LoadAllSortByName();
				var absenceRepository = _repositoryFactory.CreateAbsenceRepository(uow);
				_requestableAbsenceCollection = absenceRepository.LoadRequestableAbsence();

				var shiftCategoryRepository = _repositoryFactory.CreateShiftCategoryRepository(uow);
				_shiftCategories = shiftCategoryRepository.LoadAll().ToList();

				_absences = absenceRepository.LoadAll().ToList();

				var dayOffRepository = _repositoryFactory.CreateDayOffRepository(uow);
				_dayOffTemplates = dayOffRepository.LoadAll().ToList();

				var skillRepository = _repositoryFactory.CreateSkillRepository(uow);
				_skillCollection = new List<ISkill>(skillRepository.FindAllWithoutMultisiteSkills());

				setIsUsingPrimarySkill(uow);

				initializeSupportedSkillTypes(uow);
			}
			_view.EnableHandlingOfAbsenceRequestPeriods(DoRequestableAbsencesExist);
			_view.InitializeView();
			_workflowControlSetModelCollection.Clear();
			workflowControlSetCollection.ForEach(
				w => _workflowControlSetModelCollection.Add(new WorkflowControlSetModel(w)));

			FillAllowedPreferenceActivityCombo();
			refreshListOrAddNewIfEmpty();
			SetCulture();
		}

		public IList<IDayOffTemplate> DayOffCollection()
		{
			return _dayOffTemplates;
		}

		public IList<IShiftCategory> ShiftCategoriesCollection()
		{
			return _shiftCategories;
		}

		public IList<IAbsence> AbsencesCollection()
		{
			return _absences;
		}

		public IList<ISkillType> SupportedSkillTypes()
		{
			return _supportedSkillTypes;
		}

		private void refreshListOrAddNewIfEmpty()
		{
			if (WorkflowControlSetModelCollection.IsEmpty())
			{
				_view.DisableAllButAdd();
			}
			else
			{
				RefreshWorkflowControlSetCombo();
			}
		}

		public void RefreshWorkflowControlSetCombo()
		{
			_view.FillWorkloadControlSetCombo(WorkflowControlSetModelCollection, "Name");
		}

		public void FillAllowedPreferenceActivityCombo()
		{
			ActivityCollection.Insert(0, null);
			_view.FillAllowedPreferenceActivityCombo(ActivityCollection, "Name");
		}

		public void AddWorkflowControlSet()
		{
			IWorkflowControlSet newDomainEntity = new WorkflowControlSet(Resources.NewWorkflowControlSet);
			newDomainEntity.SetFairnessType(FairnessType.EqualNumberOfShiftCategory);
			newDomainEntity.AbsenceRequestExpiredThreshold = 15;
			var newModel = new WorkflowControlSetModel(newDomainEntity);
			_workflowControlSetModelCollection.Add(newModel);
			RefreshWorkflowControlSetCombo();
			_selectedModel = newModel;
			_view.SelectWorkflowControlSet(newModel);
		}

		public void DefaultPreferencePeriods(IWorkflowControlSetModel model, DateTime today)
		{
			var culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;
			var startDate = new DateOnly(today.AddMonths(1).Year, today.AddMonths(1).Month, 1);
			var endDate = new DateOnly(today.AddMonths(1).Year, today.AddMonths(1).Month,
				DateHelper.GetLastDateInMonth(today.AddMonths(1), culture).Day);
			var insertPeriod = new DateOnlyPeriod(startDate, endDate);

			startDate = new DateOnly(today.AddMonths(2).Year, today.AddMonths(2).Month, 1);
			endDate = new DateOnly(today.AddMonths(2).Year, today.AddMonths(2).Month,
				DateHelper.GetLastDateInMonth(today.AddMonths(2), culture).Day);
			var preferencePeriod = new DateOnlyPeriod(startDate, endDate);
			model.PreferencePeriod = preferencePeriod;
			model.PreferenceInputPeriod = insertPeriod;
			_view.SetPreferencePeriods(insertPeriod, preferencePeriod);
		}

		public void DefaultStudentAvailabilityPeriods(IWorkflowControlSetModel model, DateTime today)
		{
			var culture = TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture;
			var startDate = new DateOnly(today.AddMonths(1).Year, today.AddMonths(1).Month, 1);
			var endDate = new DateOnly(today.AddMonths(1).Year, today.AddMonths(1).Month,
				DateHelper.GetLastDateInMonth(today.AddMonths(1), culture).Day);
			var insertPeriod = new DateOnlyPeriod(startDate, endDate);

			startDate = new DateOnly(today.AddMonths(2).Year, today.AddMonths(2).Month, 1);
			endDate = new DateOnly(today.AddMonths(2).Year, today.AddMonths(2).Month,
				DateHelper.GetLastDateInMonth(today.AddMonths(2), culture).Day);
			var studentAvailabilityPeriod = new DateOnlyPeriod(startDate, endDate);
			model.StudentAvailabilityPeriod = studentAvailabilityPeriod;
			model.StudentAvailabilityInputPeriod = insertPeriod;
			_view.SetStudentAvailabilityPeriods(insertPeriod, studentAvailabilityPeriod);
		}

		public void DeleteWorkflowControlSet()
		{
			if (SelectedModel == null) return;
			SelectedModel.ToBeDeleted = true;
			refreshListOrAddNewIfEmpty();
		}

		public void SaveChanges()
		{
			var toBeRemovedFromList = new List<IWorkflowControlSetModel>();
			var toBeUpdatedAfterPersist = new List<IWorkflowControlSetModel>();
			using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var repository = _repositoryFactory.CreateWorkflowControlSetRepository(uow);
				foreach (var workflowControlSetModel in _workflowControlSetModelCollection)
				{
					if (workflowControlSetModel.IsNew && workflowControlSetModel.ToBeDeleted)
					{
						toBeRemovedFromList.Add(workflowControlSetModel);
						continue;
					}
					if (workflowControlSetModel.ToBeDeleted)
					{
						toBeRemovedFromList.Add(workflowControlSetModel);
						repository.Remove(workflowControlSetModel.OriginalDomainEntity);
						continue;
					}
					if (workflowControlSetModel.IsNew)
					{
						repository.Add(workflowControlSetModel.DomainEntity);
						workflowControlSetModel.UpdateAfterMerge(workflowControlSetModel.DomainEntity);
						continue;
					}
					if (workflowControlSetModel.IsDirty)
					{
						uow.Reassociate(workflowControlSetModel.OriginalDomainEntity);
						var updatedWorkflowControlSet = uow.Merge(workflowControlSetModel.DomainEntity);
						LazyLoadingManager.Initialize(updatedWorkflowControlSet.AllowedPreferenceActivity);
						LazyLoadingManager.Initialize(updatedWorkflowControlSet.UpdatedBy);

						foreach (var absenceRequestOpenPeriod in updatedWorkflowControlSet.AbsenceRequestOpenPeriods)
						{
							LazyLoadingManager.Initialize(absenceRequestOpenPeriod.Absence);
						}

						updatedWorkflowControlSet.OvertimeRequestOpenPeriods.ForEach(p =>
							p.PeriodSkillTypes.ForEach(LazyLoadingManager.Initialize));
						workflowControlSetModel.UpdateAfterMerge(updatedWorkflowControlSet);
						toBeUpdatedAfterPersist.Add(workflowControlSetModel);
					}
				}
				uow.PersistAll();
			}
			foreach (var workflowControlSetModel in toBeRemovedFromList)
			{
				_workflowControlSetModelCollection.Remove(workflowControlSetModel);
			}
			foreach (var workflowControlSetModel in toBeUpdatedAfterPersist)
			{
				workflowControlSetModel.UpdateAfterMerge(workflowControlSetModel.OriginalDomainEntity);
			}
		}

		public void SetPeriodType(AbsenceRequestPeriodModel absenceRequestPeriodModel, AbsenceRequestPeriodTypeModel periodTypeModel)
		{
			int currentIndex =
				_selectedModel.DomainEntity.AbsenceRequestOpenPeriods.IndexOf(absenceRequestPeriodModel.DomainEntity);
			_selectedModel.DomainEntity.RemoveOpenAbsenceRequestPeriod(absenceRequestPeriodModel.DomainEntity);
			var absence = absenceRequestPeriodModel.DomainEntity.Absence;

			var newAbsenceRequestOpenPeriod = periodTypeModel.Item;
			newAbsenceRequestOpenPeriod.Absence = absence;
			_selectedModel.DomainEntity.InsertPeriod(newAbsenceRequestOpenPeriod, currentIndex);
			absenceRequestPeriodModel.SetDomainEntity(newAbsenceRequestOpenPeriod);

			_selectedModel.IsDirty = true;
		}

		public void SetOvertimeRequestPeriodType(OvertimeRequestPeriodModel overtimeRequestPeriodModel, OvertimeRequestPeriodTypeModel periodTypeModel)
		{
			int currentIndex =
				_selectedModel.DomainEntity.OvertimeRequestOpenPeriods.IndexOf(overtimeRequestPeriodModel.DomainEntity);
			_selectedModel.DomainEntity.RemoveOpenOvertimeRequestPeriod(overtimeRequestPeriodModel.DomainEntity);

			var newOvertimeRequestOpenPeriod = periodTypeModel.Item;
			resetOvertimeRequestDefaultPeriod(newOvertimeRequestOpenPeriod);

			_selectedModel.DomainEntity.InsertOvertimePeriod(newOvertimeRequestOpenPeriod, currentIndex);
			overtimeRequestPeriodModel.SetDomainEntity(newOvertimeRequestOpenPeriod);

			_selectedModel.IsDirty = true;
		}

		public void AddOpenDatePeriod()
		{
			addNewOpenPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);
		}

		public void AddOvertimeRequestOpenDatePeriod()
		{
			addNewOvertimeRequestOpenPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[0].Item);
		}

		public void AddOpenRollingPeriod()
		{
			addNewOpenPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item);
		}

		public void AddOvertimeRequestOpenRollingPeriod()
		{
			addNewOvertimeRequestOpenPeriod(WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters[1].Item);
		}

		private void addNewOpenPeriod(IAbsenceRequestOpenPeriod absenceRequestOpenPeriod)
		{
			absenceRequestOpenPeriod.Absence = RequestableAbsenceCollection[0];
			if (absenceRequestOpenPeriod.Absence.Tracker != null)
				absenceRequestOpenPeriod.PersonAccountValidator = new PersonAccountBalanceValidator();
			else
				absenceRequestOpenPeriod.PersonAccountValidator = new AbsenceRequestNoneValidator();

			_selectedModel.DomainEntity.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);
			_selectedModel.IsDirty = true;

			_view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
			_view.RefreshOpenPeriodsGrid();
		}

		private void addNewOvertimeRequestOpenPeriod(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			resetOvertimeRequestDefaultPeriod(overtimeRequestOpenPeriod);
			_selectedModel.DomainEntity.AddOpenOvertimeRequestPeriod(overtimeRequestOpenPeriod);
			_selectedModel.IsDirty = true;

			_view.SetOvertimeOpenPeriodsGridRowCount(_selectedModel.OvertimeRequestPeriodModels.Count);
			_view.RefreshOvertimeOpenPeriodsGrid();
		}

		private void resetOvertimeRequestDefaultPeriod(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			var availableDays = StaffingInfoAvailableDaysProvider.GetDays(_toggleManager);
			if (overtimeRequestOpenPeriod is OvertimeRequestOpenDatePeriod)
			{
				((OvertimeRequestOpenDatePeriod)overtimeRequestOpenPeriod).Period =
					new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(availableDays));
			}
			else
			{
				((OvertimeRequestOpenRollingPeriod)overtimeRequestOpenPeriod).BetweenDays = new MinMax<int>(0, availableDays);
			}
		}

		public void DeleteAbsenceRequestPeriod(IList<AbsenceRequestPeriodModel> absenceRequestPeriodModels)
		{
			if (absenceRequestPeriodModels == null || absenceRequestPeriodModels.Count == 0)
				return;

			if (!_view.ConfirmDeleteOfRequestPeriod())
				return;

			deleteSelectedAbsenceRequestPeriods(absenceRequestPeriodModels);
		}

		public void DeleteOvertimeRequestPeriod(IList<OvertimeRequestPeriodModel> overtimeRequestPeriodModels)
		{
			if (overtimeRequestPeriodModels == null || overtimeRequestPeriodModels.Count == 0)
				return;

			if (!_view.ConfirmDeleteOfRequestPeriod())
				return;

			deleteSelectedOvertimeRequestPeriods(overtimeRequestPeriodModels);
		}

		private void deleteSelectedAbsenceRequestPeriods(IEnumerable<AbsenceRequestPeriodModel> absenceRequestPeriodModels)
		{
			foreach (AbsenceRequestPeriodModel absenceRequestPeriodModel in absenceRequestPeriodModels)
			{
				_selectedModel.DomainEntity.RemoveOpenAbsenceRequestPeriod(absenceRequestPeriodModel.DomainEntity);
			}

			_selectedModel.IsDirty = true;
			_view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
			_view.RefreshOpenPeriodsGrid();
		}

		private void deleteSelectedOvertimeRequestPeriods(IEnumerable<OvertimeRequestPeriodModel> overtimeRequestPeriodModels)
		{
			foreach (var overtimeRequestPeriodModel in overtimeRequestPeriodModels)
			{
				_selectedModel.DomainEntity.RemoveOpenOvertimeRequestPeriod(overtimeRequestPeriodModel.DomainEntity);
			}

			_selectedModel.IsDirty = true;
			_view.SetOvertimeOpenPeriodsGridRowCount(_selectedModel.OvertimeRequestPeriodModels.Count);
			_view.RefreshOvertimeOpenPeriodsGrid();
		}

		public void NextProjectionPeriod()
		{
			var startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.StartDate.Date, 1);
			var endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.EndDate.Date, 1);
			ProjectionPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			_view.RefreshOpenPeriodsGrid();
			_view.RefreshOvertimeOpenPeriodsGrid();
		}

		public void PreviousProjectionPeriod()
		{
			var startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.StartDate.Date, -1);
			var endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.EndDate.Date, -1);
			ProjectionPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			_view.RefreshOpenPeriodsGrid();
			_view.RefreshOvertimeOpenPeriodsGrid();
		}

		public void MoveUp(AbsenceRequestPeriodModel absenceRequestPeriodModel)
		{
			if (SelectedModel != null)
			{
				SelectedModel.DomainEntity.MovePeriodUp(absenceRequestPeriodModel.DomainEntity);
				SelectedModel.IsDirty = true;
				_view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
				_view.RefreshOpenPeriodsGrid();
			}
		}

		public void MoveDown(AbsenceRequestPeriodModel absenceRequestPeriodModel)
		{
			if (SelectedModel != null)
			{
				SelectedModel.DomainEntity.MovePeriodDown(absenceRequestPeriodModel.DomainEntity);
				SelectedModel.IsDirty = true;
				_view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
				_view.RefreshOpenPeriodsGrid();
			}
		}

		public void MoveUp(OvertimeRequestPeriodModel overtimeRequestPeriodModel)
		{
			if (SelectedModel != null)
			{
				SelectedModel.DomainEntity.MovePeriodUp(overtimeRequestPeriodModel.DomainEntity);
				SelectedModel.IsDirty = true;
				_view.SetOvertimeOpenPeriodsGridRowCount(_selectedModel.OvertimeRequestPeriodModels.Count);
				_view.RefreshOvertimeOpenPeriodsGrid();
			}
		}

		public void MoveDown(OvertimeRequestPeriodModel overtimeRequestPeriodModel)
		{
			if (SelectedModel != null)
			{
				SelectedModel.DomainEntity.MovePeriodDown(overtimeRequestPeriodModel.DomainEntity);
				SelectedModel.IsDirty = true;
				_view.SetOvertimeOpenPeriodsGridRowCount(_selectedModel.OvertimeRequestPeriodModels.Count);
				_view.RefreshOvertimeOpenPeriodsGrid();
			}
		}

		public void SetSelectedAllowedPreferenceActivity(IActivity activity)
		{
			if (_selectedModel != null)
			{
				_selectedModel.AllowedPreferenceActivity = activity;
			}
		}

		public void SetWriteProtectedDays(int? integerValue)
		{
			if (_selectedModel != null)
			{
				_selectedModel.WriteProtection = integerValue;
			}
			_view.LoadDateOnlyVisualizer();
		}

		public void SetCulture()
		{
			_view.SetCalendarCultureInfo(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.Culture);
		}

		public void SetPublishedToDate(DateTime? dateTime)
		{
			if (_selectedModel != null)
			{
				if (dateTime.HasValue)
					_selectedModel.SchedulePublishedToDate = dateTime.Value.Date;
				else
					_selectedModel.SchedulePublishedToDate = null;
			}
			_view.LoadDateOnlyVisualizer();
		}

		public void SetPreferencePeriod(DateOnlyPeriod preferencePeriod)
		{
			_selectedModel.PreferencePeriod = preferencePeriod;
			_view.LoadDateOnlyVisualizer();
		}

		public void SetMaxConsecutiveWorkingDays(int maxConsecutiveWorkingDays)
		{
			if (_selectedModel != null)
			{
				_selectedModel.MaxConsecutiveWorkingDays = maxConsecutiveWorkingDays;
			}
		}

		public void SetPreferenceInputPeriod(DateOnlyPeriod preferenceInputPeriod)
		{
			_selectedModel.PreferenceInputPeriod = preferenceInputPeriod;
		}

		public void SetOpenShiftTradePeriod(MinMax<int> periodDays)
		{
			_selectedModel.ShiftTradeOpenPeriodDays = periodDays;
		}

		public void DefaultShiftTradePeriodDays(MinMax<int> periodDays)
		{
			_selectedModel.ShiftTradeOpenPeriodDays = periodDays;
			_view.SetShiftTradePeriodDays(periodDays);
		}

		public IList<DateOnlyPeriod> BasicVisualizerWriteProtectionPeriods(DateOnly today)
		{
			DateOnly start = DateOnly.MinValue;
			DateOnly end = DateOnly.MinValue;
			if (_selectedModel.WriteProtection.HasValue)
			{
				end = today.AddDays(-_selectedModel.WriteProtection.Value);
			}
			return new List<DateOnlyPeriod> { new DateOnlyPeriod(start, end) };
		}

		public IList<DateOnlyPeriod> BasicVisualizerPublishedPeriods()
		{
			var start = new DateOnly(DateHelper.MinSmallDateTime);
			var end = new DateOnly(DateHelper.MinSmallDateTime);
			if (_selectedModel.SchedulePublishedToDate.HasValue)
			{
				var tmp = _selectedModel.SchedulePublishedToDate.Value.Date;
				end = new DateOnly(tmp);
			}
			var ret = new List<DateOnlyPeriod> { new DateOnlyPeriod(start, end) };

			ret.AddRange(BasicVisualizerPreferencePeriods());
			return ret;
		}

		public IList<DateOnlyPeriod> BasicVisualizerPreferencePeriods()
		{
			return new List<DateOnlyPeriod> { _selectedModel.PreferencePeriod };
		}

		public IList<DateOnlyPeriod> BasicVisualizerStudentAvailabilityPeriods()
		{
			return new List<DateOnlyPeriod> { _selectedModel.StudentAvailabilityPeriod };
		}

		public virtual void AddAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			_selectedModel.AddAllowedPreferenceDayOff(dayOff);
		}

		public virtual void RemoveAllowedPreferenceDayOff(IDayOffTemplate dayOff)
		{
			_selectedModel.RemoveAllowedPreferenceDayOff(dayOff);
		}

		public virtual void AddAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			_selectedModel.AddAllowedPreferenceShiftCategory(shiftCategory);
		}

		public virtual void RemoveAllowedPreferenceShiftCategory(IShiftCategory shiftCategory)
		{
			_selectedModel.RemoveAllowedPreferenceShiftCategory(shiftCategory);
		}

		public virtual void AddAllowedPreferenceAbsence(IAbsence absence)
		{
			_selectedModel.AddAllowedPreferenceAbsence(absence);
		}

		public virtual void RemoveAllowedPreferenceAbsence(IAbsence absence)
		{
			_selectedModel.RemoveAllowedPreferenceAbsence(absence);
		}

		public virtual void AddAllowedAbsenceForReport(IAbsence absence)
		{
			_selectedModel.AddAllowedAbsenceForReport(absence);
		}

		public virtual void RemoveAllowedAbsenceForReport(IAbsence absence)
		{
			_selectedModel.RemoveAllowedAbsenceForReport(absence);
		}

		public void SetShiftTradeTargetTimeFlexibility(TimeSpan flexibility)
		{
			_selectedModel.ShiftTradeTargetTimeFlexibility = flexibility;
		}

		public void SetAutoGrant(bool autoGrant)
		{
			_selectedModel.AutoGrantShiftTradeRequest = autoGrant;
		}

		public void AddSkillToMatchList(ISkill skill)
		{
			_selectedModel.AddSkillToMatchList(skill);
		}

		public void RemoveSkillFromMatchList(ISkill skill)
		{
			_selectedModel.RemoveSkillFromMatchList(skill);
		}

		public void OnRadioButtonAdvFairnessEqualCheckChanged(bool value)
		{
			if (value)
				_selectedModel.SetFairnessType(FairnessType.EqualNumberOfShiftCategory);
		}

		public void OnRadioButtonAdvSeniorityCheckedChanged(bool value)
		{
			if (value)
				_selectedModel.SetFairnessType(FairnessType.Seniority);
		}

		public void SetStudentAvailabilityPeriod(DateOnlyPeriod studentAvailabilityPeriod)
		{
			_selectedModel.StudentAvailabilityPeriod = studentAvailabilityPeriod;
			_view.LoadDateOnlyVisualizer();
		}

		public void SetStudentAvailabilityInputPeriod(DateOnlyPeriod studentAvailabilityInputPeriod)
		{
			_selectedModel.StudentAvailabilityInputPeriod = studentAvailabilityInputPeriod;
		}

		public void SetAnonymousTrading(bool anonymousTrading)
		{
			_selectedModel.AnonymousTrading = anonymousTrading;
		}

		public void SetLockTrading(bool lockTrading)
		{
			_selectedModel.LockTrading = lockTrading;
		}

		public void SetAbsenceRequestWaitlisting(bool enableAbsenceRequestWaitlisting,
			WaitlistProcessOrder waitlistProcessOrder)
		{
			_selectedModel.AbsenceRequestWaitlistingEnabled = enableAbsenceRequestWaitlisting;
			_selectedModel.AbsenceRequestWaitlistingProcessOrder = waitlistProcessOrder;
		}

		public void SetAbsenceRequestCancellationThreshold(int? absenceRequestCancellationThreshold)
		{
			_selectedModel.AbsenceRequestCancellationThreshold = absenceRequestCancellationThreshold;
		}

		public void SetAbsenceRequestExpiredThreshold(int? absenceRequestExpiredThreshold)
		{
			_selectedModel.AbsenceRequestExpiredThreshold = absenceRequestExpiredThreshold;
		}

		public void SetAbsenceProbability(bool absenceProbabilityEnabled)
		{
			_selectedModel.AbsenceProbabilityEnabled = absenceProbabilityEnabled;
		}
		
		public void SetOvertimeProbability(bool overtimeProbability)
		{
			_selectedModel.IsOvertimeProbabilityEnabled = overtimeProbability;
		}
		public void SetOvertimeRequestMaximumTime(TimeSpan overtimeRequestMaximumTime)
		{
			_selectedModel.OvertimeRequestMaximumTime = overtimeRequestMaximumTime;
		}

		public void SetOvertimeRequestMinimumRestTimeThreshold(TimeSpan overtimeRequestMinimumRestTimeThreshold)
		{
			_selectedModel.OvertimeRequestMinimumRestTimeThreshold = overtimeRequestMinimumRestTimeThreshold;
		}

		public void SetOvertimeRequestMaximumContinuousWorkTime(TimeSpan overtimeRequestMaximumContinuousWorkTime)
		{
			_selectedModel.OvertimeRequestMaximumContinuousWorkTime = overtimeRequestMaximumContinuousWorkTime;
		}

		public void SetOvertimeRequestMaximumTimeHandleType(
			OvertimeRequestValidationHandleOptionView overtimeRequestValidationHandleOption)
		{
			if (_selectedModel != null)
			{
				_selectedModel.OvertimeRequestMaximumOvertimeValidationHandleOptionView = overtimeRequestValidationHandleOption;
			}
		}

		public void SetOvertimeRequestMaximumContinuousWorkTimeHandleType(
			OvertimeRequestValidationHandleOptionView overtimeRequestValidationHandleOption)
		{
			if (_selectedModel != null)
			{
				_selectedModel.OvertimeRequestMaximumContinuousWorkTimeValidationHandleOptionView = overtimeRequestValidationHandleOption;
			}
		}

		public void SetOvertimeRequestMaximumEnabled(bool overtimeRequestMaximumTimeEnabled)
		{
			_selectedModel.OvertimeRequestMaximumTimeEnabled = overtimeRequestMaximumTimeEnabled;
		}
		public void SetOvertimeRequestMaximumContinuousWorkTimeEnabled(bool overtimeRequestMaximumContinuousWorkTimeEnabled)
		{
			_selectedModel.OvertimeRequestMaximumContinuousWorkTimeEnabled = overtimeRequestMaximumContinuousWorkTimeEnabled;
		}

		private void initializeSupportedSkillTypes(IUnitOfWork uow)
		{
			var skillTypeRepository = _repositoryFactory.CreateSkillTypeRepository(uow);
			var skillTypeNames = new[] { SkillTypeIdentifier.Phone, SkillTypeIdentifier.Chat, SkillTypeIdentifier.Email };
			_supportedSkillTypes = skillTypeRepository.LoadAll()
				.Where(a => skillTypeNames.Contains(a.Description.Name)).ToList();
		}

		public void SetOvertimeRequestIntradayStaffingCheckMethod(OvertimeRequestStaffingCheckMethodOptionView overtimeRequestStaffingCheckMethodOptionView)
		{
			if (_selectedModel != null)
			{
				_selectedModel.OvertimeRequestStaffingCheckMethodOptionView = overtimeRequestStaffingCheckMethodOptionView;
			}
		}

		public void SetOvertimeRequestUsePrimarySkill(bool overtimeRequestUsePrimarySkill)
		{
			_selectedModel.OvertimeRequestUsePrimarySkill = overtimeRequestUsePrimarySkill;
		}

		private void setIsUsingPrimarySkill(IUnitOfWork uow)
		{
			var skillRepository = _repositoryFactory.CreateSkillRepository(uow);
			_isUsingPrimarySkill = skillRepository.LoadAll().Any(a => a.IsCascading());
		}

		public bool IsUsingPrimarySkill()
		{
			return _isUsingPrimarySkill;
		}
	}
}