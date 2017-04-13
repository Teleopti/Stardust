using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Shifts.Presenters
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

        public void SetDaysOfWeekCollection(IList<IDaysOfWeekViewModel> value)
        {
            SetModelCollection(new ReadOnlyCollection<IDaysOfWeekViewModel>(value));
        }
    }
}
