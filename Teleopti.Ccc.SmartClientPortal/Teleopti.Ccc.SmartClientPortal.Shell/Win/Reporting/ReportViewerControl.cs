using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using Microsoft.Win32;

namespace Teleopti.Ccc.Win.Reporting
{
	//Presenter
	public partial class ReportViewerControl : UserControl
	{
		private delegate void LoadReportDelegate<T>(string reportName, ReportDataPackage<T> reportDataPackage);

		public ReportViewerControl()
		{
			InitializeComponent();
			reportViewer1.ShowBackButton = false;
			reportViewer1.Messages = new ReportViewerToolbarTexts(); //set to a class that return correct strings
		}

		public ReportViewer ReportViewerControl1
		{
			get { return reportViewer1; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void LoadReport<T>(string reportFile, ReportDataPackage<T> reportDataPackage)
		{
			if (InvokeRequired)
			{
				var paramsList = new object[] { reportFile, reportDataPackage.Data, reportDataPackage.Parameters };
				BeginInvoke(new LoadReportDelegate<T>(LoadReport), paramsList);
				return;
			}

			// Set RDLC file
			string reportPath = string.Format(CultureInfo.InvariantCulture, "{0}\\Reports\\{1}", Application.StartupPath, reportFile);
			reportViewer1.LocalReport.ReportPath = reportPath;

			reportViewer1.ShowPrintButton = true;
			reportViewer1.LocalReport.DataSources.Clear();

			foreach (KeyValuePair<string, IList<T>> keyValuePair in reportDataPackage.Data)
				reportViewer1.LocalReport.DataSources.Add(new ReportDataSource(keyValuePair.Key, keyValuePair.Value));

			IList<ReportParameter> reportParameters = new List<ReportParameter>
														  {
																new ReportParameter("culture", Thread.CurrentThread.CurrentCulture.IetfLanguageTag, false)
														  };

			ReportParameterInfoCollection repInfos = reportViewer1.LocalReport.GetParameters();
			foreach (ReportParameterInfo repInfo in repInfos)
			{
				string resourceKey = repInfo.Name;

				string resourceValue = UserTexts.Resources.ResourceManager.GetString(resourceKey);
				if (!string.IsNullOrEmpty(resourceValue))
					reportParameters.Add(new ReportParameter(repInfo.Name, resourceValue, false));
				foreach (IReportDataParameter param in reportDataPackage.Parameters)
				{
					if (param.Name.ToLower(CultureInfo.CurrentCulture) == repInfo.Name.ToLower(CultureInfo.CurrentCulture))
					{
						reportParameters.Add(new ReportParameter(repInfo.Name, param.Value, false));
					}

				}
			}

			reportViewer1.LocalReport.SetParameters(reportParameters);
			reportViewer1.RefreshReport();
		}

		private void ReleaseManagedResources()
		{
			var winRSviewerField = reportViewer1.GetType().GetField("winRSviewer",
																					  BindingFlags.NonPublic | BindingFlags.Instance);
			var winRSviewer = (Control)winRSviewerField.GetValue(reportViewer1);

			var reportToolbarField = reportViewer1.GetType().GetField("reportToolBar",
																					  BindingFlags.NonPublic | BindingFlags.Instance);
			var reportToolbar = (Control)reportToolbarField.GetValue(reportViewer1);

			FixToolBarTextBoxes(winRSviewer);
			FixToolBarTextBoxes(reportToolbar);
			reportViewer1.Dispose();
			reportViewer1 = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void FixToolBarTextBoxes(Control view)
		{
			foreach (Control ctrl in view.Controls)
			{
				FixToolBarTextBoxes(ctrl);

				ToolStrip toolStrip = ctrl as ToolStrip;
				if (toolStrip != null)
				{
					foreach (var toolStripItem in toolStrip.Items)
					{
						ToolStripControlHost toolStripControlHost = toolStripItem as ToolStripControlHost;
						if (toolStripControlHost != null)
						{
							RemoveUserPreferenceChanged(toolStripControlHost.Control);
						}
					}
				}
				RemoveUserPreferenceChanged(ctrl);
				DisposeContextMenuStrip(ctrl);
			}
		}

		private static void DisposeContextMenuStrip(Control ctrl)
		{
			if (ctrl.ContextMenuStrip != null)
			{
				RemoveUserPreferenceChanged(ctrl.ContextMenuStrip);
			}
		}

		private static void RemoveUserPreferenceChanged(object ctrl)
		{
			try
			{
				var preferenceChangedEventHandler = (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(typeof(UserPreferenceChangedEventHandler), ctrl, "UserPreferenceChanged");
				var numberOfTimes = preferenceChangedEventHandler.GetInvocationList().Length;

				for (int i = 0; i < numberOfTimes; i++)
				{
					SystemEvents.UserPreferenceChanged -= preferenceChangedEventHandler;
				}
			}
			catch (ArgumentException)
			{
			}

			try
			{
				var preferenceChangedEventHandler = (UserPreferenceChangedEventHandler)Delegate.CreateDelegate(typeof(UserPreferenceChangedEventHandler), ctrl, "OnUserPreferenceChanged");
				var numberOfTimes = preferenceChangedEventHandler.GetInvocationList().Length;

				for (int i = 0; i < numberOfTimes; i++)
				{
					SystemEvents.UserPreferenceChanged -= preferenceChangedEventHandler;
				}
			}
			catch (ArgumentException)
			{
			}
		}
	}
}
