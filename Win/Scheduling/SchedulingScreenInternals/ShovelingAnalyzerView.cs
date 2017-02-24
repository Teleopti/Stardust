using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class ShovelingAnalyzerView : Form
	{
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ITimeZoneGuard _timeZoneGuard;

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

		public void FillForm(ISchedulingResultStateHolder schedulingResultStateHolder, ISkill skill, DateOnly date, TimeSpan timeStart)
		{
			var baseDate = TimeZoneHelper.ConvertToUtc(date.Date, _timeZoneGuard.CurrentTimeZone());
			var intervalLength = TimeSpan.FromMinutes(skill.DefaultResolution);
			var period = new DateTimePeriod(baseDate.Add(timeStart), baseDate.Add(timeStart).Add(intervalLength));
			var resCalcData = new ResourceCalculationData(schedulingResultStateHolder, false, false);
			var trackShovling = new TrackShoveling(skill, period);
			resCalcData.SetShovelingCallback(trackShovling);
			_resourceCalculation.ResourceCalculate(date.ToDateOnlyPeriod(), resCalcData);

			textBox1.Text = createOutput(trackShovling, skill, period);
		}

		private static string createOutput(TrackShoveling trackShovling, ISkill skill, DateTimePeriod period)
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
				output.AppendLine($" -> moved from skillgroup: {skillGroupAsString(addedResource.FromSkillGroup)}");
			}
			output.AppendLine();
			output.AppendLine("---REMOVED RESOURCES---");
			foreach (var removedResource in trackShovling.RemovedResources)
			{
				output.AppendLine($"Removing {removedResource.ResourcesMoved} resources");
				output.AppendLine($" -> moved to subskills: {string.Join("::", removedResource.ToSubskills.Select(x => x.Name))}");
			}
			return output.ToString();
		}

		private static string skillGroupAsString(CascadingSkillGroup skillGroup)
		{
			var allSkills = skillGroup.PrimarySkills.Union(skillGroup.SubSkillsWithSameIndex.SelectMany(x => x));
			return string.Join("::", allSkills.Select(x => x.Name));
		}
	}
}
