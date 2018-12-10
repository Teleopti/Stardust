using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;

using Size = System.Drawing.Size;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
{
    public partial class SetEfficiencyShrinkageForm : BaseDialogForm
    {
        internal event EventHandler<CustomEventArgs<CustomEfficiencyShrinkageUpdatedEventArgs>> Save;
        private readonly bool _included;
        private readonly string _name;
        private const int maxLength = 25;
        private string _helpId;
        private readonly Guid? _id;

        public SetEfficiencyShrinkageForm()
        {
            InitializeComponent();
            _helpId = Name;
            if(!DesignMode) SetTexts();
        }

        public SetEfficiencyShrinkageForm(ICustomEfficiencyShrinkage customEfficiencyShrinkage)
            : this()
        {
            InParameter.NotNull("customEfficiencyShrinkage", customEfficiencyShrinkage);
            _id = customEfficiencyShrinkage.Id;
            _name = customEfficiencyShrinkage.ShrinkageName;
            _included = customEfficiencyShrinkage.IncludedInAllowance;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = String.Format(CultureInfo.InvariantCulture, Text, UserTexts.Resources.CustomEfficiencyShrinkage.ToLower(CultureInfo.CurrentCulture));
            labelName.Text = String.Format(CultureInfo.InvariantCulture, labelName.Text, UserTexts.Resources.CustomEfficiencyShrinkage.ToLower(CultureInfo.CurrentCulture));
            textBoxExt1.Text = _name;
            textBoxExt1.MaxLength = maxLength;
            if (checkBoxInclude != null)
                checkBoxInclude.Checked = _included;
        }

        public void HideIncludedInRequestAllowance()
        {
            tableLayoutPanelFields.RowStyles.RemoveAt(1);
            tableLayoutPanelFields.Controls.Remove(checkBoxInclude);
            var height = tableLayoutPanelFields.GetRowHeights()[1];
            Size = new Size(Size.Width, Size.Height-height);
            
            checkBoxInclude = null;
        }

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			textBoxExt1.Focus();
			textBoxExt1.SelectAll();
		}

        private void buttonAdvSave_Click(object sender, EventArgs e)
        {
            var viewName = textBoxExt1.Text.Trim();
            if (string.IsNullOrEmpty(viewName))
            {
                errorProvider.SetError(textBoxExt1, UserTexts.Resources.TheInputTextCanNotBeEmptyDot);
                errorProvider.SetIconPadding(textBoxExt1, -20);
                return;
            }

        	var handler = Save;
            if (handler != null)
            {
                var eventArgs = new CustomEfficiencyShrinkageUpdatedEventArgs {Id = _id, ShrinkageName = viewName };

                if (checkBoxInclude != null)
                    eventArgs.IncludedInAllowance = checkBoxInclude.Checked;
                handler.Invoke(this, new CustomEventArgs<CustomEfficiencyShrinkageUpdatedEventArgs>(eventArgs));
            }
            Close();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public override string HelpId
        {
            get
            {
                return _helpId;
            }
        }

        public void SetHelpId(string helpId)
        {
            _helpId = helpId;
        }

        private void textBoxExt1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxExt1.Text.Trim()) &&
                !string.IsNullOrEmpty(errorProvider.GetError(textBoxExt1)))
                errorProvider.SetError(textBoxExt1, string.Empty);
        }
    }
}
