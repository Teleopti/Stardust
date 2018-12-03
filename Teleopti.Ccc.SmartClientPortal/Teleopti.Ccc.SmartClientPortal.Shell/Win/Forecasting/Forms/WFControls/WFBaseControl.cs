using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls
{
    public partial class WFBaseControl : BaseUserControl
    {
        private ForecastWorkflowPresenter _presenter;

        public WFBaseControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        protected virtual void OnOutlierNeedRefresh(IOutlier outlier)
        {
        }

        protected virtual void SetColors()
        {
        }

        public void Initialize(ForecastWorkflowPresenter presenter)
        {
            _presenter = presenter;
            SetColors();
            AfterInitialize();
        }

        protected virtual void AfterInitialize()
        {
        }

        public void OutlierRootChanged(IOutlier outlier)
        {
            OnOutlierNeedRefresh(outlier);
        }

        //todo: will try to remove, inject instead
        public ForecastWorkflowPresenter Presenter
        {
            get { return _presenter; }
        }
        
        public void CreateNewOutlier(IList<DateOnly> dateList)
        {
            var newOutlierEditor = new EditOutlier(_presenter.Model.Outliers);
            newOutlierEditor.Initialize(_presenter.Model.Workload, dateList);
            var result = newOutlierEditor.ShowDialog(this);
            if (result != DialogResult.OK) return;
            _presenter.AddOutlier(newOutlierEditor.Outlier);
        }

        public void EditOutlier(IOutlier outlier)
        {
            using (var editOutlier = new EditOutlier(_presenter.Model.Outliers))
            {
                editOutlier.Initialize(outlier);
                var result = editOutlier.ShowDialog(this);
                if (result != DialogResult.OK) return;
                _presenter.EditOutlier(editOutlier.Outlier);
            }
        }

        public void DeleteOutlier(IOutlier outlier)
        {
            _presenter.RemoveOutlier(outlier);
        }

        public virtual void PrepareSave()
        {
        }

        public virtual  void Cancel()
        {}

        private void ReleaseManagedResources()
        {
            _presenter = null;
        }
    }
}
