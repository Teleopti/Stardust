using System;
using System.Reflection;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Common
{
    /// <summary>
    /// An about dialog
    /// </summary>
    public partial class About : BaseRibbonForm
    {
        #region constructors
        public About()
        {
            InitializeComponent();
            SetTexts();

            textBoxAbout.Text = GetLicenseText();
        }

        private static string GetLicenseText()
        {
            //ILicenseActivator license = DefinedLicenseDataFactory.LicenseActivator;

            //StringBuilder options = new StringBuilder();
            //foreach(string option in license.EnabledLicenseOptionPaths)
            //{
            //    options.AppendLine(option);
            //}

            //string licenseText = String.Format(CultureInfo.CurrentCulture,
            //    Resources.License,
            //    license.CustomerName,
            //    license.ExpirationDate,
            //    license.MaxActiveAgents,
            //    license.EnabledLicenseSchemaName,
            //    options);

            //return licenseText;
            return "";
        }

        #endregion

        /// <summary>
        /// Handles the OK event of the buttonAdvOk control.
        /// closes the form without activating the filter
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void About_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }

        //I have tried to make this test-driven, but with bad results, guess it's ok to have it here
        private void SetVersionInfo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            AssemblyProductAttribute product = (AssemblyProductAttribute) assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0];
            labelProductName.Text = product.Product;

            AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0];
            labelProductVersion.Text = version.Version;

            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute) assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), true)[0];
            labelCopyright.Text = copyright.Copyright;
        }
    }
}