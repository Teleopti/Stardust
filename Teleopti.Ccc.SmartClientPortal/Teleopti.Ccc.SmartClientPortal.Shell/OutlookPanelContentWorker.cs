using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Matrix;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PerformanceManager;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts;

namespace Teleopti.Ccc.SmartClientPortal.Shell
{
	/// <summary>
	/// This will create content user controls for outlook panel control
	/// </summary>
	/// <author>Dinesh Ranasinghe</author>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Don't want this to be public. Instantiated through IoC-container.")]
	class OutlookPanelContentWorker : IDisposable
	{
		private bool _disposed;
		private readonly NavigationPanelProvider _navigationPanelProvider;

		public OutlookPanelContentWorker(NavigationPanelProvider navigationPanelProvider)
		{
			_navigationPanelProvider = navigationPanelProvider;
		}

		/// <summary>
		/// This method will return usercontrol for outlook panel control
		/// </summary>
		/// <returns>user control</returns>
		public UserControl GetOutlookPanelContent(string contentType)
		{
			UserControl uc = getParticularUserControl(contentType);
			if (uc != null) { uc.Dock = DockStyle.Fill; }
			return uc;
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-18
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-18
		/// </remarks>
		public void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				}
			}
			_disposed = true;
		}

		#endregion

		/// <summary>
		/// Gets the particular user control.
		/// </summary>
		/// <param name="contentType">Type of the content.</param>
		/// <param name="toggleManager"></param>
		/// <returns>User Control</returns>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-01-18
		/// </remarks>
		private UserControl getParticularUserControl(string contentType)
		{

			switch (contentType)
			{
				case DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage:
					{ return _navigationPanelProvider.CreateNavigationPanel<PeopleNavigator>(); }
				case DefinedRaptorApplicationFunctionPaths.OpenForecasterPage:
					{ return _navigationPanelProvider.CreateNavigationPanel<ForecasterNavigator>(); }
				case DefinedRaptorApplicationFunctionPaths.OpenSchedulePage:
					{ return _navigationPanelProvider.CreateNavigationPanel<SchedulerNavigator>(); }
				case DefinedRaptorApplicationFunctionPaths.AccessToReports:
					{ return _navigationPanelProvider.CreateNavigationPanel<MatrixNavigationView>(); }
				case DefinedRaptorApplicationFunctionPaths.Shifts:
					{ return _navigationPanelProvider.CreateNavigationPanel<ShiftsNavigationPanel>(); }
				case DefinedRaptorApplicationFunctionPaths.OpenIntradayPage:
					{
						return _navigationPanelProvider.CreateNavigationPanel<IntradayWebNavigator>();
					}
				case DefinedRaptorApplicationFunctionPaths.OpenBudgets:
					{
						return _navigationPanelProvider.CreateNavigationPanel<BudgetGroupGroupNavigatorView>();
					}
				case DefinedRaptorApplicationFunctionPaths.AccessToPerformanceManager:
					{
						return _navigationPanelProvider.CreateNavigationPanel<PerformanceManagerNavigator>();
					}
				case DefinedRaptorApplicationFunctionPaths.PayrollIntegration:
					{
						return _navigationPanelProvider.CreateNavigationPanel<PayrollExportNavigator>();
					}
			}
			return null;
		}
	}
}
