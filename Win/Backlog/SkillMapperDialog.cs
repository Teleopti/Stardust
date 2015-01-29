using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class SkillMapperDialog : Form
	{
		private readonly IDictionary<ISkill, ISkill> _skillPairs;
		private IList<ISkill> _rightList;

		public SkillMapperDialog()
		{
			InitializeComponent();
		}

		public SkillMapperDialog(IDictionary<ISkill, ISkill> skillPairs )
		{	
			InitializeComponent();
			_skillPairs = skillPairs;
			IList<ISkill> leftList = new List<ISkill>(skillPairs.Keys);
			_rightList = new List<ISkill>(skillPairs.Values);
			listBox1.DataSource = leftList;
			listBox1.DisplayMember = "Name";

			listBox2.DataSource = _rightList;
			listBox2.DisplayMember = "Name";
		}

		private void buttonRightUp_Click(object sender, EventArgs e)
		{
			var selected = (ISkill) listBox2.SelectedItem;
			var index = _rightList.IndexOf((ISkill) listBox2.SelectedItem);
			if (index > 0)
			{
				_rightList.RemoveAt(index);
				_rightList.Insert(index - 1, selected);
			}
			listBox2.Refresh();
		}

		private void buttonRightDown_Click(object sender, EventArgs e)
		{
			var selected = (ISkill)listBox2.SelectedItem;
			var index = _rightList.IndexOf((ISkill)listBox2.SelectedItem);
			if (index < _rightList.Count)
			{
				_rightList.RemoveAt(index);
				_rightList.Insert(index + 1, selected);
			}
			listBox2.Refresh();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			foreach (var key in _skillPairs.Keys)
			{
				_skillPairs[key] = _rightList[0];
				_rightList.RemoveAt(0);
			}
		}
	}
}
