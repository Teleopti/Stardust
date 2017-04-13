using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
    public partial class WFValidate : WFBaseControl
    {
        public WFValidate()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        protected override void AfterInitialize()
        {
            base.AfterInitialize();
            
            if (Presenter == null) return;

            workflowValidateView.Initialize(this);
        }

        public override void Cancel()
        {
            base.Cancel();
            workflowValidateView.PerformCancel();
        }

        protected override void OnOutlierNeedRefresh(IOutlier outlier)
        {
            base.OnOutlierNeedRefresh(outlier);
            workflowValidateView.RefreshOutliers();
        }

        private void ReleaseManagedResources()
        {
            workflowValidateView.Dispose();
        }
    }
}
