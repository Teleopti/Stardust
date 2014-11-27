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
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;
using System.Configuration;

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
			_logOnHelper = new LogOnHelper("demo", "demo", "c:\\nhib");

			var bu = _logOnHelper.GetBusinessUnitCollection().Where(b => b.Id.Equals(new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"))).First();
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
			var eventPublisher = new ServiceBusEventPublisher(sendToServiceBus);

			var connectionString = ConfigurationManager.ConnectionStrings["App"].ConnectionString; 
			// "Data Source=.;Integrated Security=SSPI;Initial Catalog=main_clone_DemoSales_TeleoptiCCC7;Current Language=us_english";
			var scenarios =
				HelperFunctions.ExecuteDataSet(CommandType.Text, "SELECT TOP 1 Id FROM Scenario  where DefaultScenario = 1",
					new List<SqlParameter>(),
					connectionString)
					.Tables[0];
			var scenario = (Guid)scenarios.Rows[0]["Id"];
			const string sql = @"SELECT TOP {0} Date, Person FROM PersonAssignment WHERE Scenario = '{1}' AND Date >= '{2}' ORDER BY Date";
			var persons =
				HelperFunctions.ExecuteDataSet(CommandType.Text, string.Format(sql,numberOfPersons, scenario, "2014-11-25"), new List<SqlParameter>(),
					connectionString)
					.Tables[0];
			
			//var dates = new List<DateTime>();
			//var date = new DateTime(2013, 11, 4, 0, 0, 0, DateTimeKind.Utc);
			//for (int i = 0; i < 7; i++)
			//{
			//	dates.Add(date.AddDays(i));
			//}
			using (IUnitOfWork uow = _logOnHelper.ChoosenDataSource.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				eventPublisher.EnsureBus();
				var index = 0;
				foreach (DataRow row in persons.Rows)
				{
					var personId = (Guid)row["Person"];
					var date = new DateTime(((DateTime) row["Date"]).Ticks, DateTimeKind.Utc);
					IEvent message = new ScheduleChangedEvent
					{
						ScenarioId = scenario,
						StartDateTime = date,
						EndDateTime = date,
						PersonId = personId,
						Datasource = _logOnHelper.ChoosenDataSource.DataSource.Application.Name
					};
					const string update = @"UPDATE PersonAssignment SET UpdatedOn = GETDATE()
WHERE Person = '{0}'
AND Date = '{1}'";
					HelperFunctions.ExecuteNonQuery(CommandType.Text, string.Format(update, personId, date.ToString("yyyy-MM-dd")),
						new List<SqlParameter>(),
						connectionString);
					eventPublisher.Publish(message);
					index++;
					if (index == 7) index = 0;
				}
			}
		}
	}
}
