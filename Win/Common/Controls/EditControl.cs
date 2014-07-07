using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// there is 4 events associated with this control,
    /// one per main button and one per dropdown
    /// 
    /// note this control needs a width of 95 pxls in ribbon to show correctly
    /// </summary>
    /// <remarks>
    /// Created by: ostenpe
    /// Created date: 2008-08-17
    /// </remarks>
    public partial class EditControl : BaseUserControl
    {

  
        private IList<ToolStripItem> _deleteSpecialItems;

        /// <summary>
        /// add toolstripbuttons to this list
        /// </summary>
        public IList<ToolStripItem> DeleteSpecialItems
        {
            get { return _deleteSpecialItems; }
        }


        private IList<ToolStripItem> _newSpecialItems;

        /// <summary>
        /// add toolstripbuttons to this list
        /// </summary>
        public IList<ToolStripItem> NewSpecialItems
        {
            get { return _newSpecialItems; }
        }

        /// <summary>
        /// fires when the main [delete button is clicked].
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<EventArgs> DeleteClicked;

        /// <summary>
        /// fires when the main [new button is clicked].
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<EventArgs> NewClicked;

		private bool _internalFlag;

	    public EditControl()
	    {
		    InitializeComponent();
		    if (!DesignMode) SetTexts();

		    _deleteSpecialItems = new List<ToolStripItem>();
		    _newSpecialItems = new List<ToolStripItem>();
		    Load += new EventHandler(NewDeleteControl_Load);
		    toolStripSplitButtonNew.ButtonClick += new EventHandler(toolStripSplitButtonNew_Click);
		    toolStripButtonNew.Click += toolStripSplitButtonNew_Click;

		    toolStripSplitButtonDelete.ButtonClick += new EventHandler(toolStripSplitButtonDelete_Click);
		    toolStripButtonDelete.Click += toolStripSplitButtonDelete_Click;

		    toolStripSplitButtonNew.RightToLeft = RightToLeft.No;
		    toolStripSplitButtonDelete.RightToLeft = RightToLeft.No;

		    toolStripButtonNew.RightToLeft = RightToLeft.No;
		    toolStripButtonDelete.RightToLeft = RightToLeft.No;

		    toolStripButtonNew.Visible = false;
		    toolStripButtonDelete.Visible = false;
	    }

	    public ToolStripSplitButton ToolStripButtonNew
        {
            get { return toolStripSplitButtonNew; }   
        }

        public ToolStripPanelItem PanelItem
        {
            get { return toolStripPanelItem1; }
        }

        public void SetButtonState(EditAction thisButton, bool enabled)
        {
            switch (thisButton)
            {
                case EditAction.Delete:
                    toolStripSplitButtonDelete.Enabled = enabled;
                    break;
                case EditAction.New:
                    toolStripSplitButtonNew.Enabled = enabled;
                    break;
            }
        }

        public void SetSpecialItemState(EditAction thisDropDown, string thisTag, bool enabled)
        {
            switch (thisDropDown)
            {
                case EditAction.Delete:
                    SetEnabled(thisTag, enabled, _deleteSpecialItems);
                    break;
                case EditAction.New:
                    SetEnabled(thisTag, enabled, _newSpecialItems);
                    break;
            }
        }
        private static void SetEnabled(string thisTag, bool enabled, IEnumerable<ToolStripItem> list)
        {
            foreach (var c in list)
            {
                if (c.Tag.ToString() != thisTag) continue;
                c.Enabled = enabled;
            }
        }


        #region internal events
        void NewDeleteControl_Load(object sender, EventArgs e)
        {
            DeleteDropDownHandler();
            NewDropdownHandler();

        }
        void toolStripSplitButtonNew_Click(object sender, EventArgs e)
        {
        	var handler = NewClicked;
            if(handler != null)
            {
            	handler.Invoke(this,e);
            }
        }
        void toolStripSplitButtonDelete_Click(object sender, EventArgs e)
        {
        	var handler = DeleteClicked;
            if(handler!=null)
            {
            	handler.Invoke(this,e);
            }

        }

        #endregion




        #region NewSpecialItems area
        /// <summary>
        /// populates the dropdown at control_load event, sets the width 
        /// of the dropdownarea to 0 if no items in list
        /// also wires up the eventhandler for the dropdown 
        /// 
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        private void NewDropdownHandler()
        {
			_internalFlag = true;
            if (_newSpecialItems.Count == 0)
            {
	            toolStripSplitButtonNew.Visible = false;
                return;
            }
			toolStripSplitButtonNew.Visible = true;
            var drop = new ToolStripDropDown();
            foreach (var item in _newSpecialItems)
            {
                drop.Items.Add(item);
            }
            toolStripSplitButtonNew.DropDown = drop;
            toolStripSplitButtonNew.DropDown.ItemClicked += newDropDown_ItemClicked;
        }

        void newDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        	var handler = NewSpecialClicked;
            if (handler != null)
            {
            	handler.Invoke(sender, e);
            }
        }
        /// <summary>
        /// Occurs when [when item in special dropdown is clicked].
        /// bubbles the sender and the event from the local dropdown event
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<ToolStripItemClickedEventArgs> NewSpecialClicked;
        #endregion

        #region DeleteSpecialItems area
        /// <summary>
        /// populates the dropdown at control_load event, sets the width 
        /// of the dropdownarea to 0 if no items in list
        /// also wires up the eventhandler for the dropdown 
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        private void DeleteDropDownHandler()
        {
			_internalFlag = true;
            if (_deleteSpecialItems.Count == 0)
            {
	            toolStripSplitButtonDelete.Visible = false;
                return;
            }
			toolStripSplitButtonDelete.Visible = true;

            var drop = new ToolStripDropDown();
            foreach (var item in _deleteSpecialItems)
            {
                drop.Items.Add(item);
            }
            toolStripSplitButtonDelete.DropDown = drop;
            toolStripSplitButtonDelete.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(DeleteDropDown_ItemClicked);
        }

       
        void DeleteDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (_deleteSpecialItems != null)
                DeleteSpecialClicked.Invoke(sender, e);
        }

        /// <summary>
        /// Occurs when [when item in special dropdown is clicked].
        /// bubbles the sender and the event from the local dropdown event
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<ToolStripItemClickedEventArgs> DeleteSpecialClicked;
        #endregion

        public void CloseDropDown()
        {
            toolStripSplitButtonNew.DropDown.Close();
            toolStripSplitButtonDelete.DropDown.Close();
        }

		private void toolStripSplitButtonNew_VisibleChanged(object sender, EventArgs e)
		{
			if (_internalFlag)
			{
				toolStripButtonNew.Visible = !((ToolStripSplitButton)sender).Visible;
				_internalFlag = false;
				return;
			}
			toolStripButtonNew.Visible = ((ToolStripSplitButton)sender).Visible;
		}

		private void toolStripSplitButtonNew_EnabledChanged(object sender, EventArgs e)
		{
			if (_internalFlag)
			{
				toolStripButtonNew.Enabled = !((ToolStripSplitButton)sender).Enabled;
				_internalFlag = false;
				return;
			}
			toolStripButtonNew.Enabled = ((ToolStripSplitButton)sender).Enabled;
		}

		private void toolStripSplitButtonDelete_VisibleChanged(object sender, EventArgs e)
		{
			if (_internalFlag)
		    {
				toolStripButtonDelete.Visible = !((ToolStripSplitButton)sender).Visible;
			    _internalFlag = false;
			    return;
		    }
			toolStripButtonDelete.Visible = ((ToolStripSplitButton)sender).Visible;
		}

		private void toolStripSplitButtonDelete_EnabledChanged(object sender, EventArgs e)
		{
			if (_internalFlag)
		    {
				toolStripButtonDelete.Enabled = !((ToolStripSplitButton)sender).Enabled;
			    _internalFlag = false;
			    return;
		    }
			toolStripButtonDelete.Enabled = ((ToolStripSplitButton)sender).Enabled;
		}
    }

    public enum EditAction
    {
        Delete,
        New
    }
}
