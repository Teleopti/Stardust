using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;

namespace Teleopti.Ccc.AgentPortal.Requests.ShiftTrade
{
    public partial class ShiftTradeView : BaseRibbonForm, IShiftTradeView
    {
        private readonly ShiftTradePresenter _presenter;
        private readonly ShiftTradeVisualView _shiftTradeVisualView;
        private readonly bool _updateState;
        private bool _forceOpen;
        private PopupControlContainer _popupControlAddDates;

        internal protected ShiftTradeView()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                SetTexts();
                SetColors();
                SetToolStripsToPreferredSize();
            }
        }

        public ShiftTradeView(ShiftTradeModel model, bool updateState)
            : this()
        {
            _updateState = updateState;
            _shiftTradeVisualView = new ShiftTradeVisualView(model);
            _shiftTradeVisualView.RemoveSelectedDays += _shiftTradeVisualView_RemoveSelectedDays;
            _presenter = new ShiftTradePresenter(this, model);
            _presenter.Initialize();
        }

        private void _shiftTradeVisualView_RemoveSelectedDays(object sender, EventArgs e)
        {
            _presenter.DeleteSelectedDates();
        }

        private void ShiftTradeForm_Load(object sender, EventArgs e)
        {
            dockControl();
            enableContent(!_updateState);
        }

        private void SetToolStripsToPreferredSize()
        {
            toolStripExActions.Size = toolStripExActions.PreferredSize;
            toolStripExMain.Size = toolStripExMain.PreferredSize;
            toolStripExAddDates.Size = toolStripExAddDates.PreferredSize;
        }

        private void SetColors()
        {
            BackColor = UserTexts.ThemeSettings.Default.StandardOfficeFormBackground;
        }

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            foreach(QuickButtonReflectable quickAccessItem in ribbonControlAdvMain.Header.QuickItems)
            {
                quickAccessItem.Text = quickAccessItem.ReflectedButton.Text;
                quickAccessItem.ToolTipText = quickAccessItem.ReflectedButton.ToolTipText;
            }
        }

       // public PersonRequestActionType Action { get; set; }

        public void SetInitialDate(DateTime initialDate)
        {
            addDatesView1.SetInitialDate(initialDate);
        }

        public void SetStatus(string value)
        {
            textBoxExtStatus.Text = value;
        }

        public string Message
        {
            set
            {
                textBoxExtMessage.Clear();
                //?Primitivt, 2009, eller?
                textBoxExtMessage.Lines = value.Split(Environment.NewLine.ToCharArray());
            }
            get { return textBoxExtMessage.Text; }
        }

        public string Subject
        {
            set { textBoxExtSubject.Text = value;}
            get { return textBoxExtSubject.Text; }
        }

        public void SetPersonName(string value)
        {
            textBoxExtName.Text = value;
        }

        public void SetLabelName(string value)
        {
            labelName.Text = value;
        }

        public void SetPanelAcceptDenyVisible(bool visible)
        {
            tableLayoutPanelMain.Visible = visible;
        }

        public void SetAcceptButtonEnabled(bool value)
        {
            toolStripButtonAccept.Enabled = value;
        }

        public void SetDenyButtonEnabled(bool value)
        {
            toolStripButtonDeny.Enabled = value;
        }

        public void SetDeleteButtonEnabled(bool value)
        {
            toolStripButtonDelete.Enabled = value;
        }

        public void SetResponseTabEnabled(bool value)
        {
            toolStripTabItemResponse.Enabled = value;
        }

        public void SetOkButtonVisible(bool value)
        {
            toolStripButtonSaveAndClose.Enabled = value;
        }

        public void SetDialogResult(DialogResult value)
        {
            DialogResult = value;
        }

        public void SetReasonMessageVisibility(bool value)
        {
            tableLayoutPanelMain.RowStyles[0].Height = value ? 30F : 0F;
            labelReturnMessage.Visible = value;
        }

        public void DisableMessage()
        {
            textBoxExtMessage.ReadOnly = true;
        }

        private void toolStripButtonAccept_Click(object sender, EventArgs e)
        {
            _presenter.Accept();
        }

        private void toolStripButtonDeny_Click(object sender, EventArgs e)
        {
            _presenter.Deny();
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            _presenter.Delete();
        }

        private void dockControl()
        {
            _shiftTradeVisualView.SuspendLayout();
            _shiftTradeVisualView.Dock = DockStyle.Fill;
            tableLayoutPanelMain.Controls.Add(_shiftTradeVisualView,0,5);
            tableLayoutPanelMain.SetColumnSpan(_shiftTradeVisualView,2);
            _shiftTradeVisualView.ResumeLayout();
        }

        private void enableContent(bool enabled)
        {
            _shiftTradeVisualView.EnableContent(enabled);
            textBoxExtSubject.ReadOnly = !enabled;
            toolStripButtonAddDays.Enabled = enabled;
            toolStripButtonRemoveDate.Enabled = enabled;
        }

        private void addDatesView1_DatesSelected(object sender, DateRangeSelectionEventArgs e)
        {
            _presenter.AddDateRange(e.DateRange.StartDate, e.DateRange.EndDate);
            _popupControlAddDates.HidePopup(PopupCloseType.Done);
        }

        public void RefreshVisualView()
        {
            _shiftTradeVisualView.OnRefresh();
        }

        public string ReasonMessage
        {
            get { return labelReturnMessage.Text; }
            set { labelReturnMessage.Text = value; }
        }

        private void addDatesView1_BeforePopup(object sender, EventArgs e)
        {
            //Must do like this to keep the add dates control open when opening the calendar
            _forceOpen = true;
        }

        private void addDatesView1_PopupClosed(object sender, EventArgs e)
        {
            //Must do like this to keep the add dates control open when opening the calendar
            _forceOpen = false;
        }

        private void popupControlAddDates_BeforeCloseUp(object sender, CancelEventArgs e)
        {
            //Must do like this to keep the add dates control open when opening the calendar
            e.Cancel = _forceOpen;
            if (!e.Cancel)
                toolStripButtonAddDays.Checked = false;
        }

        private void toolStripButtonRemoveDate_Click(object sender, EventArgs e)
        {
            _presenter.DeleteSelectedDates();
        }

        public ICollection<DateTime> SelectedDates
        {
            get { return _shiftTradeVisualView.SelectedDates(); }
        }

        private void toolStripButtonAddDays_Click(object sender, EventArgs e)
        {
            if (toolStripButtonAddDays.Checked)
            {
                _popupControlAddDates.HidePopup(PopupCloseType.Deactivated);
            }
            else
            {
                _popupControlAddDates = new PopupControlContainer();
                _popupControlAddDates.Controls.Add(addDatesView1);
                _popupControlAddDates.Name = "popupControlAddDates";
                _popupControlAddDates.ParentControl = toolStripExAddDates;
                _popupControlAddDates.BeforeCloseUp += popupControlAddDates_BeforeCloseUp;

                toolStripButtonAddDays.Checked = true;
                var locationOnForm = toolStripExAddDates.PointToScreen(new Point(0, toolStripButtonAddDays.Height + 2));
                _popupControlAddDates.ShowPopup(locationOnForm);
            }
        }

        private void toolStripButtonSaveAndClose_Click(object sender, EventArgs e)
        {
            _presenter.Ok(_updateState);
        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            _presenter.Cancel();
        }
    }
}