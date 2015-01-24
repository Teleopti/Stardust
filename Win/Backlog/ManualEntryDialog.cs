using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Backlog;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class ManualEntryDialog : Form
	{
		private readonly BacklogModel _model;
		private readonly ISkill _skill;

		public ManualEntryDialog()
		{
			InitializeComponent();
		}

		public ManualEntryDialog(BacklogModel model, ISkill skill)
		{
			InitializeComponent();
			_model = model;
			_skill = skill;
			dateTimePicker1.MinDate = _model.Period().StartDate;
			dateTimePicker1.MaxDate = _model.Period().EndDate;
			dateTimePicker1.Value = dateTimePicker1.MinDate;
		}

		private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
		{
			var date = new DateOnly(dateTimePicker1.Value);
			var hours = _model.GetManualEntryOnDate(date, _skill);
			if(!hours.HasValue)
				hours = TimeSpan.FromHours(1);
			numericUpDown1.Value = (int)hours.Value.TotalHours;
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			var date = new DateOnly(dateTimePicker1.Value);
			_model.SetManualEntryOnDate(date,_skill, TimeSpan.FromHours((int)numericUpDown1.Value));
			Close();
		}
	}
}
