using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.AnalyzerProxy.AnalyzerRef;

namespace Teleopti.Analytics.Portal.PerformanceManager.ViewModel
{
    public class AnalyzerReportViewModel
    {
        private string _olapServer;
        private string _olapDatabase;

        public AnalyzerReportViewModel(int reportId)
        {
            // Constructor for open existing report
            SetOlapInformation();
            OpenReport(reportId);
        }

        public AnalyzerReportViewModel(string reportName)
        {
            // Constructor for creating new report
            SetOlapInformation();
            int reportId = CreateNewReport(reportName);
            switch (reportId)
            {
                case -1:
                    // Report with that name already exist
                    DoReportAlreadyExist = true;
                    Message = string.Concat("Failed to create report '", reportName, "'.");
                    break;
                case -2:
                    // Report with that name already exist
                    DoReportAlreadyExist = true;
                    Message = string.Concat("Report with name '", reportName,
                                            "' already exist. Please choose another name.");
                    break;
                default:
                    OpenReport(reportId);
                    break;
            }
            //if (reportId == -2)
            //{
            //    // Report with that name already exist
            //    DoReportAlreadyExist = true;
            //    Message = string.Concat("Report with name '", reportName,
            //                            "' already exist. Please choose another name.");
            //}
            //else
            //{
            //    OpenReport(reportId);
            //}
        }

        public string Url { get; private set; }
        public ReportInstance ReportInstance { get; private set; }
        public bool DoReportAlreadyExist { get; private set; }
        public string Message { get; private set; }

        private void SetOlapInformation()
        {
            var olapInformation = new OlapInformation();

            _olapServer = olapInformation.OlapServer;
            _olapDatabase = olapInformation.OlapDatabase;
        }

        private void OpenReport(int reportId)
        {
            // Open report
            PermissionLevel userPermissions = PermissionInformation.UserPermissions;

            using (var clientProxy = new ClientProxy(_olapServer, _olapDatabase))
            {
                ReportInstance = clientProxy.OpenReport(reportId, userPermissions);
                Url = clientProxy.GetReportUrl(ReportInstance).ToString();
            }
        }
        private int CreateNewReport(string reportName)
        {
            // Create new report
            var report = new CatalogItem();
            report.Name = reportName;
            report.ItemType = CatalogItemType.Report;
            report.ParentId = 0;

            using (var clientProxy = new ClientProxy(_olapServer, _olapDatabase))
            {
                if (clientProxy.DoReportExist(reportName))
                {
                    // Report with that name already exist
                    return -2;
                }

                return clientProxy.NewReport(report);
            }
        }
    }
}