using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class DaysOfWeekPresenter : BasePresenter<IDaysOfWeekViewModel>, IDaysOfWeekPresenter
    {
        public DaysOfWeekPresenter(IExplorerPresenter explorer, IDataHelper dataHelper)
            : base(explorer,dataHelper)
        {}

        public override void LoadModelCollection()
        {
            var filteredRuleSetCollection = Explorer.Model.FilteredRuleSetCollection;
			ClearModelCollection();

            if (filteredRuleSetCollection.Count > 0)
            {
                var modelList = new List<IDaysOfWeekViewModel>();
                foreach (IWorkShiftRuleSet ruleSet in filteredRuleSetCollection)
                    modelList.Add(new DaysOfWeekViewModel(ruleSet));
                SetModelCollection(new ReadOnlyCollection<IDaysOfWeekViewModel>(modelList));
            }
        }
    }
}
