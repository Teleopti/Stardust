using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class SkillMapperDialog : Form
	{
		private readonly IList<ISkillMap_DEV> _skillMappings;
		private IList<ISkill> _allSkills;
		private IList<ISkill> _rightList;
		private List<ISkill> _originalSkills;

		public SkillMapperDialog()
		{
			InitializeComponent();
		}

		public SkillMapperDialog(IEnumerable<ISkillMap_DEV> skillMappings, IEnumerable<ISkill> allSkills)
		{
			InitializeComponent();
			_skillMappings = new List<ISkillMap_DEV>(skillMappings);
			
			_allSkills = allSkills.ToList();
			_originalSkills = new List<ISkill>(_allSkills);

			rebuildDataBindings();
		}

		private void rebuildDataBindings()
		{
			_allSkills = new List<ISkill>(_originalSkills);
			IList<ISkill> leftList = new List<ISkill>();
			_rightList = new List<ISkill>();
			foreach (var skillMapping in _skillMappings)
			{
				leftList.Add(skillMapping.Parent);
				_allSkills.Remove(skillMapping.Parent);
				if (skillMapping.MappedSkill != null)
				{
					_rightList.Add(skillMapping.MappedSkill);
					_allSkills.Remove(skillMapping.MappedSkill);
				}
			}

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
			listBox2.DataSource = null;
			listBox2.DataSource = _rightList;
			listBox2.DisplayMember = "Name";
			listBox2.Refresh();
		}

		private void buttonRightDown_Click(object sender, EventArgs e)
		{
			var selected = (ISkill)listBox2.SelectedItem;
			var index = _rightList.IndexOf((ISkill)listBox2.SelectedItem);
			if (index < _rightList.Count-1)
			{
				_rightList.RemoveAt(index);
				_rightList.Insert(index + 1, selected);
			}
			listBox2.DataSource = null;
			listBox2.DataSource = _rightList;
			listBox2.DisplayMember = "Name";
			listBox2.Refresh();
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			foreach (var skillMapping in _skillMappings)
			{
				skillMapping.MappedSkill = _rightList[0];
				_rightList.RemoveAt(0);
			}
		}

		public IList<ISkillMap_DEV> Mappings()
		{
			return _skillMappings;
		}

		private void buttonAL_Click(object sender, EventArgs e)
		{
			using (var selector = new SkillSelector(_allSkills))
			{
				selector.ShowDialog(this);
				if (selector.DialogResult == DialogResult.OK)
				{
					_skillMappings.Add(new SkillMap_DEV(selector.SelectedSkills.Item1, selector.SelectedSkills.Item2));
				}
			}
			rebuildDataBindings();
		}

		private void buttonRL_Click(object sender, EventArgs e)
		{

		}

		private void buttonAR_Click(object sender, EventArgs e)
		{

		}

		private void buttonRR_Click(object sender, EventArgs e)
		{

		}
	}
}
