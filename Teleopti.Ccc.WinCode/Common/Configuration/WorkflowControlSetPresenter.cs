﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
    public class WorkflowControlSetPresenter
    {
        private readonly IWorkflowControlSetView _view;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IRepositoryFactory _repositoryFactory;
	    private readonly IToggleManager _toggleManager;
	    private IList<IActivity> _activityCollection;
        private readonly IList<IWorkflowControlSetModel> _workflowControlSetModelCollection;
        private IList<IShiftCategory> _shiftCategories;
        private IList<IAbsence> _absences;
        private IList<IDayOffTemplate> _dayOffTemplates;

		public WorkflowControlSetPresenter(IWorkflowControlSetView view, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IToggleManager toggleManager)
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

        public IEnumerable<IWorkflowControlSetModel> WorkflowControlSetModelCollection
        {
            get
            {
                return _workflowControlSetModelCollection.Where(w => !w.ToBeDeleted);
            }
        }

        private IWorkflowControlSetModel _selectedModel;
        private IList<IAbsence> _requestableAbsenceCollection;
        private IList<ISkill> _skillCollection;

        public bool DoRequestableAbsencesExist
        {
            get
            {
                if (_requestableAbsenceCollection == null || _requestableAbsenceCollection.Count == 0)
                {
                    return false;
                }

                return true;
            }
        }

        public void SetSelectedWorkflowControlSetModel(IWorkflowControlSetModel value)
        {
            _selectedModel = value;

            _view.EnableAllAuthorized();
            _view.SetName(_selectedModel.Name);
            _view.SetUpdatedInfo(_selectedModel.UpdatedInfo);
            _view.SetOpenPeriodsGridRowCount(_selectedModel.AbsenceRequestPeriodModels.Count);
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
			_view.SetFairnessType(_selectedModel.GetFairnessType(_toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317), _toggleManager.IsEnabled(Toggles.Scheduler_Seniority_24331)));
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
                _shiftCategories = shiftCategoryRepository.LoadAll();

                _absences = absenceRepository.LoadAll();

                var dayOffRepository = _repositoryFactory.CreateDayOffRepository(uow);
                _dayOffTemplates = dayOffRepository.LoadAll();

                var skillRepository = _repositoryFactory.CreateSkillRepository(uow);
                _skillCollection = new List<ISkill>(skillRepository.FindAllWithoutMultisiteSkills());
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
			if (_toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317))
			{
				newDomainEntity.SetFairnessType(FairnessType.EqualNumberOfShiftCategory);
			}
            var newModel = new WorkflowControlSetModel(newDomainEntity);
            _workflowControlSetModelCollection.Add(newModel);
            RefreshWorkflowControlSetCombo();
            _selectedModel = newModel;
            _view.SelectWorkflowControlSet(newModel);
        }

        public void DefaultPreferencePeriods(IWorkflowControlSetModel model, DateTime today)
        {
            var culture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
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
            var culture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
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

        public void AddOpenDatePeriod()
        {
            addNewOpenPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);
        }

        public void AddOpenRollingPeriod()
        {
            addNewOpenPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item);
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

        public void DeleteAbsenceRequestPeriod(IList<AbsenceRequestPeriodModel> absenceRequestPeriodModels)
        {
            if (absenceRequestPeriodModels == null || absenceRequestPeriodModels.Count == 0)
                return;

            if (!_view.ConfirmDeleteOfAbsenceRequestPeriod())
                return;

            deleteSelectedAbsenceRequestPeriods(absenceRequestPeriodModels);
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

        public void NextProjectionPeriod()
        {
            var startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.StartDate.Date, 1);
            var endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.EndDate.Date, 1);
            ProjectionPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
            _view.RefreshOpenPeriodsGrid();
        }

        public void PreviousProjectionPeriod()
        {
            var startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.StartDate.Date, -1);
            var endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(ProjectionPeriod.EndDate.Date, -1);
            ProjectionPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
            _view.RefreshOpenPeriodsGrid();
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
            _view.SetCalendarCultureInfo(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture);
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
                DateTime tmp = _selectedModel.SchedulePublishedToDate.Value.Date;
                end = new DateOnly(tmp);
            }
            var ret = new List<DateOnlyPeriod> { new DateOnlyPeriod(start, end) };

            ret.AddRange(BasicVisualizerPreferencePeriods());
            //ret.AddRange(BasicVisualizerStudentAvailabilityPeriods());
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

        public void OnRadioButtonAdvFairnessPointsCheckChanged(bool value)
        {
			if(value)
				_selectedModel.SetFairnessType(FairnessType.FairnessPoints, _toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317), _toggleManager.IsEnabled(Toggles.Scheduler_Seniority_24331));
        }

        public void OnRadioButtonAdvFairnessEqualCheckChanged(bool value)
        {
			if(value)
				_selectedModel.SetFairnessType(FairnessType.EqualNumberOfShiftCategory, _toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317), _toggleManager.IsEnabled(Toggles.Scheduler_Seniority_24331));
        }

	    public void OnRadioButtonAdvSeniorityCheckedChanged(bool value)
	    {
			if (value)
				_selectedModel.SetFairnessType(FairnessType.Seniority, _toggleManager.IsEnabled(Toggles.Scheduler_HidePointsFairnessSystem_28317), _toggleManager.IsEnabled(Toggles.Scheduler_Seniority_24331));    
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
    }
}
