using System;

namespace Teleopti.Ccc.Domain.Forecasting.ErlangA
{
	public class NumberOfAgentsNeeded
	{

		public NumberOfAgentsNeededModel CalculateNumberOfAgentsNeeded(double callsPerInterval, double averageHandelingTimeSeconds, double averagePatienceSeconds,
			int serviceLevelSeconds, double targetServiceLevelPercentage, int intervalLengthSeconds, double minimumOccupancyZeroToOne, double maximumOccupancyZeroToOne)
		{
			if (Math.Abs(callsPerInterval) < 0.00001 || Math.Abs(averageHandelingTimeSeconds) < 0.00001 || Math.Abs(targetServiceLevelPercentage) < 0.00001)
			{
				return new NumberOfAgentsNeededModel()
				{
					NumberOfAgentsNeeded = 0,
					ServiceLevelPercentage = 1,
					Occupancy = double.NaN
				};
			}

			var erlangAModel = new ErlangAModel(callsPerInterval, averageHandelingTimeSeconds, averagePatienceSeconds, serviceLevelSeconds, intervalLengthSeconds);
			var trafficIntenisity = callsPerInterval * averageHandelingTimeSeconds / intervalLengthSeconds;
			var approxNumberOfAgents = (int)Math.Ceiling(trafficIntenisity);

			var steadyStateDistribution = erlangAModel.CalculateSteadyStateDistribution(approxNumberOfAgents);
			var approxServiceLevelPercentage = erlangAModel.ServiceLevelPercentage(approxNumberOfAgents, steadyStateDistribution);
			var approxOccupancy = erlangAModel.Occupancy(approxNumberOfAgents, steadyStateDistribution);
			var upperLimitNumberOfAgents = FindUpperLimitForNumberOfAgents(approxServiceLevelPercentage, targetServiceLevelPercentage, approxNumberOfAgents,
				erlangAModel, approxOccupancy, minimumOccupancyZeroToOne, maximumOccupancyZeroToOne);

			return InterpolateNumberOfAgentsNeeded(upperLimitNumberOfAgents, targetServiceLevelPercentage, minimumOccupancyZeroToOne, maximumOccupancyZeroToOne, erlangAModel, trafficIntenisity);
		}

		private static int FindUpperLimitForNumberOfAgents(double serviceLevelPercentage, double targetServiceLevelPercentage,
			int numberOfAgents, ErlangAModel erlangAModel, double occupancy, double minimumOccupancyZeroToOne, double maximumOccupancyZeroToOne)
		{
			int upperLevelNumberOfAgents;

			if (occupancy > minimumOccupancyZeroToOne && serviceLevelPercentage < targetServiceLevelPercentage || occupancy > maximumOccupancyZeroToOne)
			{
				while (serviceLevelPercentage < targetServiceLevelPercentage || occupancy > maximumOccupancyZeroToOne)
				{
					numberOfAgents++;
					var steadyStateDistribution = erlangAModel.CalculateSteadyStateDistribution(numberOfAgents);
					serviceLevelPercentage = erlangAModel.ServiceLevelPercentage(numberOfAgents, steadyStateDistribution);
					occupancy = erlangAModel.Occupancy(numberOfAgents, steadyStateDistribution);
				}
				upperLevelNumberOfAgents = numberOfAgents;
			}
			else
			{
				while (serviceLevelPercentage > targetServiceLevelPercentage || occupancy < minimumOccupancyZeroToOne)
				{
					numberOfAgents--;
					var steadyStateDistribution = erlangAModel.CalculateSteadyStateDistribution(numberOfAgents);
					serviceLevelPercentage = erlangAModel.ServiceLevelPercentage(numberOfAgents, steadyStateDistribution);
					occupancy = erlangAModel.Occupancy(numberOfAgents, steadyStateDistribution);
				}

				upperLevelNumberOfAgents = numberOfAgents + 1;
			}

			return upperLevelNumberOfAgents;
		}

		private static NumberOfAgentsNeededModel InterpolateNumberOfAgentsNeeded(int upperLimitNumberOfAgents, double targetServiceLevelPercentage,
			double minimumOccupancyZeroToOne, double maximumOccupancyZeroToOne, ErlangAModel erlangAModel, double trafficIntensity)
		{

			var lowerLimitNumberOfAgents = upperLimitNumberOfAgents - 1;
			var steadyStateUpper = erlangAModel.CalculateSteadyStateDistribution(upperLimitNumberOfAgents);
			var highServiceLevel = erlangAModel.ServiceLevelPercentage(upperLimitNumberOfAgents, steadyStateUpper);
			double lowServiceLevel = 0;

			if (lowerLimitNumberOfAgents > 0)
			{
				var steadyStateLower = erlangAModel.CalculateSteadyStateDistribution(lowerLimitNumberOfAgents);
				lowServiceLevel = erlangAModel.ServiceLevelPercentage(lowerLimitNumberOfAgents, steadyStateLower);
			}

			var abandonmentLow = erlangAModel.AbandonPercentage(upperLimitNumberOfAgents, steadyStateUpper);
			var numberOfAgentsNeededMaxOccupancy = (1 - abandonmentLow) * trafficIntensity / maximumOccupancyZeroToOne;
			var numberOfAgentsNeededMinOccupancy = (1 - abandonmentLow) * trafficIntensity / minimumOccupancyZeroToOne;
			var numberOfAgentsNeededServiceLevel = (targetServiceLevelPercentage - (highServiceLevel - (highServiceLevel - lowServiceLevel) * upperLimitNumberOfAgents)) /
									   (highServiceLevel - lowServiceLevel);
			return GetNumberOfAgentsModel(upperLimitNumberOfAgents, minimumOccupancyZeroToOne, maximumOccupancyZeroToOne, erlangAModel,
				lowerLimitNumberOfAgents, steadyStateUpper, highServiceLevel, lowServiceLevel, numberOfAgentsNeededMaxOccupancy,
				numberOfAgentsNeededMinOccupancy, numberOfAgentsNeededServiceLevel);
		}

		private static NumberOfAgentsNeededModel GetNumberOfAgentsModel(int upperLimitNumberOfAgents,
			double minimumOccupancyZeroToOne, double maximumOccupancyZeroToOne, ErlangAModel erlangAModel,
			int lowerLimitNumberOfAgents, System.Collections.Generic.List<double> steadyStateUpper, double highServiceLevel,
			double lowServiceLevel, double numberOfAgentsNeededMaxOccupancy, double numberOfAgentsNeededMinOccupancy,
			double numberOfAgentsNeededServiceLevel)
		{
			double numberOfAgentsNeeded;
			double occupancy;
			if (numberOfAgentsNeededServiceLevel > numberOfAgentsNeededMaxOccupancy &&
				numberOfAgentsNeededServiceLevel < numberOfAgentsNeededMinOccupancy)
			{
				numberOfAgentsNeeded = numberOfAgentsNeededServiceLevel;
				if (lowerLimitNumberOfAgents < 1)
				{
					occupancy = erlangAModel.Occupancy(numberOfAgentsNeeded, steadyStateUpper);
				}
				else
				{
					var numberOfAgentsInt = (int)Math.Round(numberOfAgentsNeeded);
					if (numberOfAgentsInt == upperLimitNumberOfAgents)
					{
						occupancy = erlangAModel.Occupancy(numberOfAgentsInt, steadyStateUpper);
					}
					else
					{
						var steadyStateLower = erlangAModel.CalculateSteadyStateDistribution(lowerLimitNumberOfAgents);
						occupancy = erlangAModel.Occupancy(numberOfAgentsInt, steadyStateLower);
					}
				}
			}
			else if (numberOfAgentsNeededServiceLevel < numberOfAgentsNeededMaxOccupancy)
			{
				numberOfAgentsNeeded = numberOfAgentsNeededMaxOccupancy;
				occupancy = maximumOccupancyZeroToOne;
			}
			else
			{
				numberOfAgentsNeeded = numberOfAgentsNeededMinOccupancy;
				occupancy = minimumOccupancyZeroToOne;
			}

			var serviceLevelPercentage = (highServiceLevel - lowServiceLevel) * numberOfAgentsNeeded + highServiceLevel -
										 (highServiceLevel - lowServiceLevel) * upperLimitNumberOfAgents;
			return new NumberOfAgentsNeededModel()
			{
				NumberOfAgentsNeeded = numberOfAgentsNeeded,
				Occupancy = occupancy,
				ServiceLevelPercentage = serviceLevelPercentage
			};
		}

		public class NumberOfAgentsNeededModel
		{
			public double NumberOfAgentsNeeded;
			public double ServiceLevelPercentage;
			public double Occupancy;
		}
	}
}