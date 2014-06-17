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

        public EditControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();

            _deleteSpecialItems = new List<ToolStripItem>();
            _newSpecialItems = new List<ToolStripItem>();
            Load += new EventHandler(NewDeleteControl_Load);
            this.toolStripButtonNew.ButtonClick += new EventHandler(toolStripButtonNew_Click);
            this.toolStripButtonDelete.ButtonClick += new EventHandler(toolStripButtonDelete_Click);
        }

        public ToolStripSplitButton ToolStripButtonNew
        {
            get { return this.toolStripButtonNew; }   
        }

        public ToolStripPanelItem PanelItem
        {
            get { return this.toolStripPanelItem1; }
        }

        public void SetButtonState(EditAction thisButton, bool enabled)
        {
            switch (thisButton)
            {
                case EditAction.Delete:
                    this.toolStripButtonDelete.Enabled = enabled;
                    break;
                case EditAction.New:
                    this.toolStripButtonNew.Enabled = enabled;
                    break;
            }
        }

        public void SetSpecialItemState(EditAction thisDropDown, string thisTag, bool enabled)
        {
            switch (thisDropDown)
            {
                case EditAction.Delete:
                    SetEnabled(thisTag, enabled, this._deleteSpecialItems);
                    break;
                case EditAction.New:
                    SetEnabled(thisTag, enabled, this._newSpecialItems);
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
        void toolStripButtonNew_Click(object sender, EventArgs e)
        {
        	var handler = NewClicked;
            if(handler != null)
            {
            	handler.Invoke(this,e);
            }
        }
        void toolStripButtonDelete_Click(object sender, EventArgs e)
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
            if (_newSpecialItems.Count == 0)
            {
                toolStripButtonNew.DropDownButtonWidth = 1;
                return;
            }
            toolStripButtonNew.DropDownButtonWidth = 15;
            var drop = new ToolStripDropDown();
            foreach (var item in _newSpecialItems)
            {
                drop.Items.Add(item);
            }
            toolStripButtonNew.DropDown = drop;
            toolStripButtonNew.DropDown.ItemClicked += newDropDown_ItemClicked;
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
            if (_deleteSpecialItems.Count == 0)
            {
                this.toolStripButtonDelete.DropDownButtonWidth = 1;
                return;
            }
            this.toolStripButtonDelete.DropDownButtonWidth = 15;

            var drop = new ToolStripDropDown();
            foreach (var item in _deleteSpecialItems)
            {
                drop.Items.Add(item);
            }
            this.toolStripButtonDelete.DropDown = drop;
            this.toolStripButtonDelete.DropDown.ItemClicked += new ToolStripItemClickedEventHandler(DeleteDropDown_ItemClicked);
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
            toolStripButtonNew.DropDown.Close();
            toolStripButtonDelete.DropDown.Close();
        }
    }

    public enum EditAction
    {
        Delete,
        New
    }
}
