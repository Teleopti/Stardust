using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class ActivityTimeLimiterPresenter : BasePresenter<IActivityTimeLimiterViewModel>,
                                                IActivityTimeLimiterPresenter
    {
        public ActivityTimeLimiterPresenter(IExplorerPresenter explorer, IDataHelper dataHelper) 
            : base(explorer,dataHelper)
        {}

        public void AddAndSaveLimiter()
        {
            var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
            if (filteredRuleSetCollection.Count > 0)
            {
                foreach (IWorkShiftRuleSet ruleSet in filteredRuleSetCollection)
                {
                    ActivityTimeLimiter newLimiter = DataWorkHelper.CreateDefaultActivityTimeLimiter(ruleSet,
                                                                                                     Explorer.Model.DefaultStartPeriod.SpanningTime());
                    IActivityTimeLimiterViewModel model = new ActivityTimeLimiterViewModel(ruleSet, newLimiter);
                    AddToModelCollection(model);
                }
            }
        }

        public void DeleteLimiter(ReadOnlyCollection<int> selectedLimiters)
        {
            if (selectedLimiters != null && selectedLimiters.Count > 0)
            {
                if (ModelCollection.Count > 0)
                {
                    var listToDelete = new List<IActivityTimeLimiterViewModel>();

                    foreach (int index in selectedLimiters)
                    {
                        IActivityTimeLimiterViewModel limiter = ModelCollection[index];
                        int ruleSetIndex = Explorer.Model.FilteredRuleSetCollection.IndexOf(limiter.WorkShiftRuleSet);
                        if (ruleSetIndex >= 0)
                        {
                            limiter.WorkShiftRuleSet.DeleteLimiter(limiter.ContainedEntity);
                            DataWorkHelper.Save(limiter.WorkShiftRuleSet);
                            listToDelete.Add(limiter);
                        }
                    }

                    foreach (IActivityTimeLimiterViewModel limiter in listToDelete)
                        RemoveFromCollection(limiter);
                    listToDelete.Clear();
                }
            }
        }

        public override void LoadModelCollection()
        {
            var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
			ClearModelCollection();

            if (filteredRuleSetCollection.Count > 0)
            { 
                var modelList = new List<IActivityTimeLimiterViewModel>();
                foreach (IWorkShiftRuleSet ruleSet in filteredRuleSetCollection)
                    modelList.AddRange(parse(ruleSet, ruleSet.LimiterCollection));
                SetModelCollection(new ReadOnlyCollection<IActivityTimeLimiterViewModel>(modelList));
            }
        }

        private static IEnumerable<IActivityTimeLimiterViewModel> parse(IWorkShiftRuleSet ruleSet, IEnumerable<IWorkShiftLimiter> entities)
        {
            IList<IActivityTimeLimiterViewModel> limiterViewCollection = new List<IActivityTimeLimiterViewModel>();
            foreach (IWorkShiftLimiter item in entities)
            {
                var timeLimiter = item as ActivityTimeLimiter;
                if (timeLimiter != null)
                    limiterViewCollection.Add(parse(ruleSet, timeLimiter));
            }
            return limiterViewCollection;
        }

        private static IActivityTimeLimiterViewModel parse(IWorkShiftRuleSet ruleSet, ActivityTimeLimiter entity)
        {
            return new ActivityTimeLimiterViewModel(ruleSet, entity);
        }
    }
}
