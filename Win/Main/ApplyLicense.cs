using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{

    /// <summary>
    /// A dialog letting the user apply a license
    /// </summary>
    public partial class ApplyLicense : BaseDialogForm
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        #region constructors
        public ApplyLicense(string explanation, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            SetColors();

            //const int desiredTextWidth = 100;
            //if (explanation.Length < desiredTextWidth)
            //{
            //    int spaces = desiredTextWidth - explanation.Length;
            //    for (int i = 0; i < spaces; i++)
            //    {
            //        explanation += " ";
            //    }

            //}
            labelExplanation.Text = explanation;
            textBoxIntructions.Text = Resources.ApplyLicenseInstructions;
        }

        /// <summary>
        /// Sets the colors.
        /// </summary>
        private void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
        }

        #endregion

        /// <summary>
        /// invoke the filter event of the FilterBoxAdvanced without closing the form
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvApply_Click(object sender, EventArgs eventArgs)
        {
            if (File.Exists(textBoxExtLicenseFilePath.Text))
            {
                string licenseFilePath = textBoxExtLicenseFilePath.Text;
                string licenseFileName = Path.GetFileName(licenseFilePath);

                try
                {
                    var licenseRepository = new LicenseRepository(_unitOfWorkFactory);
                    var personRepository = new PersonRepository(_unitOfWorkFactory);

					XDocument signedXml = XDocument.Load(licenseFilePath);
					
					//Default
					var agreementResource = AgreementProvider.DefaultAgreement;
					if (signedXml.Root != null)
					{
						XElement element = signedXml.Root.Element("Agreement");
						if (element != null)
						{
							agreementResource = element.Value;
						}
					}
					var provider = new AgreementProvider();
					var agreementText = provider.GetFromResources(agreementResource);
                	using (var agree = new LicenseAgreementForm(agreementText))
                	{
                		if(agree.ShowDialog(this) != DialogResult.OK)
							return;
                	}
					
                    using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        new XmlLicensePersister().SaveNewLicense(licenseFilePath, unitOfWork, licenseRepository, XmlLicenseService.GetXmlPublicKey(), personRepository);
                    }
                    
                    var licenseStatusUpdater =
                        new LicenseStatusUpdater(new LicenseStatusRepositories(_unitOfWorkFactory,new RepositoryFactory()));
                    licenseStatusUpdater.RunCheck();

                    System.Windows.MessageBox.Show(
                        Resources.LicenseApplicationSuccess,
                        Resources.Success,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information,
                        System.Windows.MessageBoxResult.OK,
                        (RightToLeft == RightToLeft.Yes)
                            ? System.Windows.MessageBoxOptions.RtlReading | System.Windows.MessageBoxOptions.RightAlign
                            : 0);

                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (IOException e)
                {
                    labelExplanation.Text = String.Format(CultureInfo.CurrentCulture,
                                                         Resources.CannotReadLicenseFile,
                                                         licenseFileName, e);
                }
                catch (LicenseExpiredException)
                {
                    labelExplanation.Text = String.Format(CultureInfo.CurrentCulture,
                                                          Resources.NewLicenseAlreadyExpired,
                                                          licenseFileName);
                }
                catch (SignatureValidationException)
                {
                    labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, Resources.NewLicenseCorrupted,
                                                          licenseFileName);
                }
                catch (TooManyActiveAgentsException e)
                {
					if(e.LicenseType.Equals(LicenseType.Seat))
						labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, Resources.NewLicenseDoesNotCoverEnoughSeats,
                                                          e.NumberOfLicensed);
					else
					{
						labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, Resources.NewLicenseDoesNotCoverEnoughActiveAgents,
														  e.NumberOfLicensed, e.NumberOfAttemptedActiveAgents);
					}
                }
                catch (System.Xml.XmlException e)
                {
                    labelExplanation.Text = String.Format(CultureInfo.CurrentCulture,
                                                          Resources.NewLicenseNotValidFile,
                                                          licenseFileName, e.Message);
                }
                catch (DataSourceException e)
                {
                    labelExplanation.Text = String.Format(CultureInfo.CurrentCulture, Resources.DatabaseProblem,
                                                          e.Message);
                }
                labelExplanation.ForeColor = ColorHelper.WarningButtonColor;
            }

        }

        /// <summary>
        /// Handles the Click event of the buttonAdvCancel control.
        /// closes the form without activating the filter
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxExtLicenseFilePath.Text.Length > 0)
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(textBoxExtLicenseFilePath.Text);
            }
            catch (ArgumentException)
            {
            }
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxExtLicenseFilePath.Text = openFileDialog.FileName;
            }
        }
    }
}