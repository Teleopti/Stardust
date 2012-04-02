using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.ViewModel;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.View
{
	/// <summary>
	/// Interaction logic for JobHistoryView.xaml
	/// </summary>
	public partial class JobHistoryView : UserControl
	{
		public JobHistoryView()
		{
			InitializeComponent();
		}

		public void LoadData(DateTime startDate, DateTime endDate, Guid businessUnitId)
		{
			DataContext = JobHistoryMapper.Map(startDate, endDate, businessUnitId);
		}

		private void menuItemCopyError_Click(object sender, RoutedEventArgs e)
		{
			var model = tlv.SelectedItem as IJobHistory;

			if (model == null)
				return;

			var msg = new StringBuilder();
			msg.AppendLine("EXCEPTION MESSAGE");
			msg.AppendLine("===========================");
			msg.AppendLine(model.ErrorMessage);
			msg.AppendLine("");
			msg.AppendLine("EXCEPTION STACKTRACE");
			msg.AppendLine("===========================");
			msg.AppendLine(model.ErrorStackTrace);
			msg.AppendLine("");
			msg.AppendLine("INNER EXCEPTION MESSAGE");
			msg.AppendLine("===========================");
			msg.AppendLine(model.InnerErrorMessage);
			msg.AppendLine("");
			msg.AppendLine("INNER EXCEPTION STACKTRACE");
			msg.AppendLine("===========================");
			msg.AppendLine(model.InnerErrorStackTrace);

			Clipboard.SetData(DataFormats.Text, msg.ToString());
		}
	}
}
