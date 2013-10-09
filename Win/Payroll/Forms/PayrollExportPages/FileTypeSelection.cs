using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages
{
    public partial class FileTypeSelection : BaseUserControl, IPropertyPage
    {
        public FileTypeSelection()
        {
            InitializeComponent();
            SetColors();
        }

        private void SetColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;
            treeViewAdvExportType.BeginUpdate();
            treeViewAdvExportType.Nodes.Clear();

            SdkAuthentication sdkAuthentication = new SdkAuthentication();
            sdkAuthentication.SetSdkAuthenticationHeader();

            var sdkName = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["Sdk"];
            using (var proxy = new Proxy(sdkName))
            {
                bool someTreeNodeIsOptioned = false;

                ICollection<PayrollFormatDto> payrollFormatDtos = proxy.GetPayrollFormats();
                foreach (PayrollFormatDto payrollFormatDto in payrollFormatDtos)
                {
                    TreeNodeAdv treeNodeAdv = new TreeNodeAdv(payrollFormatDto.Name);
                    treeViewAdvExportType.Nodes.Add(treeNodeAdv);
                    treeNodeAdv.Tag = payrollFormatDto;
                    treeNodeAdv.Height = 30;
                    if (payrollFormatDto.FormatId == payrollExport.PayrollFormatId)
                    {
                        treeNodeAdv.Optioned = true;
                        someTreeNodeIsOptioned = true;
                    }
                }
                // Make sure atleast the first one is selected.
                if (treeViewAdvExportType.Nodes.Count > 0)
                {
                    if (!someTreeNodeIsOptioned)
                    {
                        treeViewAdvExportType.Nodes[0].Optioned = true;
                    }
                }
            }
            treeViewAdvExportType.EndUpdate();
        }

        public bool Depopulate(IAggregateRoot aggregateRoot)
        {
            IPayrollExport payrollExport = (IPayrollExport) aggregateRoot;


            TreeNodeAdv treeNodeAdv = null;
            foreach (TreeNodeAdv node in treeViewAdvExportType.Nodes)
            {
                if (node.Optioned)
                {
                    treeNodeAdv = node;
                    break;
                }
            }

            if (treeNodeAdv == null)
            {
                return false;
            }

            PayrollFormatDto payrollFormatDto = treeNodeAdv.Tag as PayrollFormatDto;
            if (payrollFormatDto == null)
            {
                throw new InvalidOperationException("Node contains invalid or none Payroll Format instance.");
            }

            payrollExport.PayrollFormatId = payrollFormatDto.FormatId;
            payrollExport.PayrollFormatName = payrollFormatDto.Name;
            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return UserTexts.Resources.FileTypeSelection ; }
        }
    }
}
