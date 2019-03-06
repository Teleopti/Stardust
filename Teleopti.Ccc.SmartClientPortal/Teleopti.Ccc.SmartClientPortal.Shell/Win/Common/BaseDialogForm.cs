using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public partial class BaseDialogForm : MetroForm, ILocalized, IHelpForm
    {
		private IList<ControlHelpContext> _manualHelpContextList = new List<ControlHelpContext>();

        public BaseDialogForm()
        {
            InitializeComponent();
            HelpButton = true;
            KeyPreview = true;
            
            base.ForeColor = Color.FromArgb(64,64,64);
        		
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

        public void SetTexts()
        {
            new LanguageResourceHelper().SetTexts(this);
        }

		[RemoveMeWithToggle(Toggles.ResourcePlanner_PrepareToRemoveRightToLeft_81112)]
		public void SetTextsNoRightToLeft()
		{
			new LanguageResourceHelper().SetTexts(this, false);
		}


		protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
	            bool offlineMode = e.Modifiers == Keys.Shift;

	            ViewBase.ShowHelp(this,offlineMode);
            }
            base.OnKeyDown(e);
        }

        public void ShowErrorMessage(string text, string caption)
        {
            ViewBase.ShowErrorMessage(text, caption);
        }

        public DialogResult ShowYesNoMessage(string text, string caption)
        {
            return ViewBase.ShowYesNoMessage(this, text, caption);
        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            ViewBase.ShowHelp(this,false);
            base.OnHelpButtonClicked(e);
        }

	    public virtual string HelpId
        {
            get { return Name; }
        }

	    public IHelpContext FindMatchingManualHelpContext(Control control)
	    {
		    return null;
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
    }
}
