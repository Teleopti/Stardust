using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public partial class AgentSkillAnalyzer : Form
	{
		private readonly IEnumerable<IPerson> _personList;
		private DateOnly _date;
		private readonly IList<ISkill> _loadedSkillList;
		private readonly IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays;
		private readonly DateOnlyPeriod _datePeriod;
		private readonly CreateIslands _createIslands;
		private IList<Island> _islandListBeforeReducing;
		private IDictionary<ISkill,TimeSpan> _skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
		private TimeSpan _totalForecastedForDate;
		private readonly TimeFormatter _timeFormatter = new TimeFormatter(new ThisCulture(CultureInfo.CurrentCulture));
		private IList<ISkill> _allSkills;
		private readonly DateTimePicker _dtpDate;

		public AgentSkillAnalyzer()
		{
			InitializeComponent();
		}

		public AgentSkillAnalyzer(IEnumerable<IPerson> personList, IEnumerable<ISkill> skillList,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod datePeriod,
			CreateIslands createIslands)
		{
			InitializeComponent();
			_dtpDate = new DateTimePicker {Format = DateTimePickerFormat.Short};
			toolStripMain.Items.Add(new ToolStripControlHost(_dtpDate));
			_personList = personList;
			_loadedSkillList = skillList.Where(s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
			_skillDays = skillDays;
			_datePeriod = datePeriod;
			_createIslands = createIslands;
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
			foreach (var island in _islandListBeforeReducing)
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
					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == islandSkill.Id);
					if (skill == null)
						notLoadedSkills++;
					else
					{
						loadedSkills++;					
					}
				}
				islandNumber++;
				var item = new ListViewItem("Island " + islandNumber);
				item.Tag = islandModel;
				item.SubItems.Add(islandModel.SkillSetsInIsland.Count().ToString(CultureInfo.InvariantCulture));
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
		}

		private void drawVirtualGroupList(ISkill filterSkill, ListView listView)
		{
			listView.Items.Clear();
			listView.SuspendLayout();

			var islandCounter = 0;
			foreach (var island in _islandListBeforeReducing)
			{
				islandCounter++;
				var islandModel = island.CreatExtendedClientModel();
				var skillGroupCounter = 0;
				foreach (var skillGroupModel in islandModel.SkillSetsInIsland)
				{
					skillGroupCounter++;
					int loadedSkills = 0;
					int notLoadedSkills = 0;
					foreach (var skill in skillGroupModel.SkillsInSkillSet)
					{
						var loadedSkill = _loadedSkillList.FirstOrDefault(s => s.Id == skill.Id);
						if (loadedSkill == null)
							notLoadedSkills++;
						else
							loadedSkills++;
					}

					if (filterSkill != null && !skillGroupModel.SkillsInSkillSet.Contains(filterSkill))
						continue;

					var item = new ListViewItem(islandCounter + ";" + skillGroupCounter) {Tag = skillGroupModel};
					item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
					item.SubItems.Add(skillGroupModel.AgentsInSkillSet.Count().ToString(CultureInfo.InvariantCulture).PadLeft(6));
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
			var selectedGroup = selectedItem.Tag as SkillSetExtendedModel;
			if (selectedGroup == null)
				return;

			fillSkillListView(selectedGroup, listViewSkillInSkillGroup);

			foreach (var person in selectedGroup.AgentsInSkillSet)
			{
				listViewAgents.Items.Add(person.Name.ToString());
			}

			listViewSkillInSkillGroup.Sort();			
			listViewAgents.Sort();
			listViewSkillInSkillGroup.ResumeLayout();
			listViewAgents.ResumeLayout();

			Refresh();
		}

		private void fillSkillListView(SkillSetExtendedModel skillSet, ListView view)
		{
			foreach (var skill in skillSet.SkillsInSkillSet)
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
				var agentCount = getPersonsOnSkill(skill).Count();
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
			var selectedSkill = selectedItem.Tag as ISkill;
			if (selectedSkill == null)
				return;

			drawVirtualGroupList(selectedSkill, listViewSkillGroupsForSkill);

			listViewSkillViewAgents.SuspendLayout();
			listViewSkillViewAgents.Items.Clear();

			foreach (var person in getPersonsOnSkill(selectedSkill))
			{
				listViewSkillViewAgents.Items.Add(person.Name.ToString());
			}

			listViewSkillViewAgents.Sort();
			listViewSkillViewAgents.ResumeLayout();
		}

		private IEnumerable<IPerson> getPersonsOnSkill(ISkill skill)
		{
			var personsOnSkill = new List<IPerson>();
			foreach (var person in _personList)
			{
				var personPeriod = person.Period(_date);
				foreach (var personSkill in personPeriod.PersonSkillCollection)
				{
					if(personSkill.Skill.Id == skill.Id && personSkill.Active)
						personsOnSkill.Add(person);
				}
			}

			return personsOnSkill;
		}

		private void listViewIslandsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewIslands.SelectedItems.Count == 0)
				return;

			var selectedIsland = listViewIslands.SelectedItems[0].Tag as IslandExtendedModel;
			if (selectedIsland == null)
				return;

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
					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == skillGroupSkill.Id);
					if (skill == null)
						notLoadedSkills++;
					else
						loadedSkills++;
				}

				var item = new ListViewItem(skillGroupCounter.ToString());
				item.Tag = skillGroup;
				item.SubItems.Add(loadedSkills.ToString().PadLeft(3) + " (" + notLoadedSkills.ToString().PadLeft(3) + ")");
				item.SubItems.Add(skillGroup.AgentsInSkillSet.Count().ToString().PadLeft(6));
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
			var selectedGroup = selectedItem.Tag as SkillSetExtendedModel;
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
			_islandListBeforeReducing = _createIslands.Create(new ReduceNoSkillSets(), _personList, _date.ToDateOnlyPeriod()).ToList();
			createSkillDayForSkillsDic();
		}

		private void toolStripButtonRemoveNotLoadedSkillsClick(object sender, EventArgs e)
		{
			MessageBox.Show("Not implemented");
			//Gör något annat dessa skills ska redan vara borttagna, indikerar bara att det finns oanvända skills för agenterna och det vill jag visa
			
			//foreach (var key in _skillGroupsCreatorResult.GetKeys())
			//{
			//	var splitted = key.Split("|".ToCharArray());
			//	foreach (var guidString in splitted)
			//	{
			//		Guid guid;
			//		if (!Guid.TryParse(guidString, out guid))
			//			continue;

			//		var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);

			//		if (skill == null)
			//		{
			//			skill = _allSkills.FirstOrDefault(s => s.Id == guid);
			//			if (skill == null)
			//				continue;

			//			foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key))
			//			{
			//				inactivatePersonalSkillFor(person, guidString);
			//			}
			//		}
			//	}
			//}
			//LoadData();
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
	}
}
