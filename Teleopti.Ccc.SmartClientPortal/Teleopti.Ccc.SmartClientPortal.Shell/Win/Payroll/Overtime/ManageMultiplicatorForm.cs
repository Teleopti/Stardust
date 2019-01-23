using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.Overtime
{
    /// <summary>
    /// Facilitates adding new definition sets
    /// </summary>
    public partial class ManageMultiplicatorForm : BaseDialogForm
    {
    	public event EventHandler<MultiplicatorAddedEventArgs> MultiplicatorAdded;

		private string _nextItemNameText;

    	/// <summary>
        /// Gets the person's selected culture.
        /// </summary>
        private static CultureInfo CurrentCulture
        {
            get
            {
                return
                    TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
            }
        }

    	/// <summary>
        /// Initializes a new instance of the <see cref="ManageMultiplicatorForm"/> class.
        /// </summary>
        /// <param name="nextItemName">Name of the next item.</param>
        public ManageMultiplicatorForm(string nextItemName)
        {
            InitializeComponent();

            _nextItemNameText = nextItemName;

            LoadDefinitionTypes();
            SetDefaultTexts();
            SetTexts();
        }

        /// <summary>
        /// Loads the definition types.
        /// </summary>
        private void LoadDefinitionTypes()
        {
            IList<KeyValuePair<MultiplicatorType, string>> multiplicatorTypeCollection = LanguageResourceHelper.TranslateEnumToList<MultiplicatorType>();
            cmbType.DataSource = multiplicatorTypeCollection;
            cmbType.ValueMember = "Key";
            cmbType.DisplayMember = "Value";
        }

        /// <summary>
        /// Sets the default texts.
        /// </summary>
        private void SetDefaultTexts()
        {
            textBoxName.Text = _nextItemNameText;
            textBoxShortName.Text = string.Empty;
        	textBoxMultiplicatorValue.CurrentCulture = CurrentCulture;
            textBoxMultiplicatorValue.DoubleValue = 1d;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (ValiateControls())
            {
                MultiplicatorType multiplicatorType = (MultiplicatorType)cmbType.SelectedValue;
                IMultiplicator multiplicator = new Multiplicator(multiplicatorType);
                multiplicator.Description = new Description(textBoxName.Text, textBoxShortName.Text);
                multiplicator.DisplayColor = colorPickerButton1.SelectedColor;
                multiplicator.MultiplicatorValue = Double.Parse(textBoxMultiplicatorValue.Text, CurrentCulture);
                multiplicator.ExportCode = textBoxExportCode.Text;

            	var handler = MultiplicatorAdded;
                if (handler != null)
                {
                    handler.Invoke(this, new MultiplicatorAddedEventArgs(multiplicator));
                }

                Close();
            }
            else
            {
				var handler = MultiplicatorAdded;
                if (handler != null)
                {
                    handler.Invoke(this, new MultiplicatorAddedEventArgs(null));
                }
            }
        }

        /// <summary>
        /// Valiates the controls.
        /// </summary>
        /// <returns></returns>
        private bool ValiateControls()
        {
            bool returnVal = true;
            if (string.IsNullOrEmpty(textBoxName.Text))
            {
                ShowErrorMessage(UserTexts.Resources.DefinitionSetNameBlank, Text);
                returnVal = false;
            }

            if (!textBoxMultiplicatorValue.IsValid())
            {
                ShowErrorMessage(UserTexts.Resources.MultiplicatorValueInvalid, Text);
                returnVal = false;
            }
            return returnVal;

        }
    }
}
