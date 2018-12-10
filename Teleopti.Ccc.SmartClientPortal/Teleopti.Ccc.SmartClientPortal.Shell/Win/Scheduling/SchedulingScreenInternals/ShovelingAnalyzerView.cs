using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Cascading.TrackShoveling;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public partial class ShovelingAnalyzerView : Form
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private ResourceCalculationData resourceCalculationData;
		private DateTimePeriod period;
		private DateOnly date;

		public ShovelingAnalyzerView()
		{
			InitializeComponent();
		}

		public ShovelingAnalyzerView(IResourceCalculation resourceCalculation, ITimeZoneGuard timeZoneGuard)
		{
			_resourceCalculation = resourceCalculation;
			_timeZoneGuard = timeZoneGuard;
			InitializeComponent();
		}

		public void FillForm(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly theDate, TimeSpan timeStart)
		{
			date = theDate;
			var baseDate = TimeZoneHelper.ConvertToUtc(date.Date, _timeZoneGuard.CurrentTimeZone());
			var intervalLength = TimeSpan.FromMinutes(skill.DefaultResolution);
			period = new DateTimePeriod(baseDate.Add(timeStart), baseDate.Add(timeStart).Add(intervalLength));
			resourceCalculationData = new ResourceCalculationData(schedulingResultStateHolder, false, false);
			var trackShovling = new TrackShovelingOneSkill(skill, period);
			resourceCalculationData.SetShovelingCallback(trackShovling);
			_resourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), resourceCalculationData);

			textBox1.Text = createOutputForOneSkill(trackShovling, skill, period);
		}


		private void button1_Click(object sender, EventArgs e)
		{
			var skills = resourceCalculationData.Skills.OrderBy(x => x.Name);
			var trackShovling = new TrackShoveling(skills, period);
			resourceCalculationData.SetShovelingCallback(trackShovling);
			_resourceCalculation.ResourceCalculate(date, resourceCalculationData);

			var output = string.Join("",skills.Select(skill => createOutputForOneSkill(trackShovling.For(skill), skill, period)));
			textBox1.Text = output;
		}

		private static string createOutputForOneSkill(TrackShovelingOneSkill trackShovling, ISkill skill, DateTimePeriod period)
		{
			var output = new StringBuilder();
			output.AppendLine($"Skill [{skill.Name}] at UTC period [{period}]");
			output.AppendLine();
			output.AppendLine($"Resources before shoveling: {trackShovling.ResourcesBeforeShoveling}");
			output.AppendLine();
			output.AppendLine("---ADDED RESOURCES---");
			foreach (var addedResource in trackShovling.AddedResources)
			{
				output.AppendLine($"Adding {addedResource.ResourcesMoved} resources");
				output.AppendLine($" -> moved from skillgroup: {skillSetAsString(addedResource.FromSkillSet)}");
			}
			output.AppendLine();
			output.AppendLine("---REMOVED RESOURCES---");
			foreach (var removedResource in trackShovling.RemovedResources)
			{
				output.AppendLine($"Removing {removedResource.ResourcesMoved} resources");
				output.AppendLine($" -> moved to subskills: {string.Join("::", removedResource.ToSubskills.Select(x => x.Name))}");
			}
			output.AppendLine();
			output.AppendLine("------------------------------------------------");
			output.AppendLine();
			return output.ToString();
		}

		private static string skillSetAsString(CascadingSkillSet skillSet)
		{
			var allSkills = skillSet.PrimarySkills.Union(skillSet.SubSkillsWithSameIndex.SelectMany(x => x));
			return string.Join("::", allSkills.Select(x => x.Name));
		}
	}
}
