using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public class AgentSkillAnalyzerIslandView
	{
		private readonly IEnumerable<IPerson> _personList;
		private readonly IDictionary<ISkill, TimeSpan> _skillDayForecastForSkills;
		private readonly TimeSpan _totalForecastedForDate;
		private readonly DateOnly _date;
		private IList<ISkill> _oldSkillList;

		public AgentSkillAnalyzerIslandView(IEnumerable<IPerson> personList, IDictionary<ISkill, TimeSpan> skillDayForecastForSkills, TimeSpan totalForecastedForDate, DateOnly date)
		{
			_personList = personList;
			_skillDayForecastForSkills = skillDayForecastForSkills;
			_totalForecastedForDate = totalForecastedForDate;
			_date = date;
		}

		public void DrawIslandList(IEnumerable<Island> islands, IEnumerable<ISkill> loadedSkillList, ListView listViewIslands)
		{
			listViewIslands.Items.Clear();
			listViewIslands.Groups.Clear();
			listViewIslands.SuspendLayout();
			var islandNumber = 0;
			foreach (var island in islands)
			{
				var islandModel = island.CreatExtendedClientModel();
				var notLoadedSkills = 0;
				var loadedSkills = 0;
				var personsInIsland = 0;
				foreach (var skillgroup in islandModel.SkillSetsInIsland)
				{
					personsInIsland += skillgroup.AgentsInSkillSet.Count();
				}
				foreach (var islandSkill in islandModel.SkillsInIsland)
				{
					var skill = loadedSkillList.FirstOrDefault(s => s.Id == islandSkill.Id);
					if (skill == null)
						notLoadedSkills++;
					else
					{
						loadedSkills++;
					}
				}
				islandNumber++;
				var item = new ListViewItem("Island " + islandNumber) { Tag = islandModel };
				item.SubItems.Add(islandModel.SkillSetsInIsland.Count().ToString(CultureInfo.InvariantCulture));
				item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
				item.SubItems.Add(personsInIsland.ToString(CultureInfo.InvariantCulture).PadLeft(6));
				listViewIslands.Items.Add(item);

			}
			listViewIslands.Sort();
			listViewIslands.ResumeLayout();
			listViewIslands.Items[0].Selected = true;
		}

		public void ListViewIslandSelectedIndexChanged(IslandExtendedModel selectedIsland, IEnumerable<ISkill> loadedSkillList, ListView listViewGroupsInIsland)
		{
			listViewGroupsInIsland.Items.Clear();
			listViewGroupsInIsland.SuspendLayout();

			var skillGroupCounter = 0;
			foreach (var skillGroup in selectedIsland.SkillSetsInIsland)
			{
				skillGroupCounter++;
				int loadedSkills = 0;
				int notLoadedSkills = 0;
				foreach (var skillGroupSkill in skillGroup.SkillsInSkillSet)
				{
					var skill = loadedSkillList.FirstOrDefault(s => s.Id == skillGroupSkill.Id);
					if (skill == null)
						notLoadedSkills++;
					else
						loadedSkills++;
				}

				var item = new ListViewItem(skillGroupCounter.ToString()) { Tag = skillGroup };
				item.SubItems.Add(loadedSkills.ToString().PadLeft(3) + " (" + notLoadedSkills.ToString().PadLeft(3) + ")");
				item.SubItems.Add(skillGroup.AgentsInSkillSet.Count().ToString().PadLeft(6));
				listViewGroupsInIsland.Items.Add(item);

			}

			_oldSkillList = null;
			listViewGroupsInIsland.ResumeLayout();
			if (listViewGroupsInIsland.Items.Count > 0)
				listViewGroupsInIsland.Items[0].Selected = true;
		}

		public void ListViewGroupsInIslandSelectedIndexChanged(ListView listViewGroupsInIsland, ListView listViewIslandsSkillsOnGroup)
		{
			if (listViewGroupsInIsland.SelectedItems.Count == 0)
				return;

			if (listViewGroupsInIsland.SelectedItems.Count == 1)
				_oldSkillList = null;

			var selectedItem = listViewGroupsInIsland.SelectedItems[0];
			var selectedGroup = selectedItem.Tag as SkillSetExtendedModel;
			if (selectedGroup == null)
				return;

			listViewIslandsSkillsOnGroup.SuspendLayout();
			listViewIslandsSkillsOnGroup.Items.Clear();
			SkillSetExtendedModel lastSelectedGroup = null;
			var modelList = new List<SkillSetExtendedModel>();
			for (int i = 0; i < listViewGroupsInIsland.SelectedItems.Count; i++)
			{
				selectedItem = listViewGroupsInIsland.SelectedItems[i];
				selectedGroup = selectedItem.Tag as SkillSetExtendedModel;
				if (selectedGroup == null)
					continue;

				modelList.Add(selectedGroup);
				lastSelectedGroup = selectedGroup;
			}

			var skillList = createSkillList(modelList);
			var lastSelectedSkills = createSkillList(new[] { lastSelectedGroup });
			var dispalySkillList = new List<DisplaySkill>();
			if (_oldSkillList == null)
			{
				foreach (var skill in skillList)
				{
					dispalySkillList.Add(new DisplaySkill { Color = Color.Black, Skill = skill });
				}
			}
			else
			{
				var intersect = _oldSkillList.Intersect(lastSelectedSkills).ToList();
				foreach (var skill in intersect)
				{
					dispalySkillList.Add(new DisplaySkill { Color = Color.BurlyWood, Skill = skill });
					_oldSkillList.Remove(skill);
					lastSelectedSkills.Remove(skill);
				}

				foreach (var skill in _oldSkillList)
				{
					dispalySkillList.Add(new DisplaySkill { Color = Color.Black, Skill = skill });
				}

				foreach (var skill in lastSelectedSkills)
				{
					dispalySkillList.Add(new DisplaySkill { Color = Color.Red, Skill = skill });
				}
			}

			_oldSkillList = skillList.ToList();
			fillSkillListView(dispalySkillList, listViewIslandsSkillsOnGroup);
			listViewIslandsSkillsOnGroup.Columns[0].Text = "#Skills " + skillList.Count;
			listViewIslandsSkillsOnGroup.Sort();
			listViewIslandsSkillsOnGroup.ResumeLayout();
		}

		public void FilterOnIntersecting(IslandExtendedModel selectedIsland, ListView listViewGroupsInIsland)
		{
			if (listViewGroupsInIsland.SelectedItems.Count != 1)
				return;

			var selectedSkillGroup = listViewGroupsInIsland.SelectedItems[0].Tag as SkillSetExtendedModel;

			listViewGroupsInIsland.SuspendLayout();
			listViewGroupsInIsland.Items.Clear();

			var skillGroupCounter = 0;
			int notLoadedSkills = 0;
			var item = new ListViewItem(skillGroupCounter.ToString()) { Tag = selectedSkillGroup };
			item.SubItems.Add(selectedSkillGroup.SkillsInSkillSet.Count().ToString().PadLeft(3) + " (" + notLoadedSkills.ToString().PadLeft(3) + ")");
			item.SubItems.Add(selectedSkillGroup.AgentsInSkillSet.Count().ToString().PadLeft(6));
			listViewGroupsInIsland.Items.Add(item);
			skillGroupCounter++;

			foreach (var skillGroup in selectedIsland.SkillSetsInIsland)
			{
				if (skillGroup.Equals(selectedSkillGroup))
					continue;

				if (skillGroup.SkillsInSkillSet.Intersect(selectedSkillGroup.SkillsInSkillSet).Any())
				{
					item = new ListViewItem(skillGroupCounter.ToString()) { Tag = skillGroup };
					item.SubItems.Add(skillGroup.SkillsInSkillSet.Count().ToString().PadLeft(3) + " (" + notLoadedSkills.ToString().PadLeft(3) + ")");
					item.SubItems.Add(skillGroup.AgentsInSkillSet.Count().ToString().PadLeft(6));
					listViewGroupsInIsland.Items.Add(item);
					skillGroupCounter++;
				}
			}

			_oldSkillList = null;
			listViewGroupsInIsland.ResumeLayout();
		}

		private void fillSkillListView(IEnumerable<DisplaySkill> skills, ListView view)
		{
			foreach (var displaySkill in skills)
			{
				var item = createSkillItem(displaySkill);
				if (item != null)
					view.Items.Add(item);
			}
		}

		private HashSet<ISkill> createSkillList(IEnumerable<SkillSetExtendedModel> models)
		{
			var uniqueList = new HashSet<ISkill>();
			foreach (var model in models)
			{
				foreach (var skill in model.SkillsInSkillSet)
				{
					uniqueList.Add(skill);
				}
			}

			return uniqueList;
		}

		private ListViewItem createSkillItem(DisplaySkill skillToDisplay)
		{
			var timeFormatter = new TimeFormatter(new ThisCulture(CultureInfo.CurrentCulture));
			var color = skillToDisplay.Color;
			var skill = skillToDisplay.Skill;

			var item = new ListViewItem(skill.Name) { Tag = skill };

			var agentCount = getPersonsOnSkill(skill).Count();
			item.SubItems.Add(agentCount.ToString(CultureInfo.InvariantCulture).PadLeft(8));
			var agentPercent = new Percent(agentCount / (double)_personList.Count());
			item.SubItems.Add(Math.Round(agentPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
			var forecastForSkill = TimeSpan.Zero;
			_skillDayForecastForSkills.TryGetValue(skill, out forecastForSkill);
			item.SubItems.Add(timeFormatter.GetLongHourMinuteTimeString(forecastForSkill).PadLeft(7));
			var forecastPercent = _totalForecastedForDate == TimeSpan.Zero
				? new Percent()
				: new Percent(forecastForSkill.TotalMinutes / _totalForecastedForDate.TotalMinutes);
			item.SubItems.Add(Math.Round(forecastPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
			item.SubItems.Add(skill.SkillType.Description.Name);
			item.SubItems.Add(skill.Activity.Name);

			item.ForeColor = color;

			return item;
		}

		private IEnumerable<IPerson> getPersonsOnSkill(ISkill skill)
		{
			if (skill == null)
			{
				return new List<IPerson>();
			}

			var personsOnSkill = new List<IPerson>();
			foreach (var person in _personList)
			{
				var personPeriod = person.Period(_date);
				if (personPeriod == null)
					continue;

				foreach (var personSkill in personPeriod.PersonSkillCollection)
				{
					if (personSkill.Skill.Id == skill.Id && personSkill.Active)
						personsOnSkill.Add(person);
				}
			}

			return personsOnSkill;
		}
	}

	public class DisplaySkill
	{
		public ISkill Skill { get; set; }
		public Color Color { get; set; }
	}
}