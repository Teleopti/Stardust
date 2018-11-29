using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class ActivityPresenter : BasePresenter<IActivityViewModel>, IActivityPresenter
    {
        public ActivityPresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
            : base(explorer,dataHelper)
        {
        }

        public override void LoadModelCollection()
        {
            if (ModelCollection!=null)
            {
                foreach (var model in ModelCollection)
                {
                    model.ActivityTypeChanged -= activityTypeChanged;
                }
            }
            ClearModelCollection();
            foreach (var ruleSet in Explorer.Model.FilteredRuleSetCollection)
            {
                foreach (var extender in ruleSet.ExtenderCollection)
                {
                    IActivityViewModel model;
	                var activityExtender = extender as AutoPositionedActivityExtender;
	                if (activityExtender != null)
                        model = new AutoPositionViewModel(ruleSet, activityExtender);
                    else
                        model = new AbsolutePositionViewModel(ruleSet, (ActivityNormalExtender)extender);

                    model.ActivityTypeChanged += activityTypeChanged;
                    AddToModelCollection(model);
                }
            }
        }

        public void AddAbsolutePositionActivity()
        {
	        if (Explorer.Model.FilteredRuleSetCollection.Count <= 0) return;

	        var activity = Explorer.Model.ActivityCollection[0];
	        var activityLengthWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, Explorer.Model.DefaultSegment);
	        var activityPositionWithSegment = new TimePeriodWithSegment(8, 0, 9, 0, Explorer.Model.DefaultSegment);
	        foreach (var ruleSet in Explorer.Model.FilteredRuleSetCollection)
	        {
		        var extender = new ActivityAbsoluteStartExtender(activity, activityLengthWithSegment, activityPositionWithSegment);
		        ActivityViewModel<ActivityNormalExtender> model = new AbsolutePositionViewModel(ruleSet, extender);
		        model.ActivityTypeChanged += activityTypeChanged;
                    
		        ruleSet.AddExtender(extender);
		        AddToModelCollection(model);
	        }
        }

        public void ChangeExtenderType(IActivityViewModel<ActivityNormalExtender> model, 
                                       ActivityNormalExtender anExtender)
        {
            var newAdapter = new AbsolutePositionViewModel(model.WorkShiftRuleSet, anExtender);
            newAdapter.ActivityTypeChanged -= activityTypeChanged;
            newAdapter.ActivityTypeChanged += activityTypeChanged;
            ReplaceModel(model, newAdapter);

            foreach (var ruleSet in Explorer.Model.FilteredRuleSetCollection)
            {
	            var indexOfExtender = ruleSet.ExtenderCollection.IndexOf(model.ContainedEntity);
	            if (indexOfExtender == -1) continue;

	            ruleSet.DeleteExtender(model.ContainedEntity);
	            ruleSet.InsertExtender(indexOfExtender, anExtender);
	            break;
            }
        }

        public void DeleteActivities(ReadOnlyCollection<int> selected)
        {
            if (selected.Count > 0)
            {
                if (ModelCollection.Count > 0)
                {
                    IList<IActivityViewModel> toBeDeleted = selected.Select(index => ModelCollection[index - 1]).ToList();
	                foreach (var item in toBeDeleted)
                    {
                        RemoveFromCollection(item);
	                    var absolutePositionViewModel = item as AbsolutePositionViewModel;
	                    if (absolutePositionViewModel != null)
                        {
                            var absView = absolutePositionViewModel;
                            absView.WorkShiftRuleSet.DeleteExtender(absView.ContainedEntity);
                        }
                        else
                        {
                            var autoView = (AutoPositionViewModel)item;
                            autoView.WorkShiftRuleSet.DeleteExtender(autoView.ContainedEntity);
                        }
                    }
                }
            }
        }

        public void ReOrderActivities(ReadOnlyCollection<int> adapterIndexList, MoveType moveType)
        {
            foreach (var adapterIndex in adapterIndexList)
            {
				var newIndex = (adapterIndex - 1);
				var leftIndex = (moveType == MoveType.MoveUp) ? (newIndex - 1) : (newIndex + 1);
				var leftView = ModelCollection[leftIndex];
				var rightView = ModelCollection[newIndex];

                SwapModels(leftView, rightView);
                rightView.WorkShiftRuleSet.SwapExtenders(rightView.WorkShiftExtender, leftView.WorkShiftExtender);
            }
        }

        private void activityTypeChanged(object sender, ActivityTypeChangedEventArgs e)
        {
            if (e.ActivityType == ActivityType.AbsolutePosition)
            {
                //this is rather strange it is created here that means
                //that it is recreated each time you switch between absolute
                //and relative. It should be created only once.
                var autoPos = (AutoPositionViewModel) e.Item;
                autoPos.ActivityTypeChanged -= activityTypeChanged;

                var timePeriod = new TimePeriod(autoPos.ALMinTime.Hours,
                                                       autoPos.ALMinTime.Minutes,
                                                       autoPos.ALMaxTime.Hours,
                                                       autoPos.ALMaxTime.Minutes);

                var autoTimePeriod = new TimePeriod(8, 0, 9, 0);
                var activityLengthWithSegment = new TimePeriodWithSegment(timePeriod, autoPos.ALSegment);

                var activityPositionWithSegment = new TimePeriodWithSegment(8,0,9,0, Explorer.Model.DefaultSegment);
                
                if (!autoPos.APSegment.Equals(TimeSpan.Zero))
                {
                    activityPositionWithSegment = new TimePeriodWithSegment(autoTimePeriod, autoPos.APSegment);
                }
                var extender = new ActivityAbsoluteStartExtender(autoPos.CurrentActivity,
                                                                                            activityLengthWithSegment,
                                                                                            activityPositionWithSegment);
                var adapter = new AbsolutePositionViewModel(autoPos.WorkShiftRuleSet, extender);
                adapter.ActivityTypeChanged += activityTypeChanged;

	            var index = autoPos.WorkShiftRuleSet.ExtenderCollection.IndexOf(autoPos.ContainedEntity);
				autoPos.WorkShiftRuleSet.DeleteExtender(autoPos.ContainedEntity);
                autoPos.WorkShiftRuleSet.InsertExtender(index, extender);
                ReplaceModel(autoPos, adapter);
            }
            else
            {
                var absPos = (AbsolutePositionViewModel)e.Item;
                absPos.ActivityTypeChanged -= activityTypeChanged;
                
                var timePeriod = new TimePeriod(absPos.ALMinTime.Hours,
                                                       absPos.ALMinTime.Minutes,
                                                       absPos.ALMaxTime.Hours,
                                                       absPos.ALMaxTime.Minutes);
                var activityLengthWithSegment = new TimePeriodWithSegment(timePeriod, absPos.ALSegment);
                var autoPositionedActivityExtender = new AutoPositionedActivityExtender(absPos.CurrentActivity,
                                                                                                         activityLengthWithSegment,
                                                                                                         absPos.APSegment);
                var autoPositionAdapter = new AutoPositionViewModel(absPos.WorkShiftRuleSet, autoPositionedActivityExtender);
                autoPositionAdapter.ActivityTypeChanged += activityTypeChanged;

	            var index = absPos.WorkShiftRuleSet.ExtenderCollection.IndexOf(absPos.ContainedEntity);
                absPos.WorkShiftRuleSet.DeleteExtender(absPos.ContainedEntity);
	            absPos.WorkShiftRuleSet.InsertExtender(index, autoPositionedActivityExtender);
				ReplaceModel(absPos, autoPositionAdapter);
            }
            Explorer.View.RefreshActivityGridView();
        }
    }
}
