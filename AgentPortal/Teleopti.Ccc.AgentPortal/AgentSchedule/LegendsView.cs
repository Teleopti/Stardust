using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;

namespace Teleopti.Ccc.AgentPortal.AgentSchedule
{
    public static class GridStyleInfoExtensions
    {
        public static void ResetDefault()
        {
            var defaultField = typeof(GridStyleInfo).GetField("defaultStyle", BindingFlags.Static | BindingFlags.NonPublic);
            defaultField.SetValue(null, null);
        }
    }

    public partial class LegendsView : BaseUserControl, ILegendsView
    {
        private LegendsPresenter _presenter;

        public LegendsView(ILegendLoader legendLoader)
        {
            InitializeComponent();
            GridStyleInfoExtensions.ResetDefault();
            setupPresenter(legendLoader);
            setupDatasources();
            setupGrid();
            SetTexts();           
        }

        private void setupPresenter(ILegendLoader legendLoader)
        {
            _presenter = new LegendsPresenter(this, legendLoader);
            _presenter.Initialize();
        }

        public int DefaultHeight { get { return _presenter.Height(); } }

        public ArrayList AbsenceDataSource { get; set; }

        public ArrayList ActivityDataSource { get; set; }

        public int AbsenceHeight
        {
            get { return groupBoxAbsence.Height; }
            set { groupBoxAbsence.Height = value; }
        }

        public int ActivityHeight
        {
            get { return groupBoxActivity.Height; }
            set { groupBoxActivity.Height = value; }
        }

        private void setupGrid()
        {
            gridListActivities.Grid.CellDrawn += onCellDrawn;
            gridListAbsences.Grid.CellDrawn += onCellDrawn;
            gridListAbsences.Grid.ColWidths[1] = 20;
            gridListActivities.Grid.ColWidths[1] = 20;
            gridListActivities.Grid.Model.Options.DefaultGridBorderStyle = GridBorderStyle.None;
            gridListAbsences.Grid.Model.Options.DefaultGridBorderStyle = GridBorderStyle.None;
            gridListActivities.BorderStyle = BorderStyle.None;
            gridListAbsences.BorderStyle = BorderStyle.None;
            gridListAbsences.Grid.Office2007ScrollBars = true;
            gridListActivities.Grid.Office2007ScrollBars = true;
        }

        private void setupDatasources()
        {
            gridListAbsences.DataSource = AbsenceDataSource;
            gridListActivities.DataSource = ActivityDataSource;
        }

        private void onCellDrawn(object sender, GridDrawCellEventArgs e)
        {
            if (e.ColIndex > 0 && e.RowIndex > 0)
            {
                if (e.ColIndex == 1) //Column 1 is the colorbox
                {
                    drawColorBox(e);
                }
            }
        }

        private void drawColorBox(GridDrawCellEventArgs e)
        {
            var rectangle = e.Bounds;
            using (var brush = new LinearGradientBrush(rectangle, Color.WhiteSmoke, (Color)e.Style.CellValue, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, rectangle);
            }
        }
    }
}
