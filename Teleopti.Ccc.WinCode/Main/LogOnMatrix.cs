using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonMatrix
	{
		public static void SynchronizeAndLoadMatrixReports(Form owner)
		{
			using (PerformanceOutput.ForOperation("Synchronize matrix reports"))
			{
				try
				{
					using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
						MatrixSync.SynchronizeReports(uow, new RepositoryFactory());

				}
				catch (Exception ex)
				{
					owner.ShowInTaskbar = true;
					MessageBox.Show(
						owner,
						String.Format(CultureInfo.InvariantCulture, string.Concat(UserTexts.Resources.OperationCanNotProceedWithTheExternalMatrixSystem, "  "), ex.Message),
						UserTexts.Resources.ExternalSystemError,
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button1,
						0);
					owner.ShowInTaskbar = false;
					((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.ResetStatistic(); //Makes the factory return empty repository
				}
			}
		}
	}
}
