﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
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
                dateTimePickerAdvFrom.SetCultureInfoSafe(System.Globalization.CultureInfo.CurrentCulture);
                dateTimePickerAdvTo.SetCultureInfoSafe(System.Globalization.CultureInfo.CurrentCulture);
            }
        }

        public void Initialize(FindPersonsModel model, IApplicationFunction applicationFunction)
        {
            dateTimePickerAdvFrom.ValueChanged -= dateTimePickerAdvFrom_ValueChanged;
            dateTimePickerAdvTo.ValueChanged -= dateTimePickerAdvTo_ValueChanged;

            _presenter = new FindPersonsPresenter(this, model, applicationFunction);
            _presenter.Initialize();

            dateTimePickerAdvFrom.ValueChanged += dateTimePickerAdvFrom_ValueChanged;
            dateTimePickerAdvTo.ValueChanged += dateTimePickerAdvTo_ValueChanged;

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

        private void textBoxExtFind_TextChanged(object sender, EventArgs e)
        {
            _presenter.RefreshResult();
        }

        private void treeViewAdvPreviewTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
             var nodes = e.Item as TreeNodeAdv[];

             if (nodes == null || nodes.Length < 1) return;
      
             var node = nodes[0];
             treeViewAdvResult.DoDragDrop(node, DragDropEffects.Move);
        }

         private void dateTimePickerAdvFrom_ValueChanged(object sender, EventArgs e)
         {
             _presenter.FromDateChanged();
         }

         private void dateTimePickerAdvTo_ValueChanged(object sender, EventArgs e)
         {
             _presenter.ToDateChanged();
         }

        private void contextMenuStripTreeActions_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _selectedNodes = treeViewAdvResult.SelectedNodes;
        }

        private void toolStripMenuItemCut_Click(object sender, EventArgs e)
        {
            CutNodes = _selectedNodes;
        }

        public void ClearNodes()
        {
            CutNodes = null;
        }

        private void treeViewAdvResult_KeyDown(object sender, KeyEventArgs e)
        {
             if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
             {
                 _selectedNodes = treeViewAdvResult.SelectedNodes;
                 CutNodes = _selectedNodes;
             }

             base.OnKeyDown(e);
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

