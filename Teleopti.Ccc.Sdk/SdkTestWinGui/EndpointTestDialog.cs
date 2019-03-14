using SdkTestClientWin.Sdk;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SdkTestWinGui
{
	public partial class EndpointTestDialog : Form
	{
		public Form1 MainForm { get; set; }
		public EndpointTestDialog(Form1 owner)
		{
			InitializeComponent();
			MainForm = owner;
		}
		private void EndpointTestDialog_Load(object sender, EventArgs e)
		{
			gbExperimental.Enabled = false;
			var services = typeof(TeleoptiSchedulingService).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(System.Web.Services.Protocols.SoapHttpClientProtocol)));
			cbServices.DisplayMember = "Name";
			cbServices.Items.AddRange(services.ToArray());
			cbServices.SelectedIndex = 0;

			var serviceType = (Type)cbServices.SelectedItem;
			var names = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.IsSpecialName);
			cbEndpointName.Items.AddRange(names.Select(x => x.Name).OrderBy(o => o).ToArray());
			cbEndpointName.SelectedIndex = 0;

			var queryDtos = typeof(QueryDto).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(QueryDto)));
			cbDto.Items.AddRange(queryDtos.Select(x => x.Name).OrderBy(o => o).ToArray());
			cbDto.SelectedItem = "GetSchedulesByChangeDateQueryDto";
		}

		private void btnDoAction_Click(object sender, EventArgs e)
		{
			btnDoAction.Enabled = false;
			try
			{
				var qo = new GetSchedulesByChangeDateQueryDto
				{
					ChangesFromUTC = DateTime.Parse(tbChangesFrom.Text),
					ChangesToUTC = string.IsNullOrWhiteSpace(tbChangesTo.Text) ? DateTime.MinValue : DateTime.Parse(tbChangesTo.Text),
					ChangesToUTCSpecified = !string.IsNullOrWhiteSpace(tbChangesTo.Text),
					Page = int.Parse(tbPage.Text),
					PageSize = int.Parse(tbPageSize.Text),
					PageSizeSpecified = !string.IsNullOrWhiteSpace(tbPageSize.Text)
				};

				tbResponsOutput.AppendText($"# Query: {qo.ChangesFromUTC} -> {qo.ChangesToUTC}, Page: {qo.Page}, PageSize: {qo.PageSize}\n");

				var resp = MainForm.Service.SchedulingService.GetSchedulesByChangedDateTime(qo);
				PrintResponse(resp);
			}
			catch (Exception ex)
			{
				tbResponsOutput.AppendText($"Failed to send request:\n{ex.Message}\n");
			}
			btnDoAction.Enabled = true;
		}

		private void cbDto_SelectedIndexChanged(object sender, EventArgs e)
		{
			var asmName = typeof(QueryDto);
			var fullTypeName = $"SdkTestClientWin.Sdk.{cbDto.SelectedItem}, {typeof(QueryDto).Assembly.FullName}";

			var dtoType = Type.GetType(fullTypeName);
			pgQueryDto.SelectedObject = Activator.CreateInstance(dtoType);
		}

		private void btnExpCall_Click(object sender, EventArgs e)
		{
			tbResponsOutput.AppendText("\n\nONLY IMPLEMENTED FOR: GetSchedulesByChangedDateTime((GetSchedulesByChangeDateQueryDto\n");
			try
			{
				var zeObj = pgQueryDto.SelectedObject;
				var resp = MainForm.Service.SchedulingService.GetSchedulesByChangedDateTime((GetSchedulesByChangeDateQueryDto)zeObj);
				PrintResponse(resp);
			}
			catch (Exception ex)
			{
				tbResponsOutput.AppendText($"ERROR:\n{ex.Message}\n");
			}
		}

		private void cbServices_SelectedIndexChanged(object sender, EventArgs e)
		{
			var serviceType = (Type)cbServices.SelectedItem;
			var names = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.IsSpecialName);
			cbEndpointName.Items.Clear();
			cbEndpointName.Items.AddRange(names.Select(x => x.Name).OrderBy(o => o).ToArray());
			cbEndpointName.SelectedIndex = 0;
		}

		private void PrintResponse(ScheduleChangesDto resp)
		{
			tbResponsOutput.AppendText($"# Response:\n  - ChangesUpToUTC: {resp.ChangesUpToUTC}\n  - Page: {resp.Page}\n  - TotalPages: {resp.TotalPages}\n  - Total Schedules: {resp.TotalSchedules}\n  - Schedules in page: {resp.Schedules.Count()}\n");
		}
	}
}
