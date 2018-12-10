using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class AccessibilityDatePresenter : BasePresenter<IAccessibilityDateViewModel>, 
                                              IAccessibilityDatePresenter
    {
        public AccessibilityDatePresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
            : base(explorer,dataHelper)
        {
        }

        public override void LoadModelCollection()
        {
            var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
            if (filteredRuleSetCollection.Count > 0)
            {
                ClearModelCollection();

                foreach (IWorkShiftRuleSet ruleSet in filteredRuleSetCollection)
                {
                    foreach (var date in ruleSet.AccessibilityDates)
                    {
                        IAccessibilityDateViewModel model = new AccessibilityDateViewModel(ruleSet, date.Date);
                        AddToModelCollection(model);
                    }
                }
            }
        }

        public void AddAccessibilityDate()
        {
            var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
            if(filteredRuleSetCollection != null &&
               filteredRuleSetCollection.Count > 0)
            {
                foreach (IWorkShiftRuleSet ruleSet in filteredRuleSetCollection)
                {
                    var dateTime = DateOnly.Today;

                    var existingDates = ruleSet.AccessibilityDates.ToList();
                    existingDates.Sort();
                    if (existingDates.Count > 0)
                        dateTime = existingDates[existingDates.Count - 1].Add(TimeSpan.FromDays(1));
                    IAccessibilityDateViewModel model = new AccessibilityDateViewModel(ruleSet, dateTime.Date);
                    AddToModelCollection(model);
                    ruleSet.AddAccessibilityDate(dateTime);
                }
            }
        }

        public void RemoveSelectedAccessibilityDates(ReadOnlyCollection<int> dates)
        {
            if (dates.Count > 0)
            {
                if (ModelCollection.Count > 0)
                {
                    IList<IAccessibilityDateViewModel> toBeDeleted = new List<IAccessibilityDateViewModel>();
                    foreach (int index in dates)
                    {
                        int ruleSetIndex;
                        IAccessibilityDateViewModel accessDates = ModelCollection[index];

                        var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
                        if ((ruleSetIndex = filteredRuleSetCollection.IndexOf(accessDates.WorkShiftRuleSet)) >= 0)
                        {
                            IWorkShiftRuleSet ruleSet = filteredRuleSetCollection[ruleSetIndex];
                            ruleSet.RemoveAccessibilityDate(new DateOnly(accessDates.Date));
                            toBeDeleted.Add(accessDates);
                        }
                    }
                    foreach (IAccessibilityDateViewModel adapter in toBeDeleted)
                        RemoveFromCollection(adapter);
                    toBeDeleted.Clear();
                }
            }
        }

        public void SetAccessibilityDates(ReadOnlyCollection<IAccessibilityDateViewModel> dates)
        {
            SetModelCollection(dates);
        }
    }
}
