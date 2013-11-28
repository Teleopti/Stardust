using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common
{
    public partial class BaseDialogForm : Office2007Form, ILocalized, IHelpForm
    {
        public BaseDialogForm()
        {
            SetColorScheme(Office2007Theme.Blue);
            InitializeComponent();
            HelpButton = true;
            KeyPreview = true;
            
            base.ForeColor = Color.Navy;
            UseOffice2007SchemeBackColor = true;
        		
        }

        protected void SetColorScheme(Office2007Theme scheme)
        {
            Office2007Colors.ApplyManagedScheme(this, scheme);
        }

        public void SetTexts()
        {
            new LanguageResourceHelper().SetTexts(this);
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
            ViewBase.ShowHelp(this,true);
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
