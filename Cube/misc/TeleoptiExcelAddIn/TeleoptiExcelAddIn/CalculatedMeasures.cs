using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace Teleopti.ExcelAddIn
{
    public partial class CalculatedMeasures : Form
    {
        private readonly CubeFields _cubeFields;

        private CalculatedMeasures()
        {
            InitializeComponent();
        }

        public CalculatedMeasures(CubeFields cubeFields):this()
        {
            _cubeFields = cubeFields;
        }

        private void CalculatedMeasures_Load(object sender, EventArgs e)
        {
            FillCalculatedMemberList();
            FillFieldList();
        }

        private void FillCalculatedMemberList()
        {
            CalculatedMembers calcMembers = Globals.Ribbons.Ribbon1.PivotTable.CalculatedMembers;
            cboName.BeginUpdate();
            cboName.Items.Clear();
            foreach (CalculatedMember calcMember in calcMembers)
            {
                cboName.Items.Add(calcMember.Name);
            }
            cboName.EndUpdate();
        }

        private void FillFieldList()
        {
            lsbFields.BeginUpdate();
            lsbFields.Items.Clear();
            foreach (CubeField cubeField in _cubeFields)
            {
                //Only show measures
                if (cubeField.Name.StartsWith("[Measures]", StringComparison.CurrentCulture))
                    lsbFields.Items.Add(cubeField.Name);
            }
            lsbFields.EndUpdate();
        }

        private void btnInsertField_Click(object sender, EventArgs e)
        {
            if (lsbFields.SelectedIndex > -1)
            {
                txtFormula.Text += lsbFields.Text;
                txtFormula.Focus();
                txtFormula.Select(txtFormula.Text.Length, 0);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!IsNameValid())
                return;
            if(!IsFormulaValid())
                return;
            AddCalculatedMeasure();
            FillCalculatedMemberList();
            FillFieldList();
            cboName.SelectedIndex = cboName.Items.Count - 1;
        }

        private void AddCalculatedMeasure()
        {
            try
            {
                Globals.Ribbons.Ribbon1.PivotTable.CalculatedMembers.Add(cboName.Text, txtFormula.Text, null,
                                                                      XlCalculatedMemberType.xlCalculatedMember);
                Globals.Ribbons.Ribbon1.PivotTable.ViewCalculatedMembers = true;
            }
            catch(COMException comEx)
            {
                Trace.WriteLine("Error trying to add Calculated Measure. Msg: " + comEx.Message);
                MessageBox.Show("The calculated measure could not be created. The cause could be an invalid name or formula.", "Warning", MessageBoxButtons.OK,
                            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private bool IsNameValid()
        {
            if (cboName.Text.Length > 0 
                    && cboName.Text.IndexOf(" ", StringComparison.CurrentCulture) == -1 
                    && IsNameOccupied())
            {
                return true;
            }
            else
            {
                MessageBox.Show("Invalid Name. (Name must be unique and cannot include spaces.)", "Information", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
        }

        private bool IsFormulaValid()
        {
            //TODO: Check for invalid charcters.
            if (txtFormula.Text.Length > 0 && IsFormulaUsingCubeField())
            {
                return true;
            }
            else
            {
                MessageBox.Show("Invalid formula. (At least one [Measures] field must be used.)", "Information", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.DefaultDesktopOnly);
                return false;
            }
        }

        private bool IsNameOccupied()
        {
            CalculatedMembers calcMembers = Globals.Ribbons.Ribbon1.PivotTable.CalculatedMembers;
            
            foreach (CalculatedMember calcMember in calcMembers)
            {
                if(cboName.Text.IndexOf(calcMember.Name, StringComparison.CurrentCulture) > -1)
                    return false;   //Name occupied
            }
            return true;
        }

        private bool IsFormulaUsingCubeField()
        {
            // A formula must at least use one measure field
            foreach (CubeField cubeField in _cubeFields)
            {
                //Only check measures
                if (cubeField.Name.StartsWith("[Measures]", StringComparison.CurrentCulture))
                {
                    if(txtFormula.Text.IndexOf(cubeField.Name, StringComparison.CurrentCulture) > -1)
                        return true;
                }
            }
            return false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            CalculatedMember calcMember = GetSelectedCalculatedMember();
            if(calcMember != null)
            {
                calcMember.Delete();
                Globals.Ribbons.Ribbon1.PivotTable.ViewCalculatedMembers = true;
                FillCalculatedMemberList();
                cboName.Text = "";
                txtFormula.Text = "";
            }
        }

        private CalculatedMember GetSelectedCalculatedMember()
        {
            CalculatedMembers calcMembers = Globals.Ribbons.Ribbon1.PivotTable.CalculatedMembers;
            if (cboName.SelectedIndex > -1 && calcMembers.Count > 0)
            {
                for (int index = 1; index <= calcMembers.Count; index++)
                {
                    CalculatedMember calcMember = calcMembers[index];
                    if (cboName.SelectedItem.ToString() == calcMember.Name)
                    {
                        return calcMember;
                    }
                }
            }
            return null;
        }

        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculatedMember calcMember = GetSelectedCalculatedMember();
            if (calcMember != null)
            {
                txtFormula.Text = calcMember.Formula;
            }
            else txtFormula.Text = "";
        }

        private void lsbFields_DoubleClick(object sender, EventArgs e)
        {
            btnInsertField_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtFormula.Text = "";
        }

        private void lsbFields_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
