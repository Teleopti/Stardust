using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace PBI30532LoadTest
{
	public partial class Form1 : Form
	{
		private LogOnHelper _logOnHelper;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			sendToServiceBus((int)numericUpDown1.Value);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_logOnHelper = new LogOnHelper("CCCAdmin", "1234567", "c:\\nhib");

			var bu = _logOnHelper.GetBusinessUnitCollection().Where(b => b.Id.Equals(new Guid("1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B"))).First();
			_logOnHelper.LogOn(bu);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			var processInfo = new ProcessStartInfo("c:\\data\\main\\Teleopti.Support.Tool\\SQLServerPerformance\\RML\\Start.bat",
				"\"1440\" \"1\" \"\" \"0\"")
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			};

			Process.Start(processInfo);
			Cursor.Current = Cursors.Default;
		}

		private void button5_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			var processInfo = new ProcessStartInfo("cmd.exe", "/c taskkill /IM cscript.exe /F");
			Process process = Process.Start(processInfo);
			Cursor.Current = Cursors.Default;
			process.Close();
		}

		//private void button2_Click(object sender, EventArgs e)
		//{
		//	Cursor.Current = Cursors.WaitCursor;
		//	var testDate = new DateTime(2013, 06, 15);
		//	var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
		//	dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.QueueStatistics);
		//	var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
		//	{
		//		Helper =
		//			new JobHelper(
		//				new RaptorRepository(
		//					"Data Source=.;Integrated Security=SSPI;Initial Catalog=main_Telia_TeleoptiAnalytics;Current Language=us_english",
		//					""), null, null, null)
		//	};
		//	var steps = new IntradayJobCollection(jobParameters);
		//	var bus = _logOnHelper.GetBusinessUnitCollection();
		//	var result = new List<IJobResult>();
		//	foreach (var businessUnit in bus)
		//	{
		//		label1.Text = "running " + businessUnit.Name;
		//		foreach (var step in steps)
		//		{
					
		//			label2.Text = "running " + step.Name;
		//			Application.DoEvents();
		//			step.Run(new List<IJobStep>(), businessUnit, result, bus.IndexOf(businessUnit).Equals(bus.Count - 1));
		//		}
		//	}
		//	Cursor.Current = Cursors.Default;
		//	label1.Text = "Intraday ready.";
		//}

		internal class StateManager : State
		{

			private ISessionData _sessData;

			public override ISessionData SessionScopeData
			{
				get { return _sessData; }
			}

			public override void ClearSession()
			{
				_sessData = null;
			}

			public override void SetSessionData(ISessionData sessionData)
			{
				_sessData = sessionData;
			}
		}

		private void button2_Click_1(object sender, EventArgs e)
		{
			sendToServiceBus(500);
		}

		private void sendToServiceBus(int numberOfPersons)
		{
			var sendToServiceBus = new ServiceBusSender();
			var eventPublisher = new ServiceBusEventPublisher(sendToServiceBus,
				new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())));

			var persons =
				HelperFunctions.ExecuteDataSet(CommandType.Text, string.Format("SELECT TOP {0} Id FROM Person",numberOfPersons), new List<SqlParameter>(),
					"Data Source=.;Integrated Security=SSPI;Initial Catalog=main_Telia_TeleoptiCCC7;Current Language=us_english")
					.Tables[0];
			var scenarios =
				HelperFunctions.ExecuteDataSet(CommandType.Text, "SELECT TOP 1 Id FROM Scenario  where DefaultScenario = 1",
					new List<SqlParameter>(),
					"Data Source=.;Integrated Security=SSPI;Initial Catalog=main_Telia_TeleoptiCCC7;Current Language=us_english")
					.Tables[0];
			var dates = new List<DateTime>();
			var date = new DateTime(2013, 11, 4, 0, 0, 0, DateTimeKind.Utc);
			for (int i = 0; i < 7; i++)
			{
				dates.Add(date.AddDays(i));
			}
			using (IUnitOfWork uow = _logOnHelper.ChoosenDataSource.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var index = 0;
				foreach (DataRow row in persons.Rows)
				{
					var personId = (Guid)row["Id"];
					var scenario = (Guid)scenarios.Rows[0]["Id"];
					IEvent message = new ScheduleChangedEvent
					{
						ScenarioId = scenario,
						StartDateTime = dates[index],
						EndDateTime = dates[index],
						PersonId = personId,
					};

					eventPublisher.Publish(message);
					index++;
					if (index == 7) index = 0;
				}
			}
		}
	}
}
