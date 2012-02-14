using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace Teleopti.Ccc.OnlineReporting.Testing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBoxReports.DataSource = Report.GetAllReports();
            
            comboBoxCulture.DataSource = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        }

        private void buttonShow_Click(object sender, EventArgs e)
        {
            
                Report selectedReport = ((Report) comboBoxReports.SelectedValue);
            switch (selectedReport.RdlcFile)
            {
                case "report_scheduled_time_per_activity.rdlc":
                    ShowScheduleTimePerActivity(selectedReport.RdlcFile);
                    break;
                default:
                    break;
            }
            
        }
        
        private void ShowScheduleTimePerActivity(string rdlcFile)
        {
            DateTime dateFrom = new DateTime(2009, 2, 2 );
            DateTime dateTo = new DateTime(2009, 2, 10);

            CultureInfo inf = ((CultureInfo)comboBoxCulture.SelectedItem);

            Thread.CurrentThread.CurrentUICulture = inf;
            Thread.CurrentThread.CurrentCulture = inf;

            var data = new Dictionary<string, IList<IReportData>>(); 
            data.Add("DataSet1", Model.ScheduledTimePerActivityModel.GetDummyData() );

            var parameters = new List<IReportDataParameter>();
            parameters.Add(new ReportDataParameter("param_scenario","Default"));
            parameters.Add(new ReportDataParameter("param_agents", "Ola Håkansson, Jenna J, Anders Forsberg, Micke Deigård"));
            parameters.Add(new ReportDataParameter("param_activities", "Aktivity 1,Aktivity 2,Aktivity 3,Aktivity 4,Aktivity 5,Aktivity 6,Aktivity 7"));
            // observera att man bör formatera dessa rätt utifrån baserat på CurrentUser's inställningar
            // dom tas emot som strängar
            parameters.Add(new ReportDataParameter("param_date_from", dateFrom.ToShortDateString()));
            parameters.Add(new ReportDataParameter("param_date_to", dateTo.ToShortDateString()));
            parameters.Add(new ReportDataParameter("param_timezone", TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time").StandardName));
            reportViewerControl1.LoadReport(rdlcFile, data, parameters);
        }


    }
}
