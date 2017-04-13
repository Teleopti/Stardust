﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Grouping
{
	public partial class FindPersonsView : BaseUserControl, IFindPersonsView
	{
		private FindPersonsPresenter _presenter;
		private SelectedNodesCollection _selectedNodes;
		private readonly ErrorProvider _errorProvider = new ErrorProvider();

		public FindPersonsView()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				SetTexts();
			}
		}

		public void Initialize(FindPersonsModel model, IApplicationFunction applicationFunction, IComponentContext container)
		{
			dateTimePickerAdvFrom.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			dateTimePickerAdvTo.SetCultureInfoSafe(CultureInfo.CurrentCulture);

			dateTimePickerAdvFrom.ValueChanged -= dateTimePickerAdvFromValueChanged;
			dateTimePickerAdvTo.ValueChanged -= dateTimePickerAdvToValueChanged;

			_presenter = new FindPersonsPresenter(this, model, applicationFunction, container);
			_presenter.Initialize();

			dateTimePickerAdvFrom.ValueChanged += dateTimePickerAdvFromValueChanged;
			dateTimePickerAdvTo.ValueChanged += dateTimePickerAdvToValueChanged;

			dateTimePickerAdvFrom.ForeColor = Color.Black;
			dateTimePickerAdvTo.ForeColor = Color.Black;

			_errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
		}

		public DateTime FromDate
		{
			get { return dateTimePickerAdvFrom.Value; }
			set { dateTimePickerAdvFrom.Value = value; }
		}

		public DateTime ToDate
		{
			get { return dateTimePickerAdvTo.Value; }
			set { dateTimePickerAdvTo.Value = value; }
		}

		public string FindText
		{
			get { return textBoxExtFind.Text; }
			set { textBoxExtFind.Text = value; }
		}

		public TreeNodeAdvCollection Result
		{
			get { return treeViewAdvResult.Nodes; }
		}

		public SelectedNodesCollection CutNodes
		{
			get; private set;
		}

		public bool TextBoxFindEnabled
		{
			get { return textBoxExtFind.Enabled; }
			set { textBoxExtFind.Enabled = value; }
		}

		private void textBoxExtFindTextChanged(object sender, EventArgs e)
		{
			_presenter.RefreshResult();
		}

		private void treeViewAdvPreviewTreeItemDrag(object sender, ItemDragEventArgs e)
		{
			var nodes = e.Item as TreeNodeAdv[];

			if (nodes == null || nodes.Length < 1) return;

			var node = nodes[0];
			treeViewAdvResult.ShowDragNodeCue = true;
			treeViewAdvResult.DoDragDrop(node, DragDropEffects.Move);
		}

		public void TryStopDragMode()
		{
			//bug #34898 workaround for bug in syncfusion that does not know that drag is stopped
			treeViewAdvResult.ShowDragNodeCue = false;
		}

		private void dateTimePickerAdvFromValueChanged(object sender, EventArgs e)
		{
			_presenter.FromDateChanged();
		}

		private void dateTimePickerAdvToValueChanged(object sender, EventArgs e)
		{
			_presenter.ToDateChanged();
		}

		private void contextMenuStripTreeActionsOpening(object sender, CancelEventArgs e)
		{
			_selectedNodes = treeViewAdvResult.SelectedNodes;
		}

		private void toolStripMenuItemCutClick(object sender, EventArgs e)
		{
			CutNodes = _selectedNodes;
		}

		public void ClearNodes()
		{
			CutNodes = null;
		}

		private void treeViewAdvResultKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
			{
				_selectedNodes = treeViewAdvResult.SelectedNodes;
				CutNodes = _selectedNodes;
			}

			OnKeyDown(e);
		}

		public void SetErrorOnEndDate(string errorValue)
		{
			_errorProvider.SetError(dateTimePickerAdvTo, errorValue);
			_errorProvider.SetIconPadding(dateTimePickerAdvTo, -35);
		}

		public void SetErrorOnStartDate(string errorValue)
		{
			_errorProvider.SetError(dateTimePickerAdvFrom, errorValue);
			_errorProvider.SetIconPadding(dateTimePickerAdvFrom, -35);
		}

		public void ClearDateErrors()
		{
			_errorProvider.SetError(dateTimePickerAdvTo, string.Empty);
			_errorProvider.SetError(dateTimePickerAdvFrom, string.Empty);
		}

	}
}

