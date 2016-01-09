using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class SkillGroupReducerSuggestions : Form
	{
		public SkillGroupReducerSuggestions()
		{
			InitializeComponent();
		}

		public void LoadData(IList<SkillGroupReducer.SkillGroupReducerResult> suggestedActionsList, VirtualSkillGroupsCreatorResult skillGroupsCreatorResult, IList<ISkill> skillList)
		{
			foreach (var skillGroupReducerResult in suggestedActionsList)
			{
				var groupName = skillGroupsCreatorResult.GetNameForKey(skillGroupReducerResult.RemoveFromGroupKey);
				Guid guid;
				if (!Guid.TryParse(skillGroupReducerResult.SkillGuidStringToRemove, out guid))
					return;

				var skill = skillList.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					return;

				var skillName = skill.Name;
				var item = new ListViewItem(skillName);
				item.SubItems.Add(groupName);
				item.SubItems.Add(skillGroupsCreatorResult.GetNameForKey(skillGroupReducerResult.Releasing));
				listView1.Items.Add(item);
			}
		}
	}
}
