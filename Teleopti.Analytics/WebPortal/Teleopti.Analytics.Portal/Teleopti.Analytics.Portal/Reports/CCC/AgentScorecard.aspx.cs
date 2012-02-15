using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Globalization;
using Microsoft.AnalysisServices.AdomdClient;

using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.Reports.Ccc
{
    public partial class AgentScorecard : MatrixBasePage
    {
        private CccReports _cccReports;
        private CommonReports _commonReports;

        private enum EnumArrow
            {
            Arrow_DownWard = -1,
            Arrow_Straight = 0,
            Arrow_UpWard = 1
        }
        protected void Page_Init(object sender, EventArgs e)
        {
           
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _cccReports = new CccReports(OlapConnectionString);
            _commonReports = new CommonReports(ConnectionString,new Guid());
            LoggedOnUser.Text = LoggedOnUserInformation;
            
            if (!Page.IsPostBack)
            {
                LoadScorecards();
            }
        }

        protected void ScorecardsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOneScorecard();
            
        }

        protected static string GetResource(string resKey)
        {
            return ReportTexts.Resources.ResourceManager.GetString(resKey);
        }

        private void LoadScorecards()
        {
            DataSet set = _commonReports.ExecuteDataSet("mart.report_ccc_scorecard_get",
                                                    new[]
                                                        {
                                                            new SqlParameter("@UserID", UserCode),
                                                            new SqlParameter("@DateUtc", DateTime.Today.ToUniversalTime()),
                                                        });
            if (set.Tables[0].Rows.Count > 0)
            {
                ScorecardsCombo.DataSource = set;
                ScorecardsCombo.DataBind();

                if (set.Tables[0].Rows.Count == 1)
                {
                    ScorecardsCombo.Visible = false;
                    ScorecardLabel.Visible = true;
                    ScorecardLabel.Text = set.Tables[0].Rows[0].Field<string>("name");
                }
                else
                {
                    ScorecardsCombo.Visible = true;
                    ScorecardLabel.Visible = false;
                }

                LoadOneScorecard();
            }
            else
            {
                //No scorecard to show
                ScorecardsCombo.Visible = false;
                ScorecardLabel.Visible = true;
                ScorecardLabel.Text = ReportTexts.Resources.ResNoScorecardAvailable;
                ResultTable.Visible = false;
            }
        }


        /// <summary>
        /// Loads the one scorecard.
        /// </summary>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-28    
        /// /// </remarks>
        private void LoadOneScorecard()
        {
            Guid scorecardID = new Guid(ScorecardsCombo.SelectedValue);
            int period;
            DataSet sCard = _commonReports.ExecuteDataSet("mart.report_ccc_scorecard_get_period",
                                                      new[]
                                                          {
                                                              new SqlParameter("@ScorecardId", scorecardID),
                                                          });
            if (sCard.Tables[0].Rows.Count == 1)
            {
                period = sCard.Tables[0].Rows[0].Field<int>("period");
            }
            else
                return;

            ResultTable.Visible = true;
            CellSet set = _cccReports.GetAgentScorecardData(UserCode, scorecardID, period);

            Axis axRow = set.Axes[1];

            //TODO: Make team-scorecard-kpi bridge table so that we fetch correct scorecards for MyTime (right now we get them all, with some empty value)
            foreach (Position rPos in axRow.Positions)
            {
                int targetType = 1;
                string format = "P";
                if (targetType == 1) format = "N1";

                TableRow newRow = new TableRow();
                TableCell cell = NewCell();

                cell.Text = ReportTexts.Resources.ResourceManager.GetString(rPos.Members[0].Caption);

                cell.Wrap = false;
                newRow.Cells.Add(cell);

                try
                {

                    AddCellWithTrafficLightToRow(newRow, (int)set[3, rPos.Ordinal].Value);
                    AddCellWithNumberRow(newRow, GetDouble((double?)set[1, rPos.Ordinal].Value), format);
                    AddCellWithNumberRow(newRow, GetDouble((double?)set[0, rPos.Ordinal].Value), format);
                    AddCellWithNumberRow(newRow, GetDouble((double?)set[2, rPos.Ordinal].Value), format);
                    AddCellWithNumberRow(newRow, GetDouble((double?)set[4, rPos.Ordinal].Value), format);
                    AddCellWithArrowToRow(newRow, GetArrow((short?)set[5, rPos.Ordinal].Value));
                }

                catch (Exception)
                {
                    //until Scorecard PBI #15655 is done catch the error and contiune 
                    //... maybe, since we hardly know any MDX. Keep this catch :-(
                    continue;
                }

                ResultTable.Rows.Add(newRow);

            }
        }

        private static double GetDouble(double? val)
        {
        return val == null ? 0 : (double)val;
        }
        
        private static EnumArrow GetArrow(short? val)
        {
            return val == null ? EnumArrow.Arrow_Straight : (EnumArrow)val;
        }

        private static void AddCellWithNumberRow(TableRow row, double value, string format)
        {
            TableCell cell = NewCell();
            cell.Text = value.ToString(format,CultureInfo.CurrentCulture);
            cell.Wrap = false;
            row.Cells.Add(cell);
        }
        
        private static void AddCellWithArrowToRow(TableRow row, EnumArrow theArrow)
        {

            TableCell cell = NewCell();
            cell.Attributes.Add("width", "32px");
            cell.Attributes.Add("height", "32px");
            cell.HorizontalAlign = HorizontalAlign.Center;
            System.Web.UI.WebControls.Image img = new System.Web.UI.WebControls.Image();
            img.SkinID = theArrow.ToString();
            cell.Controls.Add(img);
            cell.CssClass = "ScorecardArrowCell";
            row.Cells.Add(cell);
        }

        private static void AddCellWithTrafficLightToRow(TableRow row, int backColor)
        {
            TableCell cell = NewCell();
            cell.BackColor = Color.FromArgb(backColor);
            cell.Attributes.Add("width", "32px");
            cell.Attributes.Add("height", "32px");
            cell.CssClass = "ScorecardTrafficLightCell";
            row.Cells.Add(cell);
        }

        private static TableCell NewCell()
        {
            TableCell cell = new TableCell();
            cell.CssClass = "ScorecardCell";
            return cell;
        }
    }
}
