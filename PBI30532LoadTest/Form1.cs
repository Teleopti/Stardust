using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.ApplicationConfig.Common;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			var sendToServiceBus = new ServiceBusSender();
			var eventPublisher = new ServiceBusEventPublisher(sendToServiceBus, new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make())));
			
			var persons = HelperFunctions.ExecuteDataSet(CommandType.Text, "SELECT TOP 100 Id FROM Person", new List<SqlParameter>(), "Data Source=.;Integrated Security=SSPI;Initial Catalog=main_Telia_TeleoptiCCC7;Current Language=us_english").Tables[0];
			var scenarios = HelperFunctions.ExecuteDataSet(CommandType.Text, "SELECT TOP 1 Id FROM Scenario  where DefaultScenario = 1", new List<SqlParameter>(), "Data Source=.;Integrated Security=SSPI;Initial Catalog=main_Telia_TeleoptiCCC7;Current Language=us_english").Tables[0];

			using (IUnitOfWork uow = _logOnHelper.ChoosenDataSource.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				foreach (DataRow row in persons.Rows)
				{
					var personId = (Guid)row["Id"];
					var scenario = (Guid)scenarios.Rows[0]["Id"];
					IEvent message = new ScheduleChangedEvent
					{
						ScenarioId = scenario,
						StartDateTime = DateTime.UtcNow,
						EndDateTime = DateTime.UtcNow,
						PersonId = personId,
					};

					eventPublisher.Publish(message);

				}
			}
			
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_logOnHelper = new LogOnHelper("CCCAdmin", "1234567", "c:\\nhib");

			var bu = _logOnHelper.GetBusinessUnitCollection().First();
			_logOnHelper.LogOn(bu);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			int exitCode;
			ProcessStartInfo processInfo;
			Process process;

			//processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
			processInfo = new ProcessStartInfo("cmd.exe", "/c c:\\data\\main\\Teleopti.Support.Tool\\SQLServerPerformance\\RML\\Start.bat '1440' '1' '' '0'");
			processInfo.CreateNoWindow = true;
			processInfo.UseShellExecute = false;
			// *** Redirect the output ***
			processInfo.RedirectStandardError = true;
			processInfo.RedirectStandardOutput = true;

			process = Process.Start(processInfo);
			process.WaitForExit();

			// *** Read the streams ***
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			exitCode = process.ExitCode;

			Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
			Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
			Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
			process.Close();
		}

		
	}

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
}
