using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Main
{
    public static class LogOnMatrix
    {
        /// <summary>
        /// Carries out the operations with the outer matrix system.
        /// </summary>        
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void SynchronizeAndLoadMatrixReports(Form owner)
        {
            using(PerformanceOutput.ForOperation("Synchronize matrix reports"))
            {
                try
                {
                    using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                    {
                        MatrixSync.SynchronizeReports(uow, new RepositoryFactory());
                    }
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
                    ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).DataSource.ResetStatistic(); //Makes the factory return empty repository
                }                
            }
        }

    }
}
