using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common
{

    public partial class BaseDialogForm : Office2007Form, ILocalized, IHelpForm
    {
        private readonly IList<ControlHelpContext> _manualHelpContextList = new List<ControlHelpContext>();

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
                if (e.KeyCode == Keys.F1 && e.Modifiers == Keys.Shift)
                    ShowHelp(true);//Offline
                else
                    ShowHelp(false);
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
                ControlHelpContext helpContext = _manualHelpContextList.FirstOrDefault(c => c.Control.Name.Equals(control.Name));
                _manualHelpContextList.Remove(helpContext);
            }
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
            ShowHelp(true);
            base.OnHelpButtonClicked(e);
        }

        public void ShowHelp(bool local)
        {
            ColorHelper guiHelper = new ColorHelper();
            var activeControl = guiHelper.GetActiveControl(this);
            IHelpContext userControl = null;
            while (activeControl != null)
            {
                userControl = activeControl as IHelpContext;
                if (userControl != null && userControl.HasHelp) break;
                userControl = _manualHelpContextList.FirstOrDefault(c => c.Control.Equals(activeControl));
                if (userControl != null && userControl.HasHelp) break;
                activeControl = activeControl.Parent;
            }

            HelpHelper.Current.GetHelp(this, userControl, local);
        }

        public virtual string HelpId
        {
            get { return Name; }
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
