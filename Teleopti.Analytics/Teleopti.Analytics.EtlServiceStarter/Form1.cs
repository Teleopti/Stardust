using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.ServiceLogic;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace EtlServiceStarter
{
	public partial class Form1 : Form
	{
		private EtlJobStarter _etlJobStarter;
        private BusStartup _busStartup;
        private readonly IInitializePayrollFormats _initializePayrollFormats;

		public Form1()
		{
			InitializeComponent();
		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			_etlJobStarter = new EtlJobStarter(ConfigurationManager.AppSettings["datamartConnectionString"], "", "false");
            _etlJobStarter.Start();
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			_etlJobStarter.Dispose();
		}

        private void ServiceBusStart_Click(object sender, EventArgs e)
        {
            _busStartup = new BusStartup(_initializePayrollFormats);
        }

        private void ServiceBusStop_Click(object sender, EventArgs e)
        {

        }
	}
}
