using System;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
			setColors();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
		}

		public void Populate(IAggregateRoot aggregateRoot)
		{
			var payrollExport = (IPayrollExport)aggregateRoot;
			treeViewAdvExportType.BeginUpdate();
			treeViewAdvExportType.Nodes.Clear();
			
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PayrollFormatRepository(new ThisUnitOfWork(uow));

				bool someTreeNodeIsOptioned = false;

				var payrollFormats = rep.LoadAll();
				foreach (var payrollFormat in payrollFormats)
				{
					var treeNodeAdv = new TreeNodeAdv(payrollFormat.Name);
					treeViewAdvExportType.Nodes.Add(treeNodeAdv);
					treeNodeAdv.Tag = payrollFormat;
					treeNodeAdv.Height = 30;
					if (payrollFormat.FormatId == payrollExport.PayrollFormatId)
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
			var payrollExport = (IPayrollExport)aggregateRoot;

			TreeNodeAdv treeNodeAdv = treeViewAdvExportType.Nodes.Cast<TreeNodeAdv>().FirstOrDefault(node => node.Optioned);

			if (treeNodeAdv == null)
			{
				return false;
			}

			var payrollFormat = treeNodeAdv.Tag as PayrollFormat;
			if (payrollFormat == null)
			{
				throw new InvalidOperationException("Node contains invalid or none Payroll Format instance.");
			}

			payrollExport.PayrollFormatId = payrollFormat.FormatId;
			payrollExport.PayrollFormatName = payrollFormat.Name;
			return true;
		}

		public void SetEditMode()
		{
		}

		public string PageName
		{
			get { return UserTexts.Resources.FileTypeSelection; }
		}
	}
}
