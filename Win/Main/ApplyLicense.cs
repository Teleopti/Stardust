using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Licensing.Agreements;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Secrets.Licensing;
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

        public ApplyLicense(string explanation, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
            SetColors();

            labelExplanation.Text = explanation;
            textBoxIntructions.Text = Resources.ApplyLicenseInstructions;
        }

        private void SetColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
        }

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
					
                    using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        new XmlLicensePersister().SaveNewLicense(licenseFilePath, _unitOfWorkFactory, licenseRepository, new XmlLicensePublicKeyReader().GetXmlPublicKey(), personRepository);
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