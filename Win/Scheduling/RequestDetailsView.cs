using System;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class RequestDetailsView : BaseRibbonForm, IRequestDetailsView
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly PersonRequestViewModel _model;
		private readonly RequestDetailsShiftTradeView _requestDetailsShiftTradeView;
		private readonly RequestDetailsPresenter _presenter;

		public RequestDetailsView()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				SetTexts();
				SetColors();
				SetToolStripsToPreferredSize();
			}
		}

		public RequestDetailsView(IEventAggregator eventAggregator, IPersonRequestViewModel model, IScheduleDictionary schedules)
			: this()
		{
			_eventAggregator = eventAggregator;
			_presenter = new RequestDetailsPresenter(this, model);
			_model = model as PersonRequestViewModel;
			_presenter.Initialize();
			if (_presenter.IsShiftTradeRequest())
				_requestDetailsShiftTradeView = new RequestDetailsShiftTradeView(model, schedules);
			if (!_presenter.IsRequestEditable())
			{
				toolStripButtonDeny.Enabled = false;
				toolStripButtonApprove.Enabled = false;
				toolStripButtonReply.Enabled = false;
				toolStripButtonReplyAndApprove.Enabled = false;
				toolStripButtonReplyAndDeny.Enabled = false;
			}
		}

		private void SetToolStripsToPreferredSize()
		{
			toolStripExMain.Size = toolStripExMain.PreferredSize;
		}

		private void SetColors()
		{
			BackColor = UserTexts.ThemeSettings.Default.StandardOfficeFormBackground;
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			foreach (QuickButtonReflectable quickAccessItem in ribbonControlAdvMain.Header.QuickItems)
			{
				quickAccessItem.Text = quickAccessItem.ReflectedButton.Text;
				quickAccessItem.ToolTipText = quickAccessItem.ReflectedButton.ToolTipText;
			}
		}

		public string Subject
		{
			get { return textBoxExtSubject.Text; }
			set { textBoxExtSubject.Text = value; }
		}

		public string Message
		{
			get { return textBoxExtMessage.Text; }
			set { textBoxExtMessage.Text = value; }
		}

		public string LabelName
		{
			get { return textBoxExtName.Text; }
			set {textBoxExtName.Text = value;}
		}

		public string Status
		{
			get { return textBoxExtStatus.Text; }
			set { textBoxExtStatus.Text = value; }
		}

		private void dockShiftTradeGridControl()
		{
			_requestDetailsShiftTradeView.SuspendLayout();
			_requestDetailsShiftTradeView.Dock = DockStyle.Fill;
			tableLayoutPanelMain.RowCount = 5;
			tableLayoutPanelMain.Controls.Add(_requestDetailsShiftTradeView, 0, 5);
			Height += _requestDetailsShiftTradeView.Height;
			tableLayoutPanelMain.SetColumnSpan(_requestDetailsShiftTradeView, 2);
			_requestDetailsShiftTradeView.ResumeLayout();
		}

		private void RequestDetailsView_Load(object sender, EventArgs e)
		{
			if (_requestDetailsShiftTradeView != null)
				dockShiftTradeGridControl();
			else
				tableLayoutPanelMain.Dock = DockStyle.Top;
		}

		private void toolStripButtonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripButtonApprove_Click(object sender, EventArgs e)
		{
			new ApproveRequestFromRequestDetailsView(_model).PublishEvent("ApproveRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonDeny_Click(object sender, EventArgs e)
		{
			new DenyRequestFromRequestDetailsView(_model).PublishEvent("DenyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReply_Click(object sender, EventArgs e)
		{
			new ReplyRequestFromRequestDetailsView(_model).PublishEvent("ReplyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReplyAndApprove_Click(object sender, EventArgs e)
		{
			new ReplyAndApproveRequestFromRequestDetailsView(_model).PublishEvent("ReplyAndApproveRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReplyAndDeny_Click(object sender, EventArgs e)
		{
			new ReplyAndDenyRequestFromRequestDetailsView(_model).PublishEvent("ReplyAndDenyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void RequestDetailsView_Resize(object sender, EventArgs e)
		{
            if (_requestDetailsShiftTradeView != null) _requestDetailsShiftTradeView.Refresh();
		}
	}
}
