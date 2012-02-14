using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Teleopti.Ccc.TestPayrollGui.Infrastructure;
using Teleopti.Ccc.TestPayrollGui.Sdk;


namespace Teleopti.Ccc.TestPayrollGui
{
    public partial class MainForm : Form
    {
        private ServiceApplication _app;
        private IList<string> _tickets = new List<string>();
        private string _selectedGuid;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setupGrid();
            _app = new ServiceApplication("demo", "demo");

            var payrollExports = _app.OrganizationService.GetPayrollExportByQuery(new GetAllPayrollExportsQueryDto());

            listBoxExports.DataSource = payrollExports;
            listBoxExports.DisplayMember = "Name";
            loadOldResults(payrollExports);
        }
   
        private void loadOldResults(IEnumerable<PayrollExportDto> payrollExports)
        {
            foreach (var exportDto in payrollExports)
            {
                var queryDto = new GetPayrollResultStatusByExportIdQueryDto {PayrollExportId = exportDto.Id};
                foreach (var resultDto in  _app.OrganizationService.GetPayrollResultStatusByQuery(queryDto))
                {
                    _tickets.Add(resultDto.Id);                    
                }
            }
            refreshDataGrid();
        }

        private void buttonRunExport_Click(object sender, EventArgs e)
        {
            var dto = new CreatePayrollExportCommandDto();
            dto.PayrollExportDto = (PayrollExportDto) listBoxExports.SelectedItem;
            var ticket = _app.InternalService.ExecuteCommand(dto);
            MessageBox.Show(string.Format("Async job started, Affected Id: {0}", ticket.AffectedId));
            _tickets.Add(ticket.AffectedId);
            refreshDataGrid();
        }

        private void refreshDataGrid()
        {
            dataGridViewResult.DataSource = refreshDatasource();
        }

        private void setupGrid()
        {
            dataGridViewResult.AutoGenerateColumns = false;
            var id = new DataGridViewTextBoxColumn();
            id.DataPropertyName = "Id";
            id.HeaderText = "Id";
            var finishedOk = new DataGridViewTextBoxColumn();
            finishedOk.DataPropertyName = "FinishedOk";
            finishedOk.HeaderText = "FinishedOk";
            var hasError = new DataGridViewTextBoxColumn();
            hasError.DataPropertyName = "HasError";
            hasError.HeaderText = "HasError";
            var isWorking = new DataGridViewTextBoxColumn();
            isWorking.DataPropertyName = "IsWorking";
            isWorking.HeaderText = "IsWorking";
            var timestamp = new DataGridViewTextBoxColumn();
            timestamp.DataPropertyName = "Timestamp";
            timestamp.HeaderText = "Timestamp";

            dataGridViewResult.Columns.Add(id);
            dataGridViewResult.Columns.Add(finishedOk);
            dataGridViewResult.Columns.Add(hasError);
            dataGridViewResult.Columns.Add(isWorking);
            dataGridViewResult.Columns.Add(timestamp);

            resizeColumns();
        }

        private void resizeColumns()
        {
            foreach (DataGridViewTextBoxColumn column in dataGridViewResult.Columns)
            {
                column.Width = (dataGridViewResult.Width-60)/dataGridViewResult.Columns.Count;
            }
        }

        private void buttonRefreshResults_Click(object sender, EventArgs e)
        {
            dataGridViewResult.DataSource = refreshDatasource();
        }

        private BindingList<PayrollResultDto> refreshDatasource()
        {
            var refreshedResult = new List<PayrollResultDto>();
            foreach (var result in _tickets.OrderBy(t=>t))
            {
                var queryResult = new GetPayrollResultStatusByIdQueryDto {PayrollResultId = result};
                var found = _app.OrganizationService.GetPayrollResultStatusByQuery(queryResult).FirstOrDefault();
                if (found!=null)
                {
                    refreshedResult.Add(found);
                }
            }
            return new BindingList<PayrollResultDto>(refreshedResult.OrderByDescending(i=>i.Timestamp).ToList());
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            resizeColumns();
        }

        private void dataGridViewResult_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridViewResult.CurrentCell = dataGridViewResult.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var r = dataGridViewResult.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                contextMenuStripGrid.Show(dataGridViewResult, r.Left + e.X, r.Top + e.Y);
                _selectedGuid = dataGridViewResult.CurrentRow.Cells[0].FormattedValue.ToString();
                contextMenuStripGrid.Items[0].Enabled = Convert.ToBoolean(dataGridViewResult.CurrentRow.Cells[1].FormattedValue, CultureInfo.InvariantCulture);
            }
        }


        /// <summary>
        /// Saving the result from the payroll 
        /// </summary>
        /// <param name="fileName"></param>
        private void saveResultToFile(string fileName)
        {
            var encoding = new ASCIIEncoding();
            var postData = "ResultGuid=" + _selectedGuid;
            postData += ("&UserName=" + AuthenticationSoapHeader.Current.UserName);
            postData += ("&Password=" + AuthenticationSoapHeader.Current.Password);
            postData += ("&DataSource=" + AuthenticationSoapHeader.Current.DataSource);
            postData += ("&UseWindowsIdentity=" + AuthenticationSoapHeader.Current.UseWindowsIdentity);

            var data = encoding.GetBytes(postData);

            var req = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1335/GetPayrollResultById.aspx"));

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            var newStream = req.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            var response = (HttpWebResponse)req.GetResponse();

            var fileInfo = new FileInfo(fileName);
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                using (var streamWriter = fileInfo.CreateText())
                {
                    streamWriter.Write(reader.ReadToEnd());
                }
            }
        }

        private void toolStripMenuItemSaveResult_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    saveResultToFile(dialog.FileName);
                }
            }
        }
    }
}
