using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortal.AgentPreferenceView
{
    /// <summary>
    /// there is 6 events associated with this control,
    /// one per main button and one per dropdown
    /// 
    /// note this control needs a width of 95 pxls in ribbon to show correctly
    /// </summary>
    /// <remarks>
    /// Created by: ostenpe
    /// Created date: 2008-08-17
    /// </remarks>
    public partial class ClipboardControl : BaseUserControl
    {

        private IList<ToolStripItem> _pasteSpecialItems;
        private const int textMaxLength = 11;

        public ToolStripSplitButton ToolStripSplitButtonCopy
        {
            get { return toolStripSplitButtonCopy; }
        }
        public ToolStripSplitButton ToolStripSplitButtonCut
        {
            get { return toolStripSplitButtonCut; }
        }
        public ToolStripSplitButton ToolStripSplitButtonPaste
        {
            get { return toolStripSplitButtonPaste; }
        }

        private IList<ToolStripItem> _copySpecialItems;

        private IList<ToolStripItem> _cutSpecialItems;

        #region public events
        /// <summary>
        /// fires when the main copybutton is clicked.
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<EventArgs> CopyClicked;

        /// <summary>
        /// fires when the main Paste button is clicked.
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<EventArgs> PasteClicked;

        /// <summary>
        /// fires when the main Cut button is clicked.
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<EventArgs> CutClicked;
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="ClipboardControl"/> class.
        /// and the lists
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public ClipboardControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();

            _cutSpecialItems = new List<ToolStripItem>();
            _copySpecialItems = new List<ToolStripItem>();
            _pasteSpecialItems = new List<ToolStripItem>();

            Load += Clipboard_Load;
            toolStripSplitButtonCopy.ButtonClick += toolStripSplitButtonCopy_Click;
            toolStripSplitButtonCut.ButtonClick += toolStripSplitButtonCut_Click;
            toolStripSplitButtonPaste.ButtonClick += toolStripSplitButtonPaste_Click;

        }
        #region statehandling
 
        /// <summary>
        /// Sets the enabled state of the specified button.
        /// </summary>
        /// <param name="thisButton">The this button.</param>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-20
        /// </remarks>
        public void SetButtonState(ClipboardAction thisButton, bool enabled)
        {
            switch (thisButton)
            {
                case ClipboardAction.Copy:
                    toolStripSplitButtonCopy.Enabled = enabled;
                    break;
                case ClipboardAction.Cut:
                    toolStripSplitButtonCut.Enabled = enabled;
                    break;
                case ClipboardAction.Paste:
                    toolStripSplitButtonPaste.Enabled = enabled;
                    break;
            }
            Refresh();
        }

        #endregion

        #region internal events
        void toolStripSplitButtonPaste_Click(object sender, EventArgs e)
        {
            if(PasteClicked != null)PasteClicked.Invoke(this,e);
        }

        void toolStripSplitButtonCut_Click(object sender, EventArgs e)
        {
            if(CutClicked != null) CutClicked.Invoke(this,e);
        }

        void toolStripSplitButtonCopy_Click(object sender, EventArgs e)
        {
            if (CopyClicked != null) CopyClicked.Invoke(this, e);
        }
        #endregion

        /// <summary>
        /// Handles the Load event of the Clipboard control.
        /// and sets up the dropdownbuttons
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        void Clipboard_Load(object sender, EventArgs e)
        {
            PasteDropDownHandler();
            CutDropDownHandler();
            CopyDropDownHandler();
        }


        #region CopySpecial area
        /// <summary>
        /// handles the copyspecial part of the copy button
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        private void CopyDropDownHandler()
        {
            if (_copySpecialItems.Count == 0 )
            {
                toolStripSplitButtonCopy.DropDownButtonWidth = 0;
                return;
            }
            toolStripSplitButtonCopy.DropDownButtonWidth = 15;
            var drop = new ToolStripDropDown();
            foreach (var item in _copySpecialItems)
            {
                drop.Items.Add(item);
            }
          
            toolStripSplitButtonCopy.DropDown = drop;
            toolStripSplitButtonCopy.DropDown.ItemClicked += toolStripSplitButtonCopyDropDown_ItemClicked;
          
        }

        void toolStripSplitButtonCopyDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        	var handler = CopySpecialClicked;
            if(handler!=null)
            {
                handler.Invoke(sender,e);
            }
        }

        /// <summary>
        /// Occurs when [copy special dropdown item is clicked]. 
        /// the sender is the dropdown and the event is the dropdownevent
        /// bubbled.  (ToolStripItemClickedEventArgs)
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<ToolStripItemClickedEventArgs> CopySpecialClicked;
        #endregion

        #region CutSpecial Area
        /// <summary>
        /// handles he cut special dropdown part of the cutbutton
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        private void CutDropDownHandler()
        {
            if (_cutSpecialItems.Count == 0 )
            {
                toolStripSplitButtonCut.DropDownButtonWidth = 0;
                return;
            }
            toolStripSplitButtonCut.DropDownButtonWidth = 15;

            var drop = new ToolStripDropDown();
            foreach (var item in _cutSpecialItems)
            {
                drop.Items.Add(item);
            }
          
            toolStripSplitButtonCut.DropDown = drop;
            toolStripSplitButtonCut.DropDown.ItemClicked += toolStripSplitButtonCutDropDown_ItemClicked;
        }
        void toolStripSplitButtonCutDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if(CutSpecialClicked != null)
            {
                CutSpecialClicked.Invoke(sender , e);
            }
        }
        /// <summary>
        /// Occurs when [cut special dropdownitem is clicked].
        /// tthe sender is the dropdown and the event is the dropdownevent bubbled  (ToolStripItemClickedEventArgs)
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<ToolStripItemClickedEventArgs> CutSpecialClicked;
        #endregion

        #region PasteSpecial Area
        /// <summary>
        /// handles the dropdown part of the paste button
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        private void PasteDropDownHandler()
        {
            if (_pasteSpecialItems.Count == 0 )
            {
                toolStripSplitButtonPaste.DropDownButtonWidth = 0;
                return;
            }
            toolStripSplitButtonPaste.DropDownButtonWidth = 15;
            var drop = new ToolStripDropDown();
            foreach (var item in _pasteSpecialItems)
            {
                drop.Items.Add(item);
            }
          
            toolStripSplitButtonPaste.DropDown = drop;
            toolStripSplitButtonPaste.DropDown.ItemClicked += PasteSpecialDropDown_ItemClicked;
         
        }
        void PasteSpecialDropDown_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (PasteSpecialClicked != null)
            {
                PasteSpecialClicked.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Occurs when [paste special dropdownitem is clicked].
        /// th sender is the dropdown and the event is the dropdownevent bubbled (ToolStripItemClickedEventArgs)
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-08-17
        /// </remarks>
        public event EventHandler<ToolStripItemClickedEventArgs> PasteSpecialClicked;
        #endregion


        /// <summary>
        /// Try to ensure icon is always visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripSplitButtonPaste_TextChanged(object sender, EventArgs e)
        {
            //TODO refactor to use MeasureString
            ToolStripDropDownItem item = ((ToolStripDropDownItem)sender);

            if (item.Text.Length > textMaxLength)
                item.Text = item.Text.Substring(0, textMaxLength - 1);

        }

        /// <summary>
        /// Try to ensure icon is always visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripSplitButtonCopy_TextChanged(object sender, EventArgs e)
        {
            //TODO refactor to use MeasureString
            ToolStripDropDownItem item = ((ToolStripDropDownItem)sender);

            if (item.Text.Length > textMaxLength)
                item.Text = item.Text.Substring(0, textMaxLength - 1);

        }

        /// <summary>
        /// Try to ensure icon is always visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripSplitButtonCut_TextChanged(object sender, EventArgs e)
        {
            //TODO refactor to use MeasureString
            ToolStripDropDownItem item = ((ToolStripDropDownItem)sender);

            if (item.Text.Length > textMaxLength)
                item.Text = item.Text.Substring(0, textMaxLength - 1);
        }
    }
}