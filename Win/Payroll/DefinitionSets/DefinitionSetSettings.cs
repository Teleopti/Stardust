﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Payroll;
using Teleopti.Ccc.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Payroll.DefinitionSets
{
    public partial class DefinitionSetSettings: PayrollBaseUserControl, IExplorerView , ISettingPage
    {
        private IUnitOfWork _unitOfWork;
        private IExplorerPresenter _explorerPresenter;
        private UserControl _definitionSetView;
        private UserControl _visualizeView;
        private const ClipboardOperation ClipboardAction = ClipboardOperation.Copy;
        private UserControl _multiplicatorDefinitionView;

        public DefinitionSetSettings()
        {
            InitializeComponent();
        }

        public IUnitOfWork UnitOfWork
        {
            get { return _unitOfWork; }
        }

        public IExplorerPresenter ExplorerPresenter
        {
            get { return _explorerPresenter;}
        }

        public ClipboardOperation ClipboardActionType
        {
            get{ return ClipboardAction; }
        }

        public void SetSelectedView(PayrollViewType view)
        {
            //_currentView = view;
        }

        public void SetClipboardControlState(ClipboardOperation action, bool status)
        {
          
        }

        public float GetWidthOfVisualizeControlContainer()
        {
            float panelWidth = splitContainerBottom.Panel2.Width;
            return panelWidth;
        }

        public void Refresh(PayrollViewType view)
        {
            if (_explorerPresenter == null)
                return;
            GetCommonBehaviorInstance(view).RefreshView();
        }

        public void RefreshSelectedViews()
        {
            IEnumerable<ICommonBehavior> commonBehaviors = GetCommonBehaviorInstances();

            foreach (var behavior in commonBehaviors)
            {
                behavior.Reload();
            }
        }

        public bool CheckForDelete()
        {
           throw new NotImplementedException();
        }

        public void InitializeDialogControl()
        {
            SetColors();
            SetTexts();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            gradientPanelHeader.BackgroundColor = ColorHelper.OptionsDialogHeaderGradientBrush();
            labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
            splitContainerMain.BackColor = ColorHelper.GridControlGridInteriorColor();
            splitContainerMain.Panel1.BackColor = ColorHelper.WizardBackgroundColor();
            splitContainerMain.Panel2.BackColor = ColorHelper.WizardBackgroundColor();
            splitContainerBottom.BackColor = ColorHelper.GridControlGridInteriorColor();
            splitContainerBottom.Panel1.BackColor = ColorHelper.WizardBackgroundColor();
            splitContainerBottom.Panel2.BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void LoadControl()
        {
            _explorerPresenter = new ExplorerPresenter(new PayrollHelper(_unitOfWork), this);
            int defaultSegment;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                defaultSegment = new GlobalSettingDataRepository(uow).FindValueByKey("DefaultSegment", new DefaultSegment()).SegmentLength;
            }
            _explorerPresenter.Model.SetDefaultSegment(defaultSegment);
            _explorerPresenter.Model.SetRightToLeft(RightToLeft == RightToLeft.Yes);

            _explorerPresenter.Model.SetSelectedDate(DateOnly.Today);

            InstantiateMultiplicatorDefinitionView();
            InstantiateVisualizeView();
            InstantiateDefinitionSetView();
        }

        public void SaveChanges()
        {
        }

        public void Unload()
        {
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            _unitOfWork = value;
        }

        public void Persist()
        {}

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.Scheduling);
        }

        public string TreeNode()
        {
            return Resources.MultiplicatorDefinitionSets;
        }

    	public void OnShow()
    	{
    	}

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
        }


        public ViewType ViewType
        {
            get { return ViewType.MultiplicatorDefinitionSets; }
        }

        private ICommonBehavior GetCommonBehaviorInstance(PayrollViewType view)
        {
            object common;
            switch (view)
            {
                case PayrollViewType.DefinitionSetDropDown:
                    common = _definitionSetView;
                    break;
                case PayrollViewType.VisualizeControl:
                    common = _visualizeView;
                    break;
                default:
                    common = _multiplicatorDefinitionView;
                    break;

            }
            var commonBehavior = (ICommonBehavior)common;
            return commonBehavior;
        }


        private IEnumerable<ICommonBehavior> GetCommonBehaviorInstances()
        {
            IList<ICommonBehavior> commonBehaviors = new List<ICommonBehavior>
                                                     	{
                                                     		(ICommonBehavior) _visualizeView,
                                                     		(ICommonBehavior) _multiplicatorDefinitionView
                                                     	};
        	return commonBehaviors;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InstantiateDefinitionSetView()
        {
            _explorerPresenter.DefinitionSetPresenter.LoadModel();
            _definitionSetView = new DefinitionSetDropDownView(this) {Dock = DockStyle.Fill};

        	IContainedControl container = new PayrollControlContainer(_definitionSetView , Resources.DefinitionSets);
            splitContainerMain.Panel1.Controls.Add(container.UserControl);
            container.UserControl.Dock = DockStyle.Fill;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void InstantiateMultiplicatorDefinitionView()
        {
            _multiplicatorDefinitionView = new MultiplicatorDefinitionView(this) {Dock = DockStyle.Fill};

        	var multiplicatorDefinitionView = _multiplicatorDefinitionView as IMultiplicatorDefinitionView;
            multiplicatorDefinitionView.GridDataChanged += (ContainerControlDefinitionChanged);
            
            IContainedControl container = new PayrollControlContainer(_multiplicatorDefinitionView, Resources.MultiplicatorGrid);
            splitContainerBottom.Panel2.Controls.Clear();
            splitContainerBottom.Panel2.Controls.Add(container.UserControl);
            container.UserControl.Dock = DockStyle.Fill;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InstantiateVisualizeView()
        {
            splitContainerBottom.Panel1.Controls.Remove(_visualizeView);
            _visualizeView = new VisualizeView(this) {Anchor = AnchorStyles.Left};
        	splitContainerBottom.Panel1.Controls.Add(_visualizeView);
            _visualizeView.Dock = DockStyle.Fill;
        }

        private void SplitContainerTopPanel2Resize(object sender, EventArgs e)
        {
            RefreshVisualizer();
        }

        private void DefinitionSetSettingsGotFocus(object sender, PaintEventArgs e)
        {
            //Updates the MultiplicatorList upon focusing on the control.
            //This is to reflect the multiplicator changes in overime settings page. 
            if(_explorerPresenter != null)
            {
                if(_explorerPresenter.Helper != null)
                {
                    _explorerPresenter.Helper.LoadMultiplicatorList();
                }
            }
        }

        private void ContainerControlDefinitionChanged(object sender , EventArgs e)
        {
            RefreshVisualizer();
        }

        private void RefreshVisualizer()
        {
            ICommonBehavior commonBehavior = GetCommonBehaviorInstance(PayrollViewType.VisualizeControl);
            if (commonBehavior != null)
                commonBehavior.RefreshView();
        }
    }
}
