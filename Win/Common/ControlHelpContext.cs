﻿using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common
{
    public class ControlHelpContext : IHelpContext
    {
        private readonly Control _control;

        public ControlHelpContext(Control control)
        {
            _control = control;
        }

        public Control Control
        {
            get { return _control; }
        }

        #region IHelpContext Members

        public bool HasHelp
        {
            get { return true; }
        }

        public string HelpId
        {
            get { return _control.Name; }
        }

        #endregion
    }
}
