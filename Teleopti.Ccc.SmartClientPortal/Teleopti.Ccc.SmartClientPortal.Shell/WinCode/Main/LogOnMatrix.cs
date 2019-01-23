using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
    public interface ILogonMatrix
    {
        void SynchronizeAndLoadMatrixReports(IUnitOfWorkFactory unitOfWorkFactory);
    }
    public class LogonMatrix : ILogonMatrix
	{
        private readonly ILogonView _logonView;

        public LogonMatrix(ILogonView logonView)
        {
            _logonView = logonView;
        }

        public void SynchronizeAndLoadMatrixReports(IUnitOfWorkFactory unitOfWorkFactory)
		{
			using (PerformanceOutput.ForOperation("Synchronize matrix reports"))
			{
				try
				{
                    using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
						MatrixSync.SynchronizeReports(uow, new RepositoryFactory());
				}
				catch (Exception ex)
				{
                    //have warning on view ??
                    _logonView.Warning(String.Format(CultureInfo.InvariantCulture, string.Concat(Resources.OperationCanNotProceedWithTheExternalMatrixSystem, "  "), ex.Message), Resources.ExternalSystemError);
                    ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).DataSource.RemoveAnalytics(); //Makes the factory return empty repository
				}
			}
		}
	}
}
