using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Shoveling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class CascadingSkillsAnalyzer : Form
	{
		private readonly ILifetimeScope _container;
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly DateOnly _selectedDate;

		public CascadingSkillsAnalyzer()
		{
			InitializeComponent();
		}

		public CascadingSkillsAnalyzer(ILifetimeScope container, ISchedulerStateHolder schedulerState, DateOnly selectedDate)
		{
			_container = container;
			_schedulerState = schedulerState;
			_selectedDate = selectedDate;
			InitializeComponent();
		}

		private void CascadingSkillsAnalyzer_Load(object sender, EventArgs e)
		{
			var timeList = new List<TimeSpan>();
			for (int i = 0; i < 24; i++)
			{
				timeList.Add(TimeSpan.FromHours(i));
			}
			comboBoxIntervals.DisplayMember = "TotalHours";
			comboBoxIntervals.DataSource = timeList;
		}

		private void buttonGo_Click(object sender, EventArgs e)
		{
			var shovelService = new ShovelServicePoc(_container.Resolve<ResourceCalculationContextFactory>());
			var orderedSkillList =
				_schedulerState.SchedulingResultState.Skills.Where(
					skill => skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).OrderBy(skill => skill.Name).ToList();
			var localSelectedStart = _selectedDate.Date.AddHours(((TimeSpan) comboBoxIntervals.SelectedItem).TotalHours);
			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localSelectedStart, localSelectedStart.AddHours(1), TimeZoneGuard.Instance.TimeZone);

			var result =
				shovelService.Analyze(_schedulerState.SchedulingResultState.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
					period, orderedSkillList);

			var report = new StringBuilder();
			foreach (var shovelResult in result)
			{
				report.Append(shovelResult.ResourcesOnSkillGroup.ToString().PadLeft(3));
				report.Append(" ");
				report.Append(shovelResult.PrimarySkill.Name.Truncate(20).PadRight(20));			
				foreach (var pair in shovelResult.TransferDictionary)
				{
					report.Append(" => ");
					report.Append(pair.Key.Name.Truncate(20).PadRight(20));
					report.Append(" ");
					report.Append(Math.Round(pair.Value, 2).ToString().PadLeft(6));
				}
				report.AppendLine();
			}
			textBoxReport.Text = report.ToString();
		}
	}
}
