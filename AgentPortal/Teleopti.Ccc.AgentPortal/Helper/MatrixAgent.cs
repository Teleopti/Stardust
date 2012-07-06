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
        private readonly WebBrowserControl _reportPreview = new WebBrowserControl();
        private WebBrowserControl _scoreCardDisplay;
        private string _queryStringBusinessUnit;
        private string _queryStringAuthenticationType;

        private const string MatrixWebSiteUrl = "MatrixWebSiteUrl";
        private const string RelativeAgentScoreCardUrl = "/Reports/Ccc/AgentScorecard.aspx";
        private const string ReportPreviewDefaultPage = "http://www.teleopti.com/";

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
                {
                	_scoreCardDisplay = new WebBrowserControl();
                }
                
                string matrixUrl = StateHolder.Instance.StateReader.SessionScopeData.AppSettings[MatrixWebSiteUrl];
                string scorecardUrl = string.Concat(matrixUrl, RelativeAgentScoreCardUrl);

				scorecardUrl = SetAnalyticsUrlQuerystring(scorecardUrl);
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
            if (string.IsNullOrEmpty(url)) url = ReportPreviewDefaultPage;

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
    	/// <param name="targetFrame">The target frame.</param>
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
        	if (sender.Tag == null)
        	{
        		url = string.Empty;
        		targetFrame = string.Empty;
        		return false;
        	}

        	var matrixReportInfoDto = sender.Tag as MatrixReportInfoDto;
            if (matrixReportInfoDto == null)
            {
                url = string.Empty;
                targetFrame = string.Empty;
                return false;
            }

            url = matrixReportInfoDto.ReportUrl;
            targetFrame = string.IsNullOrEmpty(matrixReportInfoDto.TargetFrame) ? "_self" : matrixReportInfoDto.TargetFrame;
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
                _reportPreview.Dispose();
                _scoreCardDisplay.Dispose();
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
