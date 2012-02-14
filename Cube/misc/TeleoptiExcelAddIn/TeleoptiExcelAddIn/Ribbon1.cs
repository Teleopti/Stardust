using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Tools.Ribbon;

namespace Teleopti.ExcelAddIn
{
    public partial class Ribbon1 : OfficeRibbon
    {
        private Worksheet _worksheet;
        private PivotTable _pivotTable;
        private CubeFields _cubeFields;

        public Ribbon1()
        {
            InitializeComponent();
        }

        public PivotTable PivotTable
        {
            get { return _pivotTable; }
            set { _pivotTable = value; }
        }

        public Worksheet Worksheet
        {
            get { return _worksheet; }
            set { _worksheet = value; }
        }

        private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
        {
            foreach (RibbonGroup ribbonGroup in tab1.Groups)
            {
                Trace.WriteLine(ribbonGroup.Name); 
            }
            
        }

        private void btnCalculatedMeasure_Click(object sender, RibbonControlEventArgs e)
        {
            InitiatePivotTable();
            if(_pivotTable == null)
                return;
            InitiateCubeFields();
            if(_cubeFields == null)
                return;
            CalculatedMeasures calcMeasuresForm = new CalculatedMeasures(_cubeFields);
            calcMeasuresForm.Show();
        }

        private void InitiatePivotTable()
        {
            _worksheet = (Worksheet)Globals.ThisAddIn.Application.ActiveSheet;
            if (_worksheet != null)
            {
                PivotTables pivotTables = (PivotTables)_worksheet.PivotTables(Missing.Value);
                if (pivotTables.Count > 0)
                {
                    _pivotTable = (PivotTable)_worksheet.PivotTables(1);
                    return;
                }
            }
            MessageBox.Show("No Pivot table on the Worksheet.", "Information", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);
        }

        private void InitiateCubeFields()
        {
            _cubeFields = _pivotTable.CubeFields;
            if (_cubeFields == null)
                MessageBox.Show("No Cube fields found for the Pivot table.", "Information", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly);
        }
    }
}
