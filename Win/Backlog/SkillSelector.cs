using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class SkillSelector : Form
	{

		public SkillSelector()
		{
			InitializeComponent();
		}

		public SkillSelector(IList<ISkill> unmappedSkills)
		{
			InitializeComponent();
			listBox1.DataSource = unmappedSkills;
			listBox1.DisplayMember = "Name";
			listBox2.DataSource = unmappedSkills;
			listBox2.DisplayMember = "Name";
		}

		private void SkillSelector_Load(object sender, EventArgs e)
		{

		}

		public Tuple<ISkill, ISkill> SelectedSkills
		{
			get { return new Tuple<ISkill, ISkill>((ISkill) listBox1.SelectedItem, (ISkill) listBox1.SelectedItem); }
		}
	}
}
