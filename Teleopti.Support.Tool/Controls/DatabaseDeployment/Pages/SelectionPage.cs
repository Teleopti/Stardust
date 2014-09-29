using System;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Support.Tool.Controls.General;

namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    public class SelectionPage : UserControl
	{
		public delegate void HasValidInputDelegate(bool isValid);
		public event HasValidInputDelegate hasValidInput;

		internal void triggerHasValidInput(bool isValid)
		{
            if (hasValidInput != null)
            {
                hasValidInput(isValid);
            }
		}

		public virtual void GetData() { }
		public virtual void SetData() { }

        public virtual bool ContentIsValid() 
        {
            return true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }
	}
}
