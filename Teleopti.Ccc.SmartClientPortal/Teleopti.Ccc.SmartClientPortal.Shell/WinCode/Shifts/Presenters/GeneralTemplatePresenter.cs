using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class GeneralTemplatePresenter : BasePresenter<IGeneralTemplateViewModel>, 
                                            IGeneralTemplatePresenter
    {
        public GeneralTemplatePresenter(IExplorerPresenter explorer, IDataHelper dataHelper) : base(explorer, dataHelper)
        {
        }

        public override void LoadModelCollection()
        {
            ClearModelCollection();
            foreach (IWorkShiftRuleSet ruleSet in Explorer.Model.FilteredRuleSetCollection)
                AddToModelCollection(new GeneralTemplateViewModel(ruleSet, Explorer.Model.DefaultSegment));
        }

		public virtual event EventHandler<EventArgs> OnlyForRestrictionsChanged;

		public void InvokeOnlyForRestrictionsCellChanged()
		{
			if (OnlyForRestrictionsChanged != null)
			{
				OnlyForRestrictionsChanged.Invoke(this, EventArgs.Empty);
			}
		}
    }
}
