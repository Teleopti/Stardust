﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class AgentSkillAnalyzer : Form
	{
		private readonly IEnumerable<IPerson> _personList;
		private DateOnly _date;
		private readonly IList<ISkill> _loadedSkillList;
		private readonly IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays;
		private readonly DateOnlyPeriod _datePeriod;
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private VirtualSkillGroupsCreatorResult _skillGroupsCreatorResult;
		private IList<OldIsland> _islandList;
		private IList<Island> _islandListBeforeReducing;
		private IDictionary<ISkill,TimeSpan> _skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
		private TimeSpan _totalForecastedForDate;
		private readonly TimeFormatter _timeFormatter = new TimeFormatter(new ThisCulture(CultureInfo.CurrentCulture));
		private IList<ISkill> _allSkills;
		private readonly DateTimePicker _dtpDate;
		private readonly IList<IPersonSkill> _modifiedPersonSkills = new List<IPersonSkill>(); 

		public AgentSkillAnalyzer()
		{
			InitializeComponent();
		}

		public AgentSkillAnalyzer(IEnumerable<IPerson> personList, IEnumerable<ISkill> skillList,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod datePeriod,
			CreateIslands createIslands, ReduceSkillGroups reduceSkillGroups)
		{
			InitializeComponent();
			_dtpDate = new DateTimePicker {Format = DateTimePickerFormat.Short};
			toolStripMain.Items.Add(new ToolStripControlHost(_dtpDate));
			_personList = personList;
			_loadedSkillList = skillList.Where(s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
			_skillDays = skillDays;
			_datePeriod = datePeriod;
			_createIslands = createIslands;
			_reduceSkillGroups = reduceSkillGroups;
			_date = datePeriod.StartDate;
			_dtpDate.MinDate = datePeriod.StartDate.Date;
			_dtpDate.MaxDate = datePeriod.EndDate.Date;
			_dtpDate.Value = _date.Date;
			_dtpDate.ValueChanged += dtpDateValueChanged;
			listViewGroupsInIsland.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewIslands.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillGroupsForSkill.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewAllVirtualGroups.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewIslandsSkillsOnGroup.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillInSkillGroup.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewAgents.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillViewAgents.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
		}

		void dtpDateValueChanged(object sender, EventArgs e)
		{
			_date = new DateOnly(_dtpDate.Value);
			LoadData();
		}

		private void drawIslandList()
		{
			listViewIslands.Items.Clear();
			listViewIslands.Groups.Clear();
			listViewIslands.SuspendLayout();
			var islandNumber = 0;		
			foreach (var island in _islandList)
			{			
				var notLoadedSkills = 0;
				var loadedSkills = 0;
				var personsInIsland = 0;
				foreach (var groupKey in island.GroupKeys)
				{
					personsInIsland += _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(groupKey).Count();
				}
				foreach (var guidString in island.SkillGuidStrings)
				{
					Guid guid;
					if (!Guid.TryParse(guidString, out guid))
						continue;

					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);
					if (skill == null)
						notLoadedSkills++;
					else
					{
						loadedSkills++;					
					}
				}
				islandNumber++;
				var item = new ListViewItem("Island " + islandNumber);
				item.Tag = island;
				item.SubItems.Add(island.GroupKeys.Count.ToString(CultureInfo.InvariantCulture));
				item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
				item.SubItems.Add(personsInIsland.ToString(CultureInfo.InvariantCulture).PadLeft(6));
				listViewIslands.Items.Add(item);

			}
			listViewIslands.Sort();
			listViewIslands.ResumeLayout();
			listViewIslands.Items[0].Selected = true;
		}

		private void createSkillDayForSkillsDic()
		{
			foreach (var skill in _loadedSkillList)
			{
				var skillDays = _skillDays[skill];
				var skillDay = skillDays.FirstOrDefault(s => s.CurrentDate == _date);
				var forecasted = TimeSpan.Zero;
				if (skillDay != null)
				{
					forecasted = skillDay.ForecastedDistributedDemand;
				}
				_skillDayForecastForSkills.Add(new KeyValuePair<ISkill, TimeSpan>(skill, forecasted));
				_totalForecastedForDate = _totalForecastedForDate.Add(forecasted);
			}
		}

		private void drawSkillList()
		{
			listViewSkillViewSkills.SuspendLayout();
			listViewSkillViewSkills.Items.Clear();
			listViewSkillViewSkills.Groups.Clear();
			listViewSkillViewSkills.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			
			var islandNumber = 0;
			foreach (var island in _islandListBeforeReducing)
			{
				var islandModel = island.CreatExtendedClientModel();
				islandNumber ++;
				var group = new ListViewGroup("Island " + islandNumber, "Island " + islandNumber);
				listViewSkillViewSkills.Groups.Add(group);
				foreach (var skill in islandModel.SkillsInIsland)
				{
					var item = createSkillItem(skill);
					listViewSkillViewSkills.Items.Add(item);
					group.Items.Add(item);
				}
			}

			autoResizeColumns(listViewSkillViewSkills);
			listViewSkillViewSkills.ResumeLayout();
			listViewSkillViewSkills.Groups[0].Items[0].Selected = true;
		}

		private void autoResizeColumns(ListView listView)
		{
			//listView.SuspendLayout();
			listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			var columnWidths = new Dictionary<int, int>();
			for (int i = 0; i < listView.Columns.Count; i++)
			{
				columnWidths.Add(i, listView.Columns[i].Width);
			}
			listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			for (int i = 0; i < listView.Columns.Count; i++)
			{
				if(columnWidths[i] > listView.Columns[i].Width)
					listView.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
			//listView.ResumeLayout();
		}

		private void drawVirtualGroupList(string filterGuid, ListView listView)
		{
			listView.Items.Clear();
			listView.SuspendLayout();

			var islandCounter = 0;
			foreach (var island in _islandListBeforeReducing)
			{
				islandCounter++;
				var islandModel = island.CreatExtendedClientModel();
				var skillGroupCounter = 0;
				foreach (var skillGroupModel in islandModel.SkillGroupsInIsland)
				{
					skillGroupCounter++;
					int loadedSkills = 0;
					int notLoadedSkills = 0;
					foreach (var skill in skillGroupModel.SkillsInSkillGroup)
					{
						var loadedSkill = _loadedSkillList.FirstOrDefault(s => s.Id == skill.Id);
						if (loadedSkill == null)
							notLoadedSkills++;
						else
							loadedSkills++;
					}

					var item = new ListViewItem(islandCounter + ";" + skillGroupCounter) {Tag = skillGroupModel};
					item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
					item.SubItems.Add(skillGroupModel.AgentsInSkillGroup.Count().ToString(CultureInfo.InvariantCulture).PadLeft(6));
					listView.Items.Add(item);
				}
			}

			autoResizeColumns(listView);
			listViewIslands.Sort();
			listView.ResumeLayout(true);
			listView.Items[0].Selected = true;
		}

		private void listViewAllVirtualGroupsSelectedIndexChanged(object sender, EventArgs e)
		{
			listViewSkillInSkillGroup.SuspendLayout();
			listViewAgents.SuspendLayout();
			listViewSkillInSkillGroup.Items.Clear();			
			listViewAgents.Items.Clear();
			if (listViewAllVirtualGroups.SelectedItems.Count == 0)
				return;

			var selectedItem = listViewAllVirtualGroups.SelectedItems[0];
			var selectedGroup = selectedItem.Tag as SkillGroupExtendedModel;
			if (selectedGroup == null)
				return;

			fillSkillListView(selectedGroup, listViewSkillInSkillGroup);

			foreach (var person in selectedGroup.AgentsInSkillGroup)
			{
				listViewAgents.Items.Add(person.Name.ToString());
			}

			listViewSkillInSkillGroup.Sort();			
			listViewAgents.Sort();
			listViewSkillInSkillGroup.ResumeLayout();
			listViewAgents.ResumeLayout();

			Refresh();
		}

		private void fillSkillListView(SkillGroupExtendedModel skillGroup, ListView view)
		{
			foreach (var skill in skillGroup.SkillsInSkillGroup)
			{
				var item = createSkillItem(skill);
				if(item != null)
					view.Items.Add(item);
			}
		}

		private ListViewItem createSkillItem(ISkill skillToDisplay)
		{
			var color = Color.Black;
			var skill = _loadedSkillList.FirstOrDefault(s => s.Id == skillToDisplay.Id);

			if (skill == null)
			{
				skill = _allSkills.FirstOrDefault(s => s.Id == skillToDisplay.Id);
				if (skill == null)
					return null;
				color = Color.Red;
			}

			var item = new ListViewItem(skill.Name);
			item.Tag = skill;
			if (color == Color.Black)
			{
				//var agentCount = _skillGroupsCreatorResult.GetPersonsForSkillKey(guidString).Count();
				var agentCount = -100;
				item.SubItems.Add(agentCount.ToString(CultureInfo.InvariantCulture).PadLeft(8));
				var agentPercent = new Percent(agentCount/(double) _personList.Count());
				item.SubItems.Add(Math.Round(agentPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
				var forecastForSkill = _skillDayForecastForSkills[skill];
				item.SubItems.Add(_timeFormatter.GetLongHourMinuteTimeString(forecastForSkill).PadLeft(7));
				var forecastPercent = _totalForecastedForDate == TimeSpan.Zero
					? new Percent()
					: new Percent(forecastForSkill.TotalMinutes/_totalForecastedForDate.TotalMinutes);
				item.SubItems.Add(Math.Round(forecastPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
				item.SubItems.Add(skill.SkillType.Description.Name);
				item.SubItems.Add(skill.Activity.Name);
			}
			else
			{
				item.SubItems.Add(" ".PadLeft(8));
				item.SubItems.Add(" ".PadLeft(5));
				item.SubItems.Add(" ".PadLeft(7));
				item.SubItems.Add(" ".PadLeft(5));
				item.SubItems.Add(skill.SkillType.Description.Name);
				item.SubItems.Add(skill.Activity.Name);
			}
			item.ForeColor = color;

			return item;
		}

		private void sortListView(object sender, ColumnClickEventArgs e)
		{
			var listView = sender as ListView;
			if (listView == null)
				return;

			if (((listViewItemComparer) listView.ListViewItemSorter).Column == e.Column)
			{
				listView.Sorting = listView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			}
			else
			{
				listView.Sorting = SortOrder.Ascending;
			}

			listView.ListViewItemSorter = new listViewItemComparer(e.Column, listView.Sorting);
		}

		class listViewItemComparer : IComparer
		{
			private readonly SortOrder _orderBy;

			public listViewItemComparer(int column, SortOrder orderBy)
			{
				Column = column;
				_orderBy = orderBy;
			}

			public int Compare(object x, object y)
			{
				var ix = x as ListViewItem;
				var iy = y as ListViewItem;
				if (iy == null || ix == null)
					return 0;

				if(ix.SubItems.Count <= Column || iy.SubItems.Count <= Column)
					return 0;

				int returnVal = String.CompareOrdinal(ix.SubItems[Column].Text, iy.SubItems[Column].Text);

				if (_orderBy == SortOrder.Descending)
					returnVal *= -1;

				return returnVal;
			}

			public int Column { get; private set; }
		}

		private void listViewSkillViewSkillsSelectedIndexChanged(object sender, EventArgs e)
		{			
			if (listViewSkillViewSkills.SelectedItems.Count == 0)
				return;

			var selectedItem = listViewSkillViewSkills.SelectedItems[0];
			var key = selectedItem.Tag as string;
			if (key == null)
				return;

			drawVirtualGroupList(key, listViewSkillGroupsForSkill);

			listViewSkillViewAgents.SuspendLayout();
			listViewSkillViewAgents.Items.Clear();

			foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillKey(key))
			{
				listViewSkillViewAgents.Items.Add(person.Name.ToString());
			}

			listViewSkillViewAgents.Sort();
			listViewSkillViewAgents.ResumeLayout();
		}

		private void listViewIslandsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewIslands.SelectedItems.Count == 0)
				return;

			var island = listViewIslands.SelectedItems[0].Tag as OldIsland;
			if (island == null)
				return;

			listViewGroupsInIsland.Items.Clear();
			listViewGroupsInIsland.SuspendLayout();

			foreach (var key in island.GroupKeys)
			{
				var splitted = key.Split("|".ToCharArray());
				int loadedSkills = 0;
				int notLoadedSkills = 0;
				foreach (var guidString in splitted)
				{
					Guid guid;
					if (!Guid.TryParse(guidString, out guid))
						continue;

					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);
					if (skill == null)
						notLoadedSkills++;
					else
						loadedSkills++;
				}

				var item = new ListViewItem(_skillGroupsCreatorResult.GetNameForKey(key));
				item.Tag = key;
				item.SubItems.Add(loadedSkills.ToString().PadLeft(3) + " (" + notLoadedSkills.ToString().PadLeft(3) + ")");
				item.SubItems.Add(_skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key).Count().ToString().PadLeft(6));
				listViewGroupsInIsland.Items.Add(item);
			}

			listViewGroupsInIsland.ResumeLayout();
			if (listViewGroupsInIsland.Items.Count > 0)
				listViewGroupsInIsland.Items[0].Selected = true;
		}

		private void listViewGroupsInIslandSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewGroupsInIsland.SelectedItems.Count == 0)
				return;

			var selectedItem = listViewGroupsInIsland.SelectedItems[0];
			var selectedGroup = selectedItem.Tag as SkillGroupExtendedModel;
			if (selectedGroup == null)
				return;

			listViewIslandsSkillsOnGroup.Items.Clear();
			listViewIslandsSkillsOnGroup.SuspendLayout();

			fillSkillListView(selectedGroup, listViewIslandsSkillsOnGroup);

			listViewIslandsSkillsOnGroup.Sort();
			listViewIslandsSkillsOnGroup.ResumeLayout();
		}

		public void LoadData()
		{
			Text = "Agent Skill Analyzer " + _date.ToShortDateString(CultureInfo.CurrentCulture);
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_allSkills = new SkillRepository(uow).LoadAll();
				foreach (var skill in _allSkills)
				{
					LazyLoadingManager.Initialize(skill.Activity);
					LazyLoadingManager.Initialize(skill.SkillType);
				}
			}
			
			calculate();
			drawLists();		
		}

		private void drawLists()
		{
			drawVirtualGroupList(null, listViewAllVirtualGroups);
			drawSkillList();
			drawIslandList();
		}

		private void calculate()
		{
			_skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
			_totalForecastedForDate = TimeSpan.Zero;
			_islandListBeforeReducing = _createIslands.Create(new ReduceNoSkillGroups(), _personList, _date.ToDateOnlyPeriod()).ToList();
			var creator = new VirtualSkillGroupsCreator(new PersonalSkillsProvider());
			_skillGroupsCreatorResult = creator.GroupOnDate(_date, _personList);
			createSkillDayForSkillsDic();
			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			_islandList = skillGroupIslandsAnalyzer.FindIslands(_skillGroupsCreatorResult);
		}

		private void toolStripButtonRemoveNotLoadedSkillsClick(object sender, EventArgs e)
		{
			foreach (var key in _skillGroupsCreatorResult.GetKeys())
			{
				var splitted = key.Split("|".ToCharArray());
				foreach (var guidString in splitted)
				{
					Guid guid;
					if (!Guid.TryParse(guidString, out guid))
						continue;

					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);

					if (skill == null)
					{
						skill = _allSkills.FirstOrDefault(s => s.Id == guid);
						if (skill == null)
							continue;

						foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key))
						{
							inactivatePersonalSkillFor(person, guidString);
						}
					}
				}
			}
			LoadData();
		}

		private void toolStripButtonFindAgentsThatHaveChangedSkillGroupDuringPeriodClick(object sender, EventArgs e)
		{
			var results = new PersonsThatChangedPersonSkillsDuringPeriodFinder().Find(_datePeriod, _personList);
			makeReport(results);
		}

		private void makeReport(IList<PersonsThatChangedPersonSkillsDuringPeriodFinder.PersonsThatChangedPersonSkillsDuringPeriodFinderResult> results)
		{
			using (var x = new PersonsThatChangedPersonSkillsDuringPeriodFinderView(results))
			{
				x.ShowDialog(this);
			}
		}

		private void toolStripMenuItemRemoveSkillClick(object sender, EventArgs e)
		{
			var selectedGroupKey = string.Empty;
			var selectedSkillKey = string.Empty;
			if (tabControl1.SelectedIndex == 2)
			{
				selectedGroupKey = listViewGroupsInIsland.SelectedItems[0].Tag as string;
				selectedSkillKey = listViewIslandsSkillsOnGroup.SelectedItems[0].Tag as string;
			}
			if (tabControl1.SelectedIndex == 1)
			{
				selectedGroupKey = listViewAllVirtualGroups.SelectedItems[0].Tag as string;
				selectedSkillKey = listViewSkillInSkillGroup.SelectedItems[0].Tag as string;
			}

			foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(selectedGroupKey))
			{
				inactivatePersonalSkillFor(person, selectedSkillKey);
			}
			calculate();
			drawLists();
		}

		private void inactivatePersonalSkillFor(IPerson person, string selectedSkillKey)
		{
			Guid guid;
			if (!Guid.TryParse(selectedSkillKey, out guid))
				return;

			var skill = _allSkills.FirstOrDefault(s => s.Id == guid);
			if (skill == null)
				return;

			var period = person.Period(_date);
			var personSkills = period.PersonSkillCollection.Where(ps => ps.Skill.Equals(skill)).ToList();
			if (personSkills.Any())
			{
				var personSkill = personSkills.First();
				((IPersonSkillModify)personSkill).Active = false;
				_modifiedPersonSkills.Add(personSkill);
			}
		}

		private void toolStripButtonReduceAnders_Click(object sender, EventArgs e)
		{
			reduce(true);
		}

		private void toolStripButtonReduceMicke_Click(object sender, EventArgs e)
		{
			reduce(false);
		}

		private void reduce(bool desc)
		{		
			var agentsAffected = new HashSet<IPerson>();
			var currentIslandsCount = _islandList.Count;
			while (true)
			{
				if (_islandList.Count > currentIslandsCount)
				{				
					currentIslandsCount = _islandList.Count;
					drawLists();				
				}
				OldIsland largestIsland = null;
				var maxAgents = 0;
				foreach (var island in _islandList)
				{
					var agentCount = island.AgentsInIsland().Count();
					if (agentCount > maxAgents)
					{
						largestIsland = island;
						maxAgents = agentCount;
					}
				}
				if (maxAgents < 500)
					break;

				var reducer = new AgentSkillReducer();
				var affectedPersons = reducer.ReduceOne(largestIsland, _skillGroupsCreatorResult, _loadedSkillList, _date, desc, _modifiedPersonSkills).ToList();
				if (!affectedPersons.Any())
					break;

				agentsAffected.UnionWith(affectedPersons);
				calculate();
			}
			drawLists();
			MessageBox.Show(agentsAffected.Count + " agents affected", "Done!");
		}

		private void toolStripButtonResetReducedClick(object sender, EventArgs e)
		{
			foreach (var modifiedPersonSkill in _modifiedPersonSkills)
			{
				((IPersonSkillModify)modifiedPersonSkill).Active = true;
			}
			_modifiedPersonSkills.Clear();
			calculate();
			drawLists();
		}
	}
}
