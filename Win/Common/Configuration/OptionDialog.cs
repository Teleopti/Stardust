using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Common.Configuration
{
    public partial class OptionDialog : BaseRibbonForm 
    {
        public OptionCore Core { get; private set; }

        protected OptionDialog()
        {
            InitializeComponent();
 
            if (DesignMode) return;
            SetColors();
            SetTexts();
        }

        public OptionDialog(OptionCore optionCore) : this()
        {
            Core = optionCore;
        }

        public void Page(Type pageType)
        {
            Core.MarkAsSelected(pageType,null);

            SetupPage();
        }

        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            Core.SetUnitOfWork(unitOfWork);
        }

        private void SetupPage()
        {
        	if (!Core.HasLastPage) return;
        	gradientPanel1.Controls.Clear();
        	var ctrl = (Control)Core.LastPage;
        	ctrl.Dock = DockStyle.Fill;
        	var width = ctrl.Width - gradientPanel1.ClientSize.Width;
        	var height = ctrl.Height - gradientPanel1.ClientSize.Height;
        	Width += width;
        	Height += height;
        	gradientPanel1.Controls.Add(ctrl);
        	ctrl.BringToFront();
        	ActiveControl = ctrl;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                SetOptionPageAsActiveControl();
            }
            base.OnKeyDown(e);
        }

        protected override void OnHelpButtonClicked(System.ComponentModel.CancelEventArgs e)
        {
            SetOptionPageAsActiveControl();
            base.OnHelpButtonClicked(e);
        }

        private void SetOptionPageAsActiveControl()
        {
            if (gradientPanel1.Controls.Count>0)
            {
                ActiveControl = gradientPanel1.Controls[0];
            }
        }

        private void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
            gradientPanelBottom.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
        }

        private void ButtonAdvOkClick(object sender, EventArgs e)
        {
            // Save changes.
            var canClose = true;
            try
            {
                Core.SaveChanges();
                foreach (var settingPage in Core.AllSelectedPages)
                {
                    var checkBeforeClosing = settingPage as ICheckBeforeClosing;
                    if (checkBeforeClosing!=null && !checkBeforeClosing.CanClose())
                    {
                        canClose = false;
                    }
                }
            }
            catch (ValidationException ex)
            {
                ShowWarningMessage(UserTexts.Resources.InvalidDataOnFollowingPage + ex.Message, UserTexts.Resources.ValidationError);
                DialogResult = DialogResult.None;
                return;
            }

            DialogResult = canClose ? DialogResult.OK : DialogResult.None;
        }
    }
}
