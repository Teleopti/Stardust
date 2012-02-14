using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class RestrictionEditorViewModel : IRestrictionAltered, INotifyPropertyChanged
    {

        private readonly ObservableCollection<IRestrictionViewModel> _restrictionModels = new ObservableCollection<IRestrictionViewModel>();
        private readonly CommandModel _addPreferenceRestriction;
        private readonly CommandModel _addStudentAvailability;
        private readonly CommandModel _updateAllCommandModel;
        private readonly ICommand _changedCommandBehavior;
        private IScheduleDay _schedulePart;

        private RestrictionEditorViewModel(IScheduleDay part)
        {
            _addPreferenceRestriction = new AddPreferenceRestrictionCommandModel(this);
            _addStudentAvailability = new AddStudentAvailabilityCommandModel(this);
            _updateAllCommandModel = new UpdateAllRestrictionViewModelsCommandModel(this);
            _changedCommandBehavior = new ChangedCommandBehavior(this);
            Load(part);
        }

        public RestrictionEditorViewModel(IScheduleDay part,IRepositoryFactory repositoryFactory,IUnitOfWorkFactory unitOfWorkFactory) : this(part)
        {
            using(IUnitOfWork uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                LoadCollections(new CommonRootCollection<IActivity>(repositoryFactory.CreateActivityRepository(uow)),
                                new CommonRootCollection<IDayOffTemplate>(repositoryFactory.CreateDayOffRepository(uow)),
                                new CommonRootCollection<IShiftCategory>(repositoryFactory.CreateShiftCategoryRepository(uow)));

            }
        }

        public RestrictionEditorViewModel(IScheduleDay part, IList<IActivity> activities, IList<IDayOffTemplate> dayOffTemplates, IList<IShiftCategory> shiftCategories)
            : this(part)
        {
            LoadCollections(activities, dayOffTemplates, shiftCategories);
        }

        private void LoadCollections(IList<IActivity> activities, IList<IDayOffTemplate> dayOffTemplates, IList<IShiftCategory> shiftCategories)
        {
            Activities = activities;
            DayOffTemplates = dayOffTemplates;
            ShiftCategories = shiftCategories;
        }

        public ObservableCollection<IRestrictionViewModel> RestrictionModels
        {
            get { return _restrictionModels; }
        }

        public CommandModel AddPreferenceRestrictionCommand
        {
            get { return _addPreferenceRestriction; }
        }

        public CommandModel AddStudentAvailabilityCommand
        {
            get { return _addStudentAvailability; }
        }

        public CommandModel UpdateAllCommand
        {
            get { return _updateAllCommandModel; }
        }

        public ICommand ChangedCommand
        {
           get
           {
               RestrictionIsAltered = true;
               return _changedCommandBehavior;
           } 
        }
        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
            private set { _schedulePart = value; }
        }

        public bool RestrictionIsAltered
        {
            get;
            set;
        }

        public void RestrictionAltered()
        {
            if (RestrictionChanged != null) RestrictionChanged(this, EventArgs.Empty);
            RestrictionIsAltered = false;
        }

        public void RestrictionRemoved(IRestrictionViewModel restriction)
        {
            if (restriction.PersistableScheduleData != null &&
                SchedulePart!=null)
            {
                SchedulePart.Remove(restriction.PersistableScheduleData);
                RestrictionModels.Remove(restriction);
            }
        }

        public event EventHandler RestrictionChanged;

        public virtual IList<IActivity> Activities { get; private set;}
        public virtual IList<IShiftCategory> ShiftCategories { get; private set;}
        public IList<IDayOffTemplate> DayOffTemplates { get; private set;}
      
        internal void ReadDataFromSchedulePart()
        {
            IEnumerable<IScheduleDataRestriction> scheduleDataRestrictions =
                _schedulePart.PersonRestrictionCollection().OfType<IScheduleDataRestriction>();

            var listToRemove = new List<IRestrictionViewModel>();
            foreach (var restrictionViewModel in RestrictionModels)
            {
                if(restrictionViewModel.BelongsToPart())
                    listToRemove.Add(restrictionViewModel);
            }
            foreach (var restrictionViewModel in listToRemove)
            {
                RestrictionModels.Remove(restrictionViewModel);
            }

            foreach (IScheduleDataRestriction scheduleDataRestriction in scheduleDataRestrictions)
            {
                IRestrictionViewModel model = RestrictionViewModel.CreateViewModel(this, scheduleDataRestriction);
                if (model != null)
                {
                    model.ScheduleDay = _schedulePart;
                    RestrictionModels.Add(model);
                }
            }
            IEnumerable<IPreferenceDay> preferenceRestrictions = _schedulePart.PersonRestrictionCollection().OfType<IPreferenceDay>();
            foreach (IPreferenceDay preferenceRestriction in preferenceRestrictions)
            {
                IRestrictionViewModel model = RestrictionViewModel.CreateViewModel(this, preferenceRestriction);
                if (model != null)
                {
                    model.ScheduleDay = _schedulePart;
                    RestrictionModels.Add(model);
                }
            }
            IEnumerable<IStudentAvailabilityDay> studDays = _schedulePart.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>();
            foreach (IStudentAvailabilityDay studDay in studDays)
            {
                IRestrictionViewModel model = RestrictionViewModel.CreateViewModel(this, studDay);
                if (model != null)
                {
                    model.ScheduleDay = _schedulePart;
                    RestrictionModels.Add(model);
                }
            }
            
        }

       internal void AddPreferenceDay(IPreferenceDay preferenceDay, IScheduleDay scheduleDay)
       {
           IRestrictionViewModel model = RestrictionViewModel.CreateViewModel(this, preferenceDay);
           if (model != null)
           {
               model.ScheduleDay = scheduleDay;
               RestrictionModels.Add(model);
           }
       }

        public void Load(IScheduleDay schedulePart)
        {
            SchedulePart = schedulePart;
            if (schedulePart != null)
            {
                //If the old SchedulePart is edited, but not Updated, it must be Updated:
                if (RestrictionIsAltered)
                {
                    UpdateAllModels();

                	var handler = RestrictionChanged;
                    if(handler!=null)
                    {
                    	handler(this,EventArgs.Empty);
                    }
                }
                SchedulePart = schedulePart;
                ReadDataFromSchedulePart();
            }
            else
            {
                RestrictionModels.Clear();
                RestrictionIsAltered = false;
            }
        }

        //Update all RestrictionViewModels from GUI
        private void UpdateAllModels()
        {
            RestrictionModels.ForEach(m=>m.CommitChanges());
            RestrictionIsAltered = false;
        }

        private class UpdateAllRestrictionViewModelsCommandModel : CommandModel
        {
            private readonly RestrictionEditorViewModel _model;

            public UpdateAllRestrictionViewModelsCommandModel(RestrictionEditorViewModel model)
            {
                _model = model;
            }

            public override string Text
            {
                get { return UserTexts.Resources.Update; }
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                _model.UpdateAllModels();       //Update all  RestrictionModels
                if (_model.SchedulePart != null)
                    _model.RestrictionAltered();    //Notify controller (SchedulingScreen)
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = true;
                if (_model != null)
                {
                    foreach (IRestrictionViewModel model in _model.RestrictionModels)
                    {
                        if (model.StartTimeLimits.Invalid || model.EndTimeLimits.Invalid || model.WorkTimeLimits.Invalid)
                        {
                            e.CanExecute = false;
                            return;
                        }
                    }
                }
            }
        }
       
        /// <summary>
        /// The definition of changed
        /// </summary>
        /// <remarks>
        /// Means that we have to update the scheduler 
        /// </remarks>
        /// 
        private class ChangedCommandBehavior : ICommand
        {
            //Wait... this needs refactoring
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
            private readonly RestrictionEditorViewModel _model;

            public ChangedCommandBehavior(RestrictionEditorViewModel model)
            {
                _model = model;
                if (CanExecuteChanged != null) CanExecuteChanged(this, EventArgs.Empty); //FxCop
            }

            public event EventHandler CanExecuteChanged; //Not used

            public void Execute(object parameter)
            {}

            public bool CanExecute(object parameter)
            {
                return true;
            }
        }

        public void AddStudentAvailabilityDay(IStudentAvailabilityDay studentAvailabilityDay, IScheduleDay scheduleDay)
        {
            IRestrictionViewModel model = RestrictionViewModel.CreateViewModel(this, studentAvailabilityDay);
            if (model != null)
            {
                model.ScheduleDay = scheduleDay;
                RestrictionModels.Add(model);
            }
        }

		//Just for fixing mem leaks in WPF
#pragma warning disable 0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
    }
}