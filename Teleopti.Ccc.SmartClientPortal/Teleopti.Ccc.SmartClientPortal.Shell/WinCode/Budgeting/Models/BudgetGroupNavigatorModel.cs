using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public class BudgetGroupNavigatorModel
    {
    	private readonly IPortalSettings _portalSettings;
    	private readonly IBudgetNavigatorDataService _service;
        public BudgetGroupRootModel BudgetRootModel { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public BudgetGroupNavigatorModel(IPortalSettings portalSettings, IBudgetNavigatorDataService service)
        {
        	_portalSettings = portalSettings;
        	_service = service;
        }

		public int BudgetingActionPaneHeight
		{
			get { return _portalSettings.BudgetingActionPaneHeight; }
		}

        public IBudgetNavigatorDataService DataService
        {
            get { return _service; }
        }
    }
}