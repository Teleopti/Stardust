using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday.Domain;

namespace Teleopti.Ccc.DomainTest.Intraday.Domain
{
	public class IntradayReforecastingServiceTest
	{
		[Test]
		public void CalculateAverageDevation()
		{
			Guid skillId = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");

			double[] forcastedVolume = { 70.985, 109.5371, 152.4953, 190.4355, 202.4295, 241.9608, 248.325, 260.6861, 253.3429, 264.4802, 257.9936, 270.2324, 263.7458, 261.91, 253.3429, 242.5727, 232.4145, 218.8295, 199.8594, 178.1967, 148.3341, 149.3132, 125.5699, 113.5759, 99.5013, 87.7521, 75.5133, 69.2715, 59.97, 50.4238 };
			double[] actualVolume = { 42, 84, 123, 163, 192, 196, 239, 241, 230, 242, 200, 207 };

			var opensAt = new DateTime(2018, 3, 28, 8, 0, 0);
			var closeAt = new DateTime(2018, 3, 28, 22, 30, 0);
			var intervalLength = 30;

			var timesSeries = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();

			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			var actualVolumeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timesSeries.Count; i++)
			{
				forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume[i], SkillId = skillId, StartTime = timesSeries[i] });
				if (i < actualVolume.Length)
				{
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume[i], SkillId = skillId, StartTime = timesSeries[i] });
				}
			}
			var provider = new IntradayReforecastingService();
			var averageDeviation = provider.CalculateAverageDeviation(forcastedVolmeList, actualVolumeList);

//			Assert.AreEqual(0.826045978152116, Math.Round(averageDeviation, 3));
			Assert.AreEqual(0.826, Math.Round(averageDeviation, 3));
		}

		[Test]
		public void CalculateAverageDevation_NoDeviations_ReturnsZero()
		{
			Guid skillId = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");

			double[] actualVolume = { 42, 84, 123, 163, 192, 196, 239, 241, 230, 242, 200, 207 };

			var opensAt = new DateTime(2018, 3, 28, 8, 0, 0);
			var closeAt = new DateTime(2018, 3, 28, 22, 30, 0);
			var intervalLength = 30;

			var timesSeries = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();

			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			var actualVolumeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timesSeries.Count; i++)
			{
				if (i < actualVolume.Length)
				{
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume[i], SkillId = skillId, StartTime = timesSeries[i] });
				}
			}
			var provider = new IntradayReforecastingService();
			var averageDeviation = provider.CalculateAverageDeviation(forcastedVolmeList, actualVolumeList);

			Assert.AreEqual(0.0, averageDeviation);
		}
				
		internal class ListOfDoubleComparer : IEqualityComparer<IList<double?>>
		{

			public bool Equals(IList<double?> x, IList<double?> y)
			{
				return !x.Except(y).Any() && !y.Except(x).Any();
			}

			public int GetHashCode(IList<double?> obj)
			{
				return obj.GetHashCode();
			}
		}

		[Test]
		public void ReforcastNeededAgents_WithOneSkill()
		{
			Guid skillId = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");

			double[] forcastedVolume = { 70.985, 109.5371, 152.4953, 190.4355, 202.4295, 241.9608, 248.325, 260.6861, 253.3429, 264.4802, 257.9936, 270.2324, 263.7458, 261.91, 253.3429, 242.5727, 232.4145, 218.8295, 199.8594, 178.1967, 148.3341, 149.3132, 125.5699, 113.5759, 99.5013, 87.7521, 75.5133, 69.2715, 59.97, 50.4238 };
			double[] forcastedAgents = { 14.044, 27.88, 37.085, 45.546, 54.473, 63.403, 69.589, 69.97, 70.878, 71.916, 70.899, 74.78, 71.662, 72.384, 68.243, 66.209, 65.595, 61.506, 57.025, 50.579, 41.471, 42.481, 35.93, 32.611, 30.527, 28.363, 22.833, 21.855, 20.846, 16.579 };
			double[] actualVolume = { 42, 84, 123, 163, 192, 196, 239, 241, 230, 242, 200, 207 };

			var opensAt = new DateTime(2018, 3, 28, 8, 0, 0);
			var closeAt = new DateTime(2018, 3, 28, 23, 0, 0);
			var intervalLength = 30;

			var timeSeries = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();

			var forecastedAgentsList = new List<StaffingInterval>();
			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			var actualVolumeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timeSeries.Count; i++)
			{
				if (i < forcastedVolume.Length)
					forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume[i], SkillId = skillId, StartTime = timeSeries[i] });
				if (i < forcastedAgents.Length)
					forecastedAgentsList.Add(new StaffingInterval { Agents = forcastedAgents[i], SkillId = skillId, StartTime = timeSeries[i] });
				if (i < actualVolume.Length)
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume[i], SkillId = skillId, StartTime = timeSeries[i] });
			}

			var provider = new IntradayReforecastingService();
			var reforecastedAgents = provider.ReforecastAllSkills(forecastedAgentsList, forcastedVolmeList, actualVolumeList, timeSeries, new DateTime(2018, 3, 28, 13, 38, 14));

			var reforecastedValues = reforecastedAgents
				.OrderBy(x => x.StartTime)
				.Select(x => x.Agents)
				.ToList();

			Assert.AreEqual(forcastedVolmeList.Count - actualVolumeList.Count, reforecastedValues.Count());

			var expectedReturn = new double?[]
				{59.196106886336942,59.792512082562766,56.371855687034845,54.691678167473448,54.184485936888045,50.80678393222405,47.105271904124415,41.780579528955876,34.2569527599464,35.091259197880042,29.679831995005529,26.938185393518651,25.216705575049644,23.429142078328464,18.861107819147264,18.053234852514496,17.21975446055901,13.695016271783931};
			var listOfDoubleComparer = new ListOfDoubleComparer();
			Assert.That(reforecastedValues, Is.EqualTo(expectedReturn).Using(listOfDoubleComparer));
		}

		[Test]
		public void ReforecastNeededAgents_WithNoForecasts_ReturnsEmptyArray()
		{
			Guid skillId = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");

			double[] forcastedVolume = { 70.985, 109.5371, 152.4953, 190.4355, 202.4295, 241.9608, 248.325, 260.6861, 253.3429, 264.4802, 257.9936, 270.2324, 263.7458, 261.91, 253.3429, 242.5727, 232.4145, 218.8295, 199.8594, 178.1967, 148.3341, 149.3132, 125.5699, 113.5759, 99.5013, 87.7521, 75.5133, 69.2715, 59.97, 50.4238 };
			double[] forcastedAgents = { 14.044, 27.88, 37.085, 45.546, 54.473, 63.403, 69.589, 69.97, 70.878, 71.916, 70.899, 74.78, 71.662, 72.384, 68.243, 66.209, 65.595, 61.506, 57.025, 50.579, 41.471, 42.481, 35.93, 32.611, 30.527, 28.363, 22.833, 21.855, 20.846, 16.579 };
			double[] actualVolume = { 42, 84, 123, 163, 192, 196, 239, 241, 230, 242, 200, 207 };

			var opensAt = new DateTime(2018, 3, 28, 8, 0, 0);
			var closeAt = new DateTime(2018, 3, 28, 23, 0, 0);
			var intervalLength = 30;

			var timeSeries = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();

			var forecastedAgentsList = new List<StaffingInterval>();
			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			var actualVolumeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timeSeries.Count; i++)
			{
				if (i < forcastedVolume.Length)
					forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume[i], SkillId = skillId, StartTime = timeSeries[i] });
				if (i < forcastedAgents.Length)
					forecastedAgentsList.Add(new StaffingInterval { Agents = forcastedAgents[i], SkillId = skillId, StartTime = timeSeries[i] });
				if (i < actualVolume.Length)
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume[i], SkillId = skillId, StartTime = timeSeries[i] });
			}

			var provider = new IntradayReforecastingService();

			var noForecastedAgents = provider.ReforecastAllSkills(new List<StaffingInterval>(), forcastedVolmeList, actualVolumeList, timeSeries, new DateTime(2018, 3, 28, 13, 38, 14));
			Assert.That(noForecastedAgents, Is.TypeOf(typeof(List<StaffingInterval>)));
			Assert.That(noForecastedAgents.Count(), Is.EqualTo(0));
		}

		[Test]
		public void ReforcastNeededAgents_WithNoActuals_ReturnsEmptyArray()
		{
			Guid skillId = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");

			double[] forcastedVolume = { 70.985, 109.5371, 152.4953, 190.4355, 202.4295, 241.9608, 248.325, 260.6861, 253.3429, 264.4802, 257.9936, 270.2324, 263.7458, 261.91, 253.3429, 242.5727, 232.4145, 218.8295, 199.8594, 178.1967, 148.3341, 149.3132, 125.5699, 113.5759, 99.5013, 87.7521, 75.5133, 69.2715, 59.97, 50.4238 };
			double[] forcastedAgents = { 14.044, 27.88, 37.085, 45.546, 54.473, 63.403, 69.589, 69.97, 70.878, 71.916, 70.899, 74.78, 71.662, 72.384, 68.243, 66.209, 65.595, 61.506, 57.025, 50.579, 41.471, 42.481, 35.93, 32.611, 30.527, 28.363, 22.833, 21.855, 20.846, 16.579 };

			var opensAt = new DateTime(2018, 3, 28, 8, 0, 0);
			var closeAt = new DateTime(2018, 3, 28, 23, 0, 0);
			var intervalLength = 30;

			var timeSeries = Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();

			var forecastedAgentsList = new List<StaffingInterval>();
			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timeSeries.Count; i++)
			{
				forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume[i], SkillId = skillId, StartTime = timeSeries[i] });
				forecastedAgentsList.Add(new StaffingInterval { Agents = forcastedAgents[i], SkillId = skillId, StartTime = timeSeries[i] });
			}

			var provider = new IntradayReforecastingService();

			var noForecastedAgents = provider.ReforecastAllSkills(forecastedAgentsList, forcastedVolmeList, new List<SkillIntervalStatistics>(), timeSeries, new DateTime(2018, 3, 28, 13, 38, 14));
			Assert.That(noForecastedAgents, Is.TypeOf(typeof(List<StaffingInterval>)));
			Assert.That(noForecastedAgents.Count(), Is.EqualTo(0));
		}

		public IList<DateTime> GenerateTimes(DateTime opensAt, DateTime closeAt, int intervalLength)
		{
			return Enumerable
				.Range(0, (int)Math.Ceiling((decimal)(closeAt - opensAt).TotalMinutes / intervalLength))
				.Select(offset => opensAt.AddMinutes(offset * intervalLength))
				.ToList();
		}

		[Test]
		public void ReforcastNeededAgents_WithTwoSkills()
		{
			Guid skillId1 = new Guid("49f23006-abc3-4ae3-a2e0-a7fa01455daa");
			Guid skillId2 = new Guid("bdb1ee47-11ac-4cc3-954b-a879014f670f");

			double[] forcastedVolume_skill1 = {70.985,109.5371,152.4953,190.4355,202.4295,241.9608,248.325,260.6861,253.3429,264.4802,257.9936,270.2324,263.7458,261.91,253.3429,242.5727,232.4145,218.8295,199.8594,178.1967,148.3341,149.3132,125.5699,113.5759,99.5013,87.7521,75.5133,69.2715,59.97,50.4238};
			double[] forcastedAgents_skill1 = {14.044,27.88,37.085,45.546,54.473,63.403,69.589,69.97,70.878,71.916,70.899,74.78,71.662,72.384,68.243,66.209,65.595,61.506,57.025,50.579,41.471,42.481,35.93,32.611,30.527,28.363,22.833,21.855,20.846,16.579};
			double[] actualVolume_skill1 = {42, 84, 123, 163, 192, 196, 239, 241, 230, 242, 200, 207};
			double[] forcastedVolume_skill2 = {79.7521,151.5634,231.2004,350.771,480.0084,590.9478,672.5412,778.8772,827.442,885.6736,892.1183,921.4643,894.0747,908.8052,863.4628,875.2012,860.7008,862.6572,799.5921,751.7178,684.2796,633.5282,570.6933,518.4459,505.7869,474.1392,413.0305,392.0855,338.1119,289.6622,251.4549,203.6957,150.5277,121.9873};
			double[] forcastedAgents_skill2 = {15.707,30.364,48.177,80.39,107.454,140.593,159.529,188.841,207.397,221.122,224.969,226.968,229.612,227.153,217.742,217.405,216.8,214.331,199.715,194.604,172.983,168.217,152.403,132.347,126.396,125.507,109.333,103.691,92.261,81.051,72.366,57.105,43.734,39.84 };
			double[] actualVolume_skill2 = {70,160,242,316,437,602,675,738,804,806,860,806,809,863};

			var timeSeries_skill1 = this.GenerateTimes(new DateTime(2018, 3, 28, 8, 0, 0), new DateTime(2018, 3, 28, 23, 0, 0), 30);
			var timeSeries_skill2 = this.GenerateTimes(new DateTime(2018, 3, 28, 7, 0, 0), new DateTime(2018, 3, 29, 0, 0, 0), 30);
			var timeSeries_total = this.GenerateTimes(new DateTime(2018, 3, 28, 7, 0, 0), new DateTime(2018, 3, 29, 0, 0, 0), 30);

			var forecastedAgentsList = new List<StaffingInterval>();
			var forcastedVolmeList = new List<SkillIntervalStatistics>();
			var actualVolumeList = new List<SkillIntervalStatistics>();
			for (int i = 0; i < timeSeries_skill1.Count; i++)
			{
				forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume_skill1[i], SkillId = skillId1, StartTime = timeSeries_skill1[i] });
				forecastedAgentsList.Add(new StaffingInterval { Agents = forcastedAgents_skill1[i], SkillId = skillId1, StartTime = timeSeries_skill1[i] });
				if (i < 12)
				{
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume_skill1[i], SkillId = skillId1, StartTime = timeSeries_skill1[i] });
				}
			}
			for (int i = 0; i < timeSeries_skill2.Count; i++)
			{
				forcastedVolmeList.Add(new SkillIntervalStatistics { Calls = forcastedVolume_skill2[i], SkillId = skillId2, StartTime = timeSeries_skill2[i] });
				forecastedAgentsList.Add(new StaffingInterval { Agents = forcastedAgents_skill2[i], SkillId = skillId2, StartTime = timeSeries_skill2[i] });
				if (i < 14)
				{
					actualVolumeList.Add(new SkillIntervalStatistics { Calls = actualVolume_skill2[i], SkillId = skillId2, StartTime = timeSeries_skill2[i] });
				}
			}

			var provider = new IntradayReforecastingService();
			var reforecastedAgents = provider.ReforecastAllSkills(forecastedAgentsList, forcastedVolmeList, actualVolumeList, timeSeries_total, new DateTime(2018, 3, 28, 13, 38, 14));

			var reforecastedValues = reforecastedAgents
				.GroupBy(x => x.StartTime)
				.OrderBy(x => x.Key)
				.Select(v => v.Sum(x => x.Agents))
				.ToArray();

			Assert.AreEqual(20, reforecastedValues.Count());
			//Assert.AreEqual(20, reforecastedAgentsDataSeries.Count(x => x.HasValue));

			var expectedReturn = new double?[]
				{262.58538524884261,262.86700419892315,258.88122709444008,254.89479663205171,240.73503534016268,232.58323094959809,208.68589272592874,198.90935688153655,176.6141286391105,158.71445005422626,147.74429005962912,144.17224294531388,127.34289147950462,120.28522709034533,105.04061480908551,93.76166225999421,84.8156639221286,67.0358726104365,40.851221628835148,37.213899247560079};
			var listOfDoubleComparer = new ListOfDoubleComparer();
			Assert.That(reforecastedValues, Is.EqualTo(expectedReturn).Using(listOfDoubleComparer));
		}
	}
}
