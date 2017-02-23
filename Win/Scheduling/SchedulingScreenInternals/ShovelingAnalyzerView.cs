using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using ResourceCalculationData = Teleopti.Ccc.Domain.ResourceCalculation.ResourceCalculationData;
using TrackShoveling = Teleopti.Ccc.Domain.ResourceCalculation.TrackShoveling;

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
			output.AppendLine($"Resources at start: {trackShovling.ResourcesBeforeShoveling}");
			output.AppendLine();
			output.AppendLine("---ADDED RESOURCES---");
			foreach (var addedResource in trackShovling.AddedResources)
			{
				var addedLine = $"{addedResource.ResourcesMoved} added resources " +
												$"from skillgroup: {string.Join("::", addedResource.FromPrimarySkills.Select(x => x.Name))}";
				addedLine += addedResource.ParallellSkills.Any()
					? $", parallell skills:  {string.Join("::", addedResource.ParallellSkills.Select(x => x.Name))}"
					: " (no parallel skills)";
				output.AppendLine(addedLine);
			}
			output.AppendLine();
			output.AppendLine("---REMOVED RESOURCES---");
			foreach (var removedResource in trackShovling.RemovedResources)
			{
				output.AppendLine($"{removedResource.ResourcesMoved} removed resources");
			}
			return output.ToString();
		}
	}
}
