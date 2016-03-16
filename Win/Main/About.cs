using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
    public partial class About : BaseDialogForm
    {
        public About()
        {
            InitializeComponent();
            if (DesignMode) return;

            SetTexts();
            setColors();
			var authorization = PrincipalAuthorization.Instance();
			buttonAdvViewActive.Visible =
				authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewActiveAgents);
            labelActiveAgentsInUse.Text = getNumberOfActiveAgents();
            textBoxAbout.Text = getLicenseText();
        }

        private static string getNumberOfActiveAgents()
        {
            int numberOfActiveAgents;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var rep = new PersonRepository(new ThisUnitOfWork(uow));
                numberOfActiveAgents = rep.NumberOfActiveAgents();
            }
            return numberOfActiveAgents.ToString(CultureInfo.CurrentCulture);
        }

        private string getLicenseText()
        {
            ILicenseActivator license = DefinedLicenseDataFactory.GetLicenseActivator(UnitOfWorkFactory.Current.Name);

            var options = new StringBuilder();
            foreach(string option in license.EnabledLicenseOptionPaths)
            {
                options.AppendLine(option);
            }
        	var licenseResource = Resources.ProductActivationKey;
        	var max = license.MaxActiveAgents;

			if(license.LicenseType == LicenseType.Seat)
			{
				licenseResource = Resources.SeatProductActivationKey;
				max = license.MaxSeats;
				labelActiveAgentsInUse.Text = "";
				labelActiveAgentsOrSeats.Text = "";
				buttonAdvViewActive.Visible = false;
			}

            string licenseText = String.Format(CultureInfo.CurrentCulture,
                licenseResource,
                license.CustomerName,
                license.ExpirationDate,
                max,
                license.EnabledLicenseSchemaName,
                options);

            return licenseText;
        }

        private void buttonAdvOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonAdvApplyProductActivationKeyClick(object sender, EventArgs e)
        {
            using (var applyProductActivationKey = new ApplyProductActivationKey("", UnitOfWorkFactory.Current))
            {
                if (applyProductActivationKey.ShowDialog(this) == DialogResult.OK)
                {
                    ProductActivationKeyWasApplied = true;
                }
                Close();
            }
        }

        public bool ProductActivationKeyWasApplied { get; private set; }

        private void aboutLoad(object sender, EventArgs e)
        {
            setVersionInfo();
        }

        //I have tried to make this test-driven, but with bad results, guess it's ok to have it here
        private void setVersionInfo()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            var product = (AssemblyProductAttribute) assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0];
            labelProductName.Text = product.Product;

            var version = (AssemblyFileVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true)[0];
            labelProductVersion.Text = version.Version;

            var copyright = (AssemblyCopyrightAttribute) assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), true)[0];
            labelCopyright.Text = copyright.Copyright;
        }

        private void setColors()
        {
            BackColor = ColorHelper.FormBackgroundColor();
            gradientPanel1.BackgroundColor = ColorHelper.ChartControlBackInterior();
            gradientPanel2.BackgroundColor = ColorHelper.ChartControlBackInterior();
            textBoxAbout.BackColor = ColorHelper.DialogBackColor();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void buttonAdvLegalNoticeClick(object sender, EventArgs e)
        {
            var legalNotice = new Legal();
            legalNotice.ShowDialog(this);
			legalNotice.Dispose();
		}

		private void buttonAdvViewActive_Click(object sender, EventArgs e)
		{
			var rep = new LicenseRepository(new FromFactory(() => UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current()));
			var arr = rep.GetActiveAgents();
			
			var strings = new string[arr.Count+1];
			strings[0] = string.Join(",", Resources.BusinessUnit, Resources.FirstName, Resources.LastName, Resources.Email,
									Resources.EmployeeNumber, Resources.Start, Resources.TerminalDate);
			for (var i = 0; i < arr.Count; i++)
			{
				var a = arr[i];
				strings[i+1] = string.Join(",", a.BusinessUnit, a.FirstName, a.LastName, a.Email, a.EmploymentNumber, a.StartDate,
										a.LeavingDate);
			}
			using (var agent = new ActiveAgents(strings))
			{
				agent.ShowDialog(this);
			}
        }

    }
}