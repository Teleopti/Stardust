using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using EO.Internal;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.DayOffPlanning;
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
		private readonly IDictionary<ISkill, IList<ISkillDay>> _skillDays;
		private readonly DateOnlyPeriod _datePeriod;
		private VirtualSkillGroupsCreatorResult _skillGroupsCreatorResult;
		private IList<Island> _islandList;
		private IDictionary<ISkill,TimeSpan> _skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
		private TimeSpan _totalForecastedForDate;
		private readonly TimeFormatter _timeFormatter = new TimeFormatter(new ThisCulture(CultureInfo.CurrentCulture));
		private IList<ISkill> _allSkills;
		private readonly DateTimePicker _dtpDate;

		public AgentSkillAnalyzer()
		{
			InitializeComponent();
		}

		public AgentSkillAnalyzer(IEnumerable<IPerson> personList, IEnumerable<ISkill> skillList, IDictionary<ISkill, IList<ISkillDay>> skillDays, DateOnlyPeriod datePeriod)
		{		
			InitializeComponent();
			_dtpDate = new DateTimePicker {Format = DateTimePickerFormat.Short};
			toolStripMain.Items.Add(new ToolStripControlHost(_dtpDate));
			_personList = personList;
			_loadedSkillList = skillList.Where(s=> s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
			_skillDays = skillDays;
			_datePeriod = datePeriod;
			_date = datePeriod.StartDate;
			_dtpDate.MinDate = datePeriod.StartDate.Date;
			_dtpDate.MaxDate = datePeriod.EndDate.Date;
			_dtpDate.Value = _date.Date;
			_dtpDate.ValueChanged += dtpDateValueChanged;
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
			listViewIslands.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
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
			listViewSkillViewSkills.ResumeLayout();
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
			foreach (var island in _islandList)
			{
				islandNumber ++;
				var group = new ListViewGroup("Island " + islandNumber, "Island " + islandNumber);
				listViewSkillViewSkills.Groups.Add(group);
				foreach (var skillGuidString in island.SkillGuidStrings)
				{
					var item = createSkillItem(skillGuidString);
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
			listView.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listView.SuspendLayout();
			foreach (var key in _skillGroupsCreatorResult.GetKeys())
			{		
				var splitted = key.Split("|".ToCharArray());
				if (filterGuid != null && !splitted.Contains(filterGuid))
					continue;

				int loadedSkills = 0;
				int notLoadedSkills = 0;
				foreach (var guidString in splitted)
				{
					Guid guid;
					if (!Guid.TryParse(guidString, out guid))
						continue;

					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);
					if (skill == null)
						notLoadedSkills ++;
					else
						loadedSkills ++;
				}

				var item = new ListViewItem(_skillGroupsCreatorResult.GetNameForKey(key));
				item.Tag = key;
				item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
				item.SubItems.Add(_skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key).Count().ToString(CultureInfo.InvariantCulture).PadLeft(6));
				listView.Items.Add(item);
			}
			autoResizeColumns(listView);
			listView.ResumeLayout(true);
			listView.Items[0].Selected = true;
		}

		private void listViewAllVirtualGroupsSelectedIndexChanged(object sender, EventArgs e)
		{			
			listViewSkillInSkillGroup.Items.Clear();			
			listView3.Items.Clear();
			if (listViewAllVirtualGroups.SelectedItems.Count == 0)
				return;

			var selectedItem = listViewAllVirtualGroups.SelectedItems[0];
			var key = selectedItem.Tag as string;
			if (key == null)
				return;

			listViewSkillInSkillGroup.SuspendLayout();
			listView3.SuspendLayout();

			fillSkillListView(key, listViewSkillInSkillGroup);

			foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(key))
			{
				listView3.Items.Add(person.Name.ToString());
			}

			listViewSkillInSkillGroup.ResumeLayout();
			listView3.ResumeLayout();

			Refresh();
		}

		private void fillSkillListView(string key, ListView view)
		{
			var splitted = key.Split("|".ToCharArray());
			foreach (var guidString in splitted)
			{
				var item = createSkillItem(guidString);
				if(item != null)
					view.Items.Add(item);
			}
		}

		private ListViewItem createSkillItem(string guidString)
		{
			Guid guid;
			if (!Guid.TryParse(guidString, out guid))
				return null;

			var color = Color.Black;
			var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);

			if (skill == null)
			{
				skill = _allSkills.FirstOrDefault(s => s.Id == guid);
				if (skill == null)
					return null;
				color = Color.Red;
			}

			var item = new ListViewItem(skill.Name);
			item.Tag = guidString;
			if (color == Color.Black)
			{
				var agentCount = _skillGroupsCreatorResult.GetPersonsForSkillKey(guidString).Count();
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

		private void listViewAllVirtualGroupsColumnClick(object sender, ColumnClickEventArgs e)
		{
			sortListView(sender, e);
		}

		private void sortListView(object sender, ColumnClickEventArgs e)
		{
			var listView = sender as ListView; 

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

			listViewSkillViewAgents.ResumeLayout();
		}

		private void listViewSkillViewSkillsColumnClick(object sender, ColumnClickEventArgs e)
		{
			sortListView(sender, e);
		}

		private void listViewIslandsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewIslands.SelectedItems.Count == 0)
				return;

			var island = listViewIslands.SelectedItems[0].Tag as Island;

			listViewGroupsInIsland.Items.Clear();
			listViewGroupsInIsland.SuspendLayout();

			int groupNumber = 0;
			foreach (var key in island.GroupKeys)
			{
				groupNumber++;
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
			var key = selectedItem.Tag as string;
			if (key == null)
				return;

			listViewIslandsSkillsOnGroup.Items.Clear();
			listViewIslandsSkillsOnGroup.SuspendLayout();

			fillSkillListView(key, listViewIslandsSkillsOnGroup);
			
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
			_skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
			_totalForecastedForDate = TimeSpan.Zero;
			
			var creator = new VirtualSkillGroupsCreator();
			_skillGroupsCreatorResult = creator.GroupOnDate(_date, _personList);
			createSkillDayForSkillsDic();
			drawVirtualGroupList(null, listViewAllVirtualGroups);
			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			_islandList = skillGroupIslandsAnalyzer.FindIslands(_skillGroupsCreatorResult);
			drawSkillList();		
			drawIslandList();
		}

		private void toolStripButtonSuggestAction_Click(object sender, EventArgs e)
		{
			var results = new SkillGroupReducer().SuggestAction(_skillGroupsCreatorResult, _allSkills);
			var resultForm = new SkillGroupReducerSuggestions();
			resultForm.LoadData(results,_skillGroupsCreatorResult,_loadedSkillList);
			resultForm.Show(this);
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			var results = new SkillGroupReducer().SuggestAction(_skillGroupsCreatorResult, _allSkills);
			foreach (var skillGroupReducerResult in results)
			{
				foreach (var person in _skillGroupsCreatorResult.GetPersonsForSkillGroupKey(skillGroupReducerResult.RemoveFromGroupKey))
				{
					Guid guid;
					if (!Guid.TryParse(skillGroupReducerResult.SkillGuidStringToRemove, out guid))
						continue;

					var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);
					if (skill == null)
						continue;
					var period = person.Period(_date);
					var personSkill = period.PersonSkillCollection.Where(ps => ps.Skill == skill).ToList();
					if (personSkill.Any())
						((IPersonSkillModify)personSkill.First()).Active = false;
				}
			}
			LoadData();
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
							var period = person.Period(_date);
							var personSkill = period.PersonSkillCollection.Where(ps => ps.Skill.Equals(skill)).ToList();
							if (personSkill.Any())
								((IPersonSkillModify)personSkill.First()).Active = false;
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
	}
}
