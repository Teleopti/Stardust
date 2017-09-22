using System;
using Teleopti.Analytics.Portal.PerformanceManager.ViewModel;

namespace Teleopti.Analytics.Portal.PerformanceManager.View
{
    public partial class AnalyzerReportView : System.Web.UI.UserControl
    {
        private AnalyzerReportViewModel _analyzerReportViewModel;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public AnalyzerReportViewModel DataSource
        {
            get 
            { 
                return _analyzerReportViewModel; 
            }
            set
            {                
                _analyzerReportViewModel = value;

                if (_analyzerReportViewModel != null)
                {
                    AnalyzerFrame.Attributes.Add("src", _analyzerReportViewModel.Url);
                }
            }
        }

        public string Url
        {
            set
            {
                AnalyzerFrame.Attributes.Add("src", value);
            }
        }
    }
}