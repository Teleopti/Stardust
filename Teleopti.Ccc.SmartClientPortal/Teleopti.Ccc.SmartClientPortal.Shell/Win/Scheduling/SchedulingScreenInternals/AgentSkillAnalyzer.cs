using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public partial class AgentSkillAnalyzer : Form
	{
		private readonly IEnumerable<IPerson> _personList;
		private DateOnly _date;
		private readonly IList<ISkill> _loadedSkillList;
		private readonly IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays;
		private readonly CreateIslands _createIslands;
		private readonly DesktopContextState _desktopContextState;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private IList<Island> _islandListBeforeReducing;
		private IList<Island> _islandListAfterReducing;
		private IDictionary<ISkill, TimeSpan> _skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
		private TimeSpan _totalForecastedForDate;
		private readonly TimeFormatter _timeFormatter = new TimeFormatter(new ThisCulture(CultureInfo.CurrentCulture));
		private IList<ISkill> _allSkills;
		private IList<Island> _islandListAfterMerging;
		private AgentSkillAnalyzerIslandView _agentSkillAnalyzerIslandView;

		public AgentSkillAnalyzer()
		{
			InitializeComponent();
		}

		public AgentSkillAnalyzer(IEnumerable<IPerson> personList, IEnumerable<ISkill> skillList,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnlyPeriod datePeriod,
			CreateIslands createIslands, DesktopContextState desktopContextState,
			ISchedulerStateHolder schedulerStateHolder)
		{
			InitializeComponent();
			_personList = personList;
			_loadedSkillList = skillList.Where(s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
			_skillDays = skillDays;
			_createIslands = createIslands;
			_desktopContextState = desktopContextState;
			_schedulerStateHolder = schedulerStateHolder;
			_date = datePeriod.StartDate;
			listViewGroupsInIsland.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewIslands.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillGroupsForSkill.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewAllVirtualGroups.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewIslandsSkillsOnGroup.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillInSkillGroup.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewAgents.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
			listViewSkillViewAgents.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);
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

		private void drawSkillList(IEnumerable<Island> islands)
		{
			listViewSkillViewSkills.SuspendLayout();
			listViewSkillViewSkills.Items.Clear();
			listViewSkillViewSkills.Groups.Clear();
			listViewSkillViewSkills.ListViewItemSorter = new listViewItemComparer(0, SortOrder.Ascending);

			var islandNumber = 0;
			foreach (var island in islands)
			{
				var islandModel = island.CreatExtendedClientModel();
				islandNumber++;
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
				if (columnWidths[i] > listView.Columns[i].Width)
					listView.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
			}
		}

		private void drawVirtualGroupList(ISkill filterSkill, ListView listView, IEnumerable<Island> islands)
		{
			listView.Items.Clear();
			listView.SuspendLayout();

			var islandCounter = 0;
			foreach (var island in islands)
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

					var item = new ListViewItem(islandCounter + ";" + skillGroupCounter) { Tag = skillGroupModel };
					item.SubItems.Add(loadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + " (" + notLoadedSkills.ToString(CultureInfo.InvariantCulture).PadLeft(3) + ")");
					item.SubItems.Add(skillGroupModel.AgentsInSkillSet.Count().ToString(CultureInfo.InvariantCulture).PadLeft(6));
					listView.Items.Add(item);
				}
			}

			autoResizeColumns(listView);
			listViewIslands.Sort();
			listView.ResumeLayout(true);
			if (listView.Items.Count > 0)
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
			if (!(selectedItem.Tag is SkillSetExtendedModel selectedGroup))
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
				if (item != null)
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

			var item = new ListViewItem(skill.Name) { Tag = skill };
			if (color == Color.Black)
			{
				var agentCount = getPersonsOnSkill(skill).Count();
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
			if (!(sender is ListView listView))
				return;

			if (((listViewItemComparer)listView.ListViewItemSorter).Column == e.Column)
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
				if (!(y is ListViewItem iy) || !(x is ListViewItem ix))
					return 0;

				if (ix.SubItems.Count <= Column || iy.SubItems.Count <= Column)
					return 0;

				int returnVal = String.CompareOrdinal(ix.SubItems[Column].Text, iy.SubItems[Column].Text);

				if (_orderBy == SortOrder.Descending)
					returnVal *= -1;

				return returnVal;
			}

			public int Column { get; }
		}

		private void listViewSkillViewSkillsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewSkillViewSkills.SelectedItems.Count == 0)
				return;

			var selectedItem = listViewSkillViewSkills.SelectedItems[0];
			if (!(selectedItem.Tag is ISkill selectedSkill))
				return;

			IList<Island> islands = new List<Island>();
			switch (toolStripDropDownButtonStep.Text)
			{
				case "Before reducing":
					islands = _islandListBeforeReducing;
					break;

				case "After reducing":
					islands = _islandListAfterReducing;
					break;

				case "After merging":
					islands = _islandListAfterMerging;
					break;
			}
			drawVirtualGroupList(selectedSkill, listViewSkillGroupsForSkill, islands);

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

		private void listViewIslandsSelectedIndexChanged(object sender, EventArgs e)
		{
			if (listViewIslands.SelectedItems.Count == 0)
				return;

			if (!(listViewIslands.SelectedItems[0].Tag is IslandExtendedModel selectedIsland))
				return;

			_agentSkillAnalyzerIslandView.ListViewIslandSelectedIndexChanged(selectedIsland, _loadedSkillList, listViewGroupsInIsland);
		}

		private void listViewGroupsInIslandSelectedIndexChanged(object sender, EventArgs e)
		{
			_agentSkillAnalyzerIslandView.ListViewGroupsInIslandSelectedIndexChanged(listViewGroupsInIsland, listViewIslandsSkillsOnGroup);
		}

		public void LoadData()
		{
			Text = @"Agent Skill Analyzer " + _date.ToShortDateString(CultureInfo.CurrentCulture);
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_allSkills = new SkillRepository(uow).LoadAll().ToList();
				foreach (var skill in _allSkills)
				{
					LazyLoadingManager.Initialize(skill.Activity);
					LazyLoadingManager.Initialize(skill.SkillType);
				}
			}

			calculate();
			_agentSkillAnalyzerIslandView = new AgentSkillAnalyzerIslandView(_personList, _skillDayForecastForSkills, _totalForecastedForDate, _date);
			drawLists();
		}

		private void drawLists()
		{
			IList<Island> islands = new List<Island>();
			switch (toolStripDropDownButtonStep.Text)
			{
				case "Before reducing":
					islands = _islandListBeforeReducing;
					break;

				case "After reducing":
					islands = _islandListAfterReducing;
					break;

				case "After merging":
					islands = _islandListAfterMerging;
					break;
			}
			drawVirtualGroupList(null, listViewAllVirtualGroups, islands);
			drawSkillList(islands);
			_agentSkillAnalyzerIslandView.DrawIslandList(islands, _loadedSkillList, listViewIslands);
		}

		private void calculate()
		{
			var callback = new LogCreateIslandsCallback();
			var commandId = Guid.NewGuid();
			var command = new SchedulingCommand
			{
				CommandId = commandId,
				AgentsToSchedule = _personList.ToArray(),
				FromWeb = false,
				ScheduleWithoutPreferencesForFailedAgents = false,
				Period = _date.ToDateOnlyPeriod(),
				RunDayOffOptimization = false
			};

			var schedulingWasOrdered = new SchedulingWasOrdered { CommandId = commandId };
			using (CommandScope.Create(schedulingWasOrdered))
			{
				using (_desktopContextState.SetForScheduling(command, _schedulerStateHolder, new SchedulingOptions(), new NoSchedulingCallback()))
				{
					_createIslands.Create(_date.ToDateOnlyPeriod(), callback);
				}
			}

			_skillDayForecastForSkills = new Dictionary<ISkill, TimeSpan>();
			_totalForecastedForDate = TimeSpan.Zero;
			_islandListBeforeReducing = callback.IslandsBasic.Islands.ToList();
			_islandListAfterReducing = callback.IslandsAfterReducing.Islands.ToList();
			_islandListAfterMerging = callback.IslandsComplete.Islands.ToList();
			createSkillDayForSkillsDic();
		}

		private void toolStripDropDownButtonStepDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			toolStripDropDownButtonStep.Text = e.ClickedItem.Text;
			drawLists();
		}

		private void toolStripMenuItemFilterOnIntersectingClick(object sender, EventArgs e)
		{
			if (!(listViewIslands.SelectedItems[0].Tag is IslandExtendedModel selectedIsland))
				return;

			_agentSkillAnalyzerIslandView.FilterOnIntersecting(selectedIsland, listViewGroupsInIsland);
		}
	}
}
