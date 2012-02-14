using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class PreferenceRestrictionViewModel : RestrictionViewModel
    {
        private readonly IActivity _nullActivity = new Activity(" ");
        private readonly IShiftCategory _nullShiftCategory = new ShiftCategory(" ");
        private readonly IDayOffTemplate _nullDayOffTemplate = new DayOffTemplate(new Description(" "));
        private  CollectionViewSource _activitiesViewSource;
        private  CollectionViewSource _categoriesViewSource;
        private CollectionViewSource _dayOffTemplatesViewSource;
        private ObservableCollection<IActivity> _activities;
        private ObservableCollection<IShiftCategory> _categories;
        private ObservableCollection<IDayOffTemplate> _dayOffTemplates;
        private bool _hasDayOff;
        private bool _hasShiftCategory;

        public PreferenceRestrictionViewModel(IPreferenceRestriction preferenceRestriction, IRestrictionAltered parent)
        {
            //Allow null parent if we want to use the ViewModel in another place than editor, just to show the model
            if (parent!=null)
            {
                InitializeCollections(preferenceRestriction, parent);
                _dayOffTemplatesViewSource.View.CurrentChanged += delegate
                                                                      {
                                                                          HasDayOff =
                                                                              _dayOffTemplatesViewSource.View.
                                                                                  CurrentItem != _nullDayOffTemplate;
                                                                      };
                _categoriesViewSource.View.CurrentChanged += delegate
                                                                 {
                                                                     HasShiftCategory =
                                                                         _categoriesViewSource.View.CurrentItem !=
                                                                         _nullShiftCategory;
                                                                 };
                ParentToCommitChanges = parent;
            }
         
            Restriction = preferenceRestriction;
            StartTimeLimits = new LimitationViewModel(preferenceRestriction.StartTimeLimitation) {InvalidStatePossible = true};
            EndTimeLimits = new LimitationViewModel(preferenceRestriction.EndTimeLimitation) {InvalidStatePossible = true};
            WorkTimeLimits = new LimitationViewModel(preferenceRestriction.WorkTimeLimitation,true,true);
            HasDayOff = preferenceRestriction.DayOffTemplate != null;
            HasShiftCategory = preferenceRestriction.ShiftCategory != null;
        }

        public bool HasShiftCategory
        {
            get { return _hasShiftCategory; }
            set
            {
                if (_hasShiftCategory != value)
                {
                    _hasShiftCategory = value;
                    NotifyPropertyChanged("HasShiftCategory");
                }
            }
        }

        private void InitializeCollections(IPreferenceRestriction preferenceRestriction, IRestrictionAltered parent)
        {
            CreateCollections(parent);
            AddDeletedItems(preferenceRestriction);
            AddNullItems();
            CreateCollectionViewSources();

            var currentActivity = NullActivity;
            if (preferenceRestriction.ActivityRestrictionCollection.Count > 0)
                currentActivity = preferenceRestriction.ActivityRestrictionCollection[0].Activity;
            
            Activities.View.MoveCurrentTo(currentActivity ?? NullActivity);
            Categories.View.MoveCurrentTo(preferenceRestriction.ShiftCategory ?? NullShiftCategory);
            DayOffTemplates.View.MoveCurrentTo(preferenceRestriction.DayOffTemplate ?? NullDayOffTemplate);
        }

        private void AddDeletedItems(IPreferenceRestriction preferenceRestriction)
        {
            if (RestrictionContainsDeletedDayOffTemplate(preferenceRestriction))
            {
                _dayOffTemplates.Insert(0, preferenceRestriction.DayOffTemplate);
            }
            if (RestrictionContainsDeletedShiftCategory(preferenceRestriction))
            {
                _categories.Insert(0, preferenceRestriction.ShiftCategory);
            }
        }

        private static bool RestrictionContainsDeletedShiftCategory(IPreferenceRestriction preferenceRestriction)
        {
            return preferenceRestriction.ShiftCategory != null &&
                   ((IDeleteTag)preferenceRestriction.ShiftCategory).IsDeleted;
        }

        private static bool RestrictionContainsDeletedDayOffTemplate(IPreferenceRestriction preferenceRestriction)
        {
            return preferenceRestriction.DayOffTemplate!=null &&
                   ((IDeleteTag)preferenceRestriction.DayOffTemplate).IsDeleted;
        }

        private void CreateCollectionViewSources()
        {
            _activitiesViewSource = new CollectionViewSource { Source = _activities };
            _categoriesViewSource = new CollectionViewSource { Source = _categories };
            _dayOffTemplatesViewSource = new CollectionViewSource {Source = _dayOffTemplates};
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Collections.ObjectModel.ObservableCollection`1<Teleopti.Interfaces.Domain.IShiftCategory>.#.ctor(System.Collections.Generic.IEnumerable`1<Teleopti.Interfaces.Domain.IShiftCategory>)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Collections.ObjectModel.ObservableCollection`1<Teleopti.Interfaces.Domain.IDayOffTemplate>.#.ctor(System.Collections.Generic.IEnumerable`1<Teleopti.Interfaces.Domain.IDayOffTemplate>)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Collections.ObjectModel.ObservableCollection`1<Teleopti.Interfaces.Domain.IActivity>.#.ctor(System.Collections.Generic.IEnumerable`1<Teleopti.Interfaces.Domain.IActivity>)")]
        private void CreateCollections(IRestrictionAltered parent)
        {
            _activities = (parent.Activities!= null)
                              ? new ObservableCollection<IActivity>(parent.Activities)
                              : new ObservableCollection<IActivity>();

            _categories = (parent.ShiftCategories != null) 
                              ? new ObservableCollection<IShiftCategory>(parent.ShiftCategories) 
                              : new ObservableCollection<IShiftCategory>();
            
            _dayOffTemplates = (parent.DayOffTemplates!=null)
                                   ? new ObservableCollection<IDayOffTemplate>(parent.DayOffTemplates) 
                                   : new ObservableCollection<IDayOffTemplate>();
        }

        private void AddNullItems()
        {
            _activities.Insert(0, NullActivity);
            _categories.Insert(0, NullShiftCategory);
            _dayOffTemplates.Insert(0,NullDayOffTemplate);
        }

        public CollectionViewSource Activities
        {
            get { return _activitiesViewSource; }
        }

        public CollectionViewSource DayOffTemplates
        {
            get { return _dayOffTemplatesViewSource;}
        }

        public CollectionViewSource Categories
        {
            get { return _categoriesViewSource; }
        }

        private IActivity NullActivity
        {
            get { return _nullActivity; }
        }

        private IShiftCategory NullShiftCategory
        {
            get { return _nullShiftCategory; }
        }

        private IDayOffTemplate NullDayOffTemplate
        {
            get { return _nullDayOffTemplate; }
        }

        public override string Description
        {
            get { return UserTexts.Resources.Preference; }
        }

        public override void CommitChanges()
        {
            UpdateTimeProperties();
            if (ParentToCommitChanges!=null)
            {
                var preferenceRestriction = (IPreferenceRestriction) Restriction;

                var selectedActivityIactivity = (IActivity)Activities.View.CurrentItem;
                if (preferenceRestriction.ActivityRestrictionCollection.Count > 0)
                {
                    // jävla krångel nu men...
                    IActivityRestriction activityRestriction = preferenceRestriction.ActivityRestrictionCollection[0];
                    preferenceRestriction.RemoveActivityRestriction(activityRestriction);
                    activityRestriction.Activity = (selectedActivityIactivity == NullActivity)
                    ? null
                    : selectedActivityIactivity;

                    preferenceRestriction.AddActivityRestriction(activityRestriction);
                }
                
                
                var selectedIShiftCategory = (IShiftCategory)Categories.View.CurrentItem;
                preferenceRestriction.ShiftCategory = (selectedIShiftCategory.Equals(NullShiftCategory)) 
                    ? null 
                    : selectedIShiftCategory;

                var selectedDayOffTemplate = (IDayOffTemplate) DayOffTemplates.View.CurrentItem;
                preferenceRestriction.DayOffTemplate = (selectedDayOffTemplate.Equals(NullDayOffTemplate))
                     ? null
                     : selectedDayOffTemplate;
               
                if(ScheduleDay != null && !BelongsToPart() && Restriction.IsRestriction())
                    ScheduleDay.Add((IPreferenceDay)Restriction.Parent);

                if (ScheduleDay != null && BelongsToPart() && !Restriction.IsRestriction())
                    ScheduleDay.Remove((IPreferenceDay)Restriction.Parent);

                ParentToCommitChanges.RestrictionIsAltered = true;
            }
         
        }

        public string ShiftCategory
        {
            get
            {
                return ((IPreferenceRestriction)Restriction).ShiftCategory.Description.Name;
            }
        }

        public string DayOff
        {
            get
            {
               return ((IPreferenceRestriction)Restriction).DayOffTemplate.Description.Name;
               
            }
        }

        public string Activity
        {
            get
            {
                var prefRestriction = (IPreferenceRestriction) Restriction;
                if (prefRestriction.ActivityRestrictionCollection.Count > 0)
                {
                    return prefRestriction.ActivityRestrictionCollection[0].Activity.Description.Name;
                }
                return "";
            }
        }

        public bool HasDayOff
        {
            get
            {
                return _hasDayOff;
            }
            set
            {
                if (_hasDayOff!=value)
                {
                    _hasDayOff = value;
                    NotifyPropertyChanged("HasDayOff");
                }
            }
        }

        public override bool BelongsToPart()
        {
            if (ScheduleDay == null)
                return false;
            
            IEnumerable<IPreferenceDay> preferenceRestrictions = ScheduleDay.PersonRestrictionCollection().OfType<IPreferenceDay>();

            foreach (var preferenceRestriction in preferenceRestrictions)
            {
                if (preferenceRestriction.Restriction.Equals(Restriction))
                    return true;
            }

            return false;
        }
    }
}