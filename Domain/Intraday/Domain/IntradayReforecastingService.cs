
using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Intraday.Domain
{
	public class IntradayReforecastingService
	{
		public IEnumerable<StaffingInterval> ReforecastAllSkills(
			IEnumerable<StaffingInterval> forecastedAgents,
			IEnumerable<SkillIntervalStatistics> forecastedVolume,
			IEnumerable<SkillIntervalStatistics> actualVolume,
			IEnumerable<DateTime> timeSeries,
			DateTime startReforecastFromTimeUtc)
		{
			if (!forecastedAgents.Any())
				return new List<StaffingInterval>();

			if (!forecastedVolume.Any())
				return new List<StaffingInterval>();

			if (!actualVolume.Any())
				return new List<StaffingInterval>();

			var skills = forecastedAgents
				.GroupBy(x => x.SkillId)
				.Select(x => x.First());

			var allSkillsReforecasted = new List<StaffingInterval>();
			foreach (var skill in skills)
			{
				var actualVolumeForThisSkill = actualVolume
					.Where(a => a.SkillId == skill.SkillId && a.StartTime <= startReforecastFromTimeUtc &&
								timeSeries.Any(t => t == a.StartTime));
				var forecastedVolumeForThisSkill = forecastedVolume
					.Where(f => f.SkillId == skill.SkillId && f.StartTime <= startReforecastFromTimeUtc &&
								timeSeries.Any(t => t == f.StartTime));

				if (forecastedVolumeForThisSkill.Any())
				{
					double averageDeviation = 0.0;
					if (!actualVolumeForThisSkill.Any())
						averageDeviation = 1.0d;
					else
						averageDeviation = this.CalculateAverageDeviation(forecastedVolumeForThisSkill, actualVolumeForThisSkill);

					var reforecastedAgents = forecastedAgents
						.Where(f => f.SkillId == skill.SkillId && f.StartTime >= startReforecastFromTimeUtc &&
									timeSeries.Any(t => t == f.StartTime))
						.Select(t =>
							new StaffingInterval
							{
								SkillId = t.SkillId,
								Agents = t.Agents * averageDeviation,
								StartTime = t.StartTime
							});

					allSkillsReforecasted.AddRange(reforecastedAgents);
				}
			}

			return allSkillsReforecasted;
		}

		public double CalculateAverageDeviation(
			IEnumerable<SkillIntervalStatistics> forecastedVolume, 
			IEnumerable<SkillIntervalStatistics> actualVolume)
		{
			if (actualVolume == null) throw new ArgumentNullException(nameof(actualVolume));
			if (forecastedVolume == null) throw new ArgumentNullException(nameof(forecastedVolume));
			var alpha = 0.2d;
			var deviations = forecastedVolume.Where(f => Math.Abs(f.Calls) >= 0.1 && actualVolume.Any(a => a.StartTime == f.StartTime))
				.Select(f =>
				{
					if (!actualVolume.Any())
						return 0;

					var corespondingActual = actualVolume.Where(a => a.StartTime == f.StartTime);
					return (corespondingActual.Sum(x => x.Calls) / f.Calls);
				});

			var result = (deviations.Any() ? deviations.Aggregate((current, next) => alpha * next + (1 - alpha) * current) : 0.0);
			return result;
		}
	}
}
