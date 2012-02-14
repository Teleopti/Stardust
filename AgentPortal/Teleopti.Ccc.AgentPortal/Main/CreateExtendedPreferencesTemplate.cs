using System;
using System.Drawing;
using Teleopti.Ccc.AgentPortal.Common;

namespace Teleopti.Ccc.AgentPortal.Main
{
    public partial class CreateExtendedPreferencesTemplate : BaseRibbonForm
    {
        private readonly string nameEmptyText = "";

        public CreateExtendedPreferencesTemplate()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public string InputName
        {
            get { return textBoxTemplateName.Text; }
            set { textBoxTemplateName.Text = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            textBoxTemplateName.Focus();
        }

        private bool NameIsValid()
        {
            if (String.IsNullOrEmpty(textBoxTemplateName.Text.Trim()))
            {
                textBoxTemplateName.Text = nameEmptyText;
                textBoxTemplateName.SelectAll();
            }
            if (textBoxTemplateName.Text == nameEmptyText) return false;
            return true;
        }

        private void textBoxTemplateName_TextChanged(object sender, EventArgs e)
        {
            if (NameIsValid())
            {
                buttonAdvOK.Enabled = true;
                textBoxTemplateName.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
            }
            else
            {
                buttonAdvOK.Enabled = false;
                if (textBoxTemplateName.Text != nameEmptyText)
                {
                    textBoxTemplateName.ForeColor = Color.Red;
                }
            }
        }
    }
}
