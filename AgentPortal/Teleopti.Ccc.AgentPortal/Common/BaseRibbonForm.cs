﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.Common
{
    /// <summary>
    /// Base Ribbon form for use in Raptor project.
    /// </summary>
    /// <remarks>
    /// Contains logic to perform translation.
    /// copied from the baseform - so potential problem here
    /// Created by: östenp / Sumedah[copy to Agent portal ]
    /// Created date: 2007-01-15
    /// </remarks>
    [CLSCompliant(true)]
    public class BaseRibbonForm : RibbonForm, ILocalized
    {
        //help class
        //private readonly HelpHelper _raptorHelp = new HelpHelper();
        private readonly IList<Control> _manualHelpContextList = new List<Control>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRibbonForm"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-01-15
        /// </remarks>
        public BaseRibbonForm()
        {
            if (!DesignMode)
            KeyPreview = true;
        }


        #region ILocalized Members
        /// <summary>
        /// Sets the texts.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2007-12-15
        /// </remarks>
        public void SetTexts()
        {
            new LanguageResourceHelper().SetTexts(this);
            SetCommonTexts();
        }

        #endregion
        /// <summary>
        /// Sets the common texts.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2007-12-15
        /// </remarks>
        protected virtual void SetCommonTexts()
        {
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
             if (e.KeyCode == Keys.F1)
             {
                 bool local = true;
                GuiHelper guiHelper = new GuiHelper();
                Control activeControl = guiHelper.GetActiveControl(this);
                BaseUserControl userControl = null;
                while (activeControl != null)
                {
                    userControl = activeControl as BaseUserControl;
                    if ((userControl == null) || (!userControl.HasHelp))
                    {
                        foreach (Control control in _manualHelpContextList)
                        {
                            if (activeControl.Equals(control))
                            {
                                userControl = control as BaseUserControl;
                                break;
                            }
                        }
                    }

                    if (userControl != null && userControl.HasHelp) break;
                    activeControl = activeControl.Parent;
                }
                if (e.KeyCode == Keys.F1 && e.Modifiers == Keys.Shift)
                    local = false;//Online

                HelpHelper.GetHelp(this, userControl, local);
             }

            base.OnKeyDown(e);
          
        }

         public void AddControlHelpContext(Control control)
        {
             foreach (Control controlItem in _manualHelpContextList)
             {
                 if (controlItem.Name == control.Name)
                 {
                     return;
                 }

                 //if not exist add
                 _manualHelpContextList.Add(control);
             }
        }

        public void RemoveControlHelpContext(Control control)
        {
            foreach (Control controlItem in _manualHelpContextList)
            {
                if (controlItem.Name == control.Name)
                {
                    _manualHelpContextList.Remove(control);
                    break;
                }
            }
        }

        /// <summary>
        /// Setups the help context.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 10/23/2008
        /// </remarks>
        public void SetupHelpContext(Control control)
        {
            RemoveControlHelpContext(control);
            AddControlHelpContext(control);
            control.Focus();
        }

        public void ShowErrorMessage(string text, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(text, "  "), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                        MessageBoxDefaultButton.Button1,
                                                        (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }
    }
}
