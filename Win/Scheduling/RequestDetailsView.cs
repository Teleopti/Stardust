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
		private readonly RequestDetailsShiftTradeView _requestDetailsShiftTradeView;

		public RequestDetailsView()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				SetTexts();
				setToolStripsToPreferredSize();
			}
			Padding = new Padding(2);
		}

		public RequestDetailsView(IEventAggregator eventAggregator, IPersonRequestViewModel model, IScheduleDictionary schedules)
			: this()
		{
			_eventAggregator = eventAggregator;
			var presenter = new RequestDetailsPresenter(this, model);
			presenter.Initialize();
			if (presenter.IsShiftTradeRequest())
				_requestDetailsShiftTradeView = new RequestDetailsShiftTradeView(model, schedules);
			if (!presenter.IsRequestEditable())
			{
				toolStripButtonDeny.Enabled = false;
				toolStripButtonApprove.Enabled = false;
				toolStripButtonReply.Enabled = false;
				toolStripButtonReplyAndApprove.Enabled = false;
				toolStripButtonReplyAndDeny.Enabled = false;
			}
		}

		private void setToolStripsToPreferredSize()
		{
			toolStripExMain.Size = toolStripExMain.PreferredSize;
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

		private void requestDetailsViewLoad(object sender, EventArgs e)
		{
			if (_requestDetailsShiftTradeView != null)
				dockShiftTradeGridControl();
			else
				tableLayoutPanelMain.Dock = DockStyle.Top;
		}

		private void toolStripButtonApproveClick(object sender, EventArgs e)
		{
			new ApproveRequestFromRequestDetailsView().PublishEvent("ApproveRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonDenyClick(object sender, EventArgs e)
		{
			new DenyRequestFromRequestDetailsView().PublishEvent("DenyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReplyClick(object sender, EventArgs e)
		{
			new ReplyRequestFromRequestDetailsView().PublishEvent("ReplyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReplyAndApproveClick(object sender, EventArgs e)
		{
			new ReplyAndApproveRequestFromRequestDetailsView().PublishEvent("ReplyAndApproveRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void toolStripButtonReplyAndDenyClick(object sender, EventArgs e)
		{
			new ReplyAndDenyRequestFromRequestDetailsView().PublishEvent("ReplyAndDenyRequestFromRequestDetailsView", _eventAggregator);
			Close();
		}

		private void requestDetailsViewResize(object sender, EventArgs e)
		{
			if (_requestDetailsShiftTradeView != null) _requestDetailsShiftTradeView.Refresh();
		}
	}
}
