using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Common.Controls;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortal.Main;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// For Matrix report generation
    /// </summary>
    /// <remarks>
    /// Created by: kosalanp
    /// Created date: 5/15/2008
    /// </remarks>
    class MatrixAgent : IDisposable
    {
        private static MatrixAgent _instance = new MatrixAgent();
        private ScoreCardControl _scoreCardPreview = new ScoreCardControl();
        private WebBrowserControl _reportPreview = new WebBrowserControl();
        private WebBrowserControl _scoreCardDisplay;
        private string _queryStringBusinessUnit;
        private string _queryStringAuthenticationType;

        private const string MatrixWebSiteUrl = "MatrixWebSiteUrl";
        private const string RelativeAgentScoreCardUrl = "/Reports/Ccc/AgentScorecard.aspx";
        //private const string DefaultAgentScoreCard = "Default";
        private const string _reportPreviewDefaultPage = "http://www.teleopti.com/";

        private MatrixAgent() { }

        public static MatrixAgent Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MatrixAgent();

                return _instance;
            }
        }

        /// <summary>
        /// Gets the score card preview.
        /// </summary>
        /// <value>The score card preview.</value>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 5/15/2008
        /// </remarks>
        public ScoreCardControl ScoreCardPreview
        {
            get
            {
                if (_scoreCardPreview == null)
                    _scoreCardPreview = new ScoreCardControl();

                string matrixURL = StateHolder.Instance.StateReader.SessionScopeData.AppSettings[MatrixWebSiteUrl];
                string scorecardUrl = string.Concat(matrixURL, RelativeAgentScoreCardUrl);
                scorecardUrl = SetAnalyticsUrlQuerystring(scorecardUrl);
                //string scorecarPreviewdUrl = scorecardUrl;
                // Since no working preview (default) scorecard exists today we show the original scorecard instead.
                //if (scorecardUrl.Contains("."))
                //{
                //    scorecarPreviewdUrl = scorecardUrl.Insert(scorecardUrl.IndexOf(".", StringComparison.OrdinalIgnoreCase), DefaultAgentScoreCard);
                //}
                //_scoreCardPreview.WebBrowser.Url = new Uri(scorecarPreviewdUrl);
                //_scoreCardPreview.WebBrowser.Navigate(scorecarPreviewdUrl);
                _scoreCardPreview.WebBrowser.Url = new Uri(scorecardUrl);
                _scoreCardPreview.WebBrowser.Navigate(scorecardUrl);

                return _scoreCardPreview;
            }
        }

        /// <summary>
        /// Gets the score card.
        /// </summary>
        /// <value>The score card.</value>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 5/16/2008
        /// </remarks>
        public WebBrowserControl ScoreCard
        {
            get
            {
                if (_scoreCardDisplay == null)
                    _scoreCardDisplay = new WebBrowserControl();
                
                //TODO: check for null here
                string matrixURL = StateHolder.Instance.StateReader.SessionScopeData.AppSettings[MatrixWebSiteUrl];
                string scorecardUrl = string.Concat(matrixURL, RelativeAgentScoreCardUrl);
                //string scorecardUrl = StateHolder.Instance.StateReader.SessionScopeData.AppSettings[RelativeAgentScoreCardUrl];
                _scoreCardDisplay.WebBrowser.Url = new Uri(scorecardUrl);
                _scoreCardDisplay.Name = "Scorecard";
                _scoreCardDisplay.WebBrowser.Navigate(scorecardUrl);

                return _scoreCardDisplay;
            }
        }

        /// <summary>
        /// Builds up a Querystring variable for current business unit.
        /// </summary>
        /// <value>The querystring variable for business unit.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-04-06
        /// </remarks>
        private string QueryStringBusinessUnit
        {
            get
            {
                if (string.IsNullOrEmpty(_queryStringBusinessUnit))
                {
                    BusinessUnitDto businessUnit = StateHolder.Instance.StateReader.SessionScopeData.BusinessUnit;
                    _queryStringBusinessUnit = string.Format(CultureInfo.InvariantCulture, "buid={0}", businessUnit.Id);
                }
                return _queryStringBusinessUnit;
            }
        }

        /// <summary>
        /// Builds up a Querystring variable for authentication type.
        /// </summary>
        /// <value>The querystring variable for authentication type.</value>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-04-06
        /// </remarks>
        private string QueryStringAuthenticationType
        {
            get
            {
                if (string.IsNullOrEmpty(_queryStringAuthenticationType))
                {
                    string forceFormsLoginText = "false";

                    AuthenticationTypeOptionDto authenticationType = StateHolder.Instance.StateReader.SessionScopeData.DataSource.AuthenticationTypeOptionDto;

                    if (authenticationType == AuthenticationTypeOptionDto.Application)
                    {
                        forceFormsLoginText = "true";
                    }
                    _queryStringAuthenticationType = string.Format(CultureInfo.InvariantCulture, "forceformslogin={0}", forceFormsLoginText);
                }

                return _queryStringAuthenticationType;
            }
        }

        /// <summary>
        /// Gets the matrix report.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 5/15/2008
        /// </remarks>
        public WebBrowserControl GetMatrixReport()
        {
            string url = MainScreen.SelectedReport;
            if (string.IsNullOrEmpty(url)) url = _reportPreviewDefaultPage;

            _reportPreview.Name = "Reports";
            url = SetAnalyticsUrlQuerystring(url);
            _reportPreview.WebBrowser.Navigate(url, MainScreen.SelectedTargetFrame);
            if (MainScreen.SelectedTargetFrame == "_blank")
            {
                return null;
            }
            return _reportPreview;
        }

        /// <summary>
        /// Tries the parse to URL.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: kosalanp
        /// Created date: 5/15/2008
        /// </remarks>
        public static bool TryParseToUrl(ToolStripMenuItem sender, out string url, out string targetFrame)
        {
            if (sender == null)
            {
                url = string.Empty;
                targetFrame = string.Empty;
                return false;
            }
            else if (sender.Tag == null)
            {
                url = string.Empty;
                targetFrame = string.Empty;
                return false;
            }

            MatrixReportInfoDto matrixReportInfoDto = sender.Tag as MatrixReportInfoDto;
            if (matrixReportInfoDto == null)
            {
                url = string.Empty;
                targetFrame = string.Empty;
                return false;
            }

            url = matrixReportInfoDto.ReportUrl;
            if (string.IsNullOrEmpty(matrixReportInfoDto.TargetFrame))
            {
                targetFrame = "_self";
            }
            else
            {
                targetFrame = matrixReportInfoDto.TargetFrame;
            }
            return !string.IsNullOrEmpty(url);
        }

        /// <summary>
        /// Sets the analytics URL querystring with business unit id and authentication type.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Jonas N
        /// Created date: 2009-04-06
        /// </remarks>
        private string SetAnalyticsUrlQuerystring(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                {
                    url += "?";
                }

                if (!url.EndsWith("?", StringComparison.OrdinalIgnoreCase) & !url.EndsWith("&", StringComparison.OrdinalIgnoreCase))
                {
                    url += "&";
                }

                // Url prepared - concat querystring variables
                return string.Concat(url, QueryStringBusinessUnit, "&", QueryStringAuthenticationType);
            }

            return string.Empty;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _reportPreview.Dispose();
                _scoreCardDisplay.Dispose();
                _scoreCardPreview.Dispose();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
