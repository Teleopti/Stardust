using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Common;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Base Ribbon form for use in Raptor project.
    /// </summary>
    /// <remarks>
    /// Contains logic to perform translation.
    /// copied from the baseform - so potential problem here
    /// Created by: östenp
    /// Created date: 2007-01-15
    /// </remarks>
    [CLSCompliant(true)]
    public class BaseRibbonForm : RibbonForm, ILocalized, IViewBase, IHelpForm
    {
        private readonly IList<ControlHelpContext> _manualHelpContextList = new List<ControlHelpContext>();
        private bool _killMode;

        public BaseRibbonForm()
        {
            initializeComponent();
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Automatically checking for F1 key before raising the event
        /// Created by: micke
        /// Created date: 2008-05-18
        /// </remarks>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
	            var offlineMode = e.Modifiers == Keys.Shift;
                
				ViewBase.ShowHelp(this,offlineMode);
            }
            base.OnKeyDown(e);
        }

        public void AddControlHelpContext(Control control)
        {
            if (!_manualHelpContextList.Any(c => c.Control.Equals(control)))
                _manualHelpContextList.Add(new ControlHelpContext(control));
        }

        public void RemoveControlHelpContext(Control control)
        {
            if (_manualHelpContextList.Any(c => c.Control.Name.Equals(control.Name)))
            {
                var helpContext = _manualHelpContextList.FirstOrDefault(c => c.Control.Name.Equals(control.Name));
                _manualHelpContextList.Remove(helpContext);
            }
        }

        public void ShowErrorMessage(string text, string caption)
        {
            ViewBase.ShowErrorMessage(text, caption);
        }

        public DialogResult ShowConfirmationMessage(string text, string caption)
        {
            return ViewBase.ShowConfirmationMessage(this, text, caption);
        }

        public DialogResult ShowYesNoMessage(string text, string caption)
        {
            return ViewBase.ShowYesNoMessage(this, text, caption);
        }

		public DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton)
		{
			return ViewBase.ShowYesNoMessage(this, text, caption, defaultButton);
		}

        public void ShowInformationMessage(string text, string caption)
        {
            ViewBase.ShowInformationMessage(this, text, caption);
        }

        public DialogResult ShowOkCancelMessage(string text, string caption)
        {
            return ViewBase.ShowOkCancelMessage(text, caption);
        }

        public DialogResult ShowWarningMessage(string text, string caption)
        {
            return ViewBase.ShowWarningMessage(text, caption);
        }

        protected override void OnHelpButtonClicked(System.ComponentModel.CancelEventArgs e)
        {
	        var showLocalHelp = Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Shift;
			ViewBase.ShowHelp(this,showLocalHelp);
            base.OnHelpButtonClicked(e);
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!KillMode)
                base.OnFormClosing(e);
        }

        private void initializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseRibbonForm));
            SuspendLayout();
            // 
            // BaseRibbonForm
            // 
            ClientSize = new System.Drawing.Size(300, 300);
            HelpButton = true;
            HelpButtonImage = ((System.Drawing.Image)(resources.GetObject("$this.HelpButtonImage")));
            Name = "BaseRibbonForm";
            ResumeLayout(false);
        }

        public virtual string HelpId
        {
            get { return Name; }
        }

	    public IHelpContext FindMatchingManualHelpContext(Control control)
	    {
		    return _manualHelpContextList.FirstOrDefault(c => c.Control.Equals(control));
	    }

	    public bool KillMode
        {
            get { return _killMode; }
        }

        /// <summary>
        /// Sets the killMode to true.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-10-07
        /// </remarks>
        public virtual void FormKill()
        {
            _killMode = true;
            Close();

        }

        public void AutoLocate()
        {
            var screenRect = Screen.GetWorkingArea(this);
            var location = Location;

            if (location.Y < screenRect.Y)
                location.Y = screenRect.Y;
            if (location.Y + Size.Height - screenRect.Y > screenRect.Height)
                location.Y = screenRect.Height - Size.Height + screenRect.Y;

            if (location.X < screenRect.X)
                location.X = screenRect.X;
            if (location.X + Size.Width - screenRect.X > screenRect.Width)
                location.X = screenRect.Width - Size.Width + screenRect.X;

            Location = location;
        }

        protected static bool CloseAllOtherForms(BaseRibbonForm caller)
        {
            var formCollection = Application.OpenForms;
            IList<Form> forms = new List<Form>();
            for (int i = 0; i < formCollection.Count; i++)
            {
                forms.Add(formCollection[i]);
            }

            foreach (var form in forms.OfType<BaseRibbonForm>())
            {
                if (form == caller)
                    continue;
                //SmartClientShellForm 
				if (form is IDummyInterface)
                    continue;

                int formCount = Application.OpenForms.Count;
                form.Close();
                if (formCount == Application.OpenForms.Count)
                {
                     return false;
                }
            }
            return true;
        }
    }
}
