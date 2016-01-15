using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.DayOffPlanning;
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
		private VirtualSkillGroupsCreatorResult _skillGroupsCreatorResult;
		private IList<SkillGroupIslandsAnalyzer.Island> _islandList;
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
					personsInIsland += _skillGroupsCreatorResult.GetPersonsForKey(groupKey).Count();
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
			listViewSkillViewSkills.Items.Clear();
			listViewSkillViewSkills.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillViewSkills.SuspendLayout();
			foreach (var skill in _loadedSkillList)
			{
				var guidString = skill.Id.ToString();
				var item = new ListViewItem(skill.Name);
				item.Tag = guidString;
				var agentCount = _skillGroupsCreatorResult.GetPersonsForSkillKey(guidString).Count();
				item.SubItems.Add(agentCount.ToString(CultureInfo.InvariantCulture).PadLeft(8));
				var agentPercent = new Percent(agentCount / (double)_personList.Count());
				item.SubItems.Add(Math.Round(agentPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
				var forecastForSkill = _skillDayForecastForSkills[skill];
				item.SubItems.Add(_timeFormatter.GetLongHourMinuteTimeString(forecastForSkill).PadLeft(7));
				var forecastPercent = _totalForecastedForDate == TimeSpan.Zero
					? new Percent()
					: new Percent(forecastForSkill.TotalMinutes / _totalForecastedForDate.TotalMinutes);
				item.SubItems.Add(Math.Round(forecastPercent.ValueAsPercent(), 2).ToString("F").PadLeft(5));
				item.SubItems.Add(skill.SkillType.Description.Name);
				item.SubItems.Add(skill.Activity.Name);
				listViewSkillViewSkills.Items.Add(item);
			}
			listViewSkillViewSkills.ResumeLayout();
		}

		private void drawVirtualGroupList(string filterGuid)
		{
			listView1.Items.Clear();
			listView1.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listView1.SuspendLayout();
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
				item.SubItems.Add(_skillGroupsCreatorResult.GetPersonsForKey(key).Count().ToString(CultureInfo.InvariantCulture).PadLeft(6));
				listView1.Items.Add(item);
			}
			listView1.ResumeLayout(true);
			listView1.Items[0].Selected = true;
		}

		private void listView1SelectedIndexChanged(object sender, EventArgs e)
		{			
			listView2.Items.Clear();			
			listView3.Items.Clear();
			if (listView1.SelectedItems.Count == 0)
				return;

			var selectedItem = listView1.SelectedItems[0];
			var key = selectedItem.Tag as string;
			if (key == null)
				return;

			listView2.SuspendLayout();
			listView3.SuspendLayout();

			fillSkillListView(key, listView2);

			foreach (var person in _skillGroupsCreatorResult.GetPersonsForKey(key))
			{
				listView3.Items.Add(person.Name.ToString());
			}

			listView2.ResumeLayout();
			listView3.ResumeLayout();

			Refresh();
		}

		private void fillSkillListView(string key, ListView view)
		{
			var splitted = key.Split("|".ToCharArray());
			foreach (var guidString in splitted)
			{
				Guid guid;
				if (!Guid.TryParse(guidString, out guid))
					continue;

				var color = Color.Black;
				var skill = _loadedSkillList.FirstOrDefault(s => s.Id == guid);

				if (skill == null)
				{
					skill = _allSkills.FirstOrDefault(s => s.Id == guid);
					if (skill == null)
						continue;
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
				item.ForeColor = color;
				view.Items.Add(item);
			}
		}

		private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
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

		private void toolStripButtonFilterOnSelectedSkillCheckedChanged(object sender, EventArgs e)
		{
			if (toolStripButtonFilterOnSelectedSkill.Checked)
			{
				if (listView2.SelectedItems.Count == 0)
					return;

				var guidString = listView2.SelectedItems[0].Tag as string;
				if (guidString == null)
					return;

				toolStripButtonFilterOnSelectedSkill.Text = "Filter groups on skill: " + listView2.SelectedItems[0].Text;
				drawVirtualGroupList(guidString);				
			}
			else
			{
				toolStripButtonFilterOnSelectedSkill.Text = "Filter groups on selected skill";
				drawVirtualGroupList(null);				
			}
		}

		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
			toolStripButtonFilterOnSelectedSkill.Enabled = listView2.SelectedItems.Count > 0;

			if (toolStripButtonFilterOnSelectedSkill.Checked)
			{
				if (listView2.SelectedItems.Count == 0)
					return;

				var guidString = listView2.SelectedItems[0].Tag as string;
				if (guidString == null)
					return;

				drawVirtualGroupList(guidString);
			}
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
				int returnVal = String.CompareOrdinal(((ListViewItem)x).SubItems[Column].Text, ((ListViewItem)y).SubItems[Column].Text);

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

			var island = listViewIslands.SelectedItems[0].Tag as SkillGroupIslandsAnalyzer.Island;

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
				item.SubItems.Add(_skillGroupsCreatorResult.GetPersonsForKey(key).Count().ToString().PadLeft(6));
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
			}
			_skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
			_totalForecastedForDate = TimeSpan.Zero;
			
			var creator = new VirtualSkillGroupsCreator();
			_skillGroupsCreatorResult = creator.GroupOnDate(_date, _personList);
			createSkillDayForSkillsDic();
			drawVirtualGroupList(null);
			drawSkillList();
			var skillGroupIslandsAnalyzer = new SkillGroupIslandsAnalyzer();
			_islandList = skillGroupIslandsAnalyzer.FindIslands(_skillGroupsCreatorResult);
			drawIslandList();
		}

		private void toolStripButtonSuggestAction_Click(object sender, EventArgs e)
		{
			var results = new SkillGroupReducer().SuggestAction(_skillGroupsCreatorResult, _islandList);
			var resultForm = new SkillGroupReducerSuggestions();
			resultForm.LoadData(results,_skillGroupsCreatorResult,_loadedSkillList);
			resultForm.Show(this);
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			var results = new SkillGroupReducer().SuggestAction(_skillGroupsCreatorResult, _islandList);
			foreach (var skillGroupReducerResult in results)
			{
				foreach (var person in _skillGroupsCreatorResult.GetPersonsForKey(skillGroupReducerResult.RemoveFromGroupKey))
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

						foreach (var person in _skillGroupsCreatorResult.GetPersonsForKey(key))
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
	}
}
