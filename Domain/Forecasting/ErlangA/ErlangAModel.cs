using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Forecasting.ErlangA
{
	public class ErlangAModel
	{

		private readonly double _callsPerInterval;
		private readonly double _serviceRate;
		private readonly double _individualAbandonRate;
		private readonly int _serviceLevelSeconds;
		private readonly int _intervalLength;
		private readonly double _trafficIntensity;
		private readonly double _averageHandelingTime;
		private readonly double _averagePatience;


		public ErlangAModel(double callsPerInterval, double averageHandelingTime, double averagePatience,
			int serviceLevelSeconds, int intervalLength)
		{
			_callsPerInterval = callsPerInterval;
			_averageHandelingTime = averageHandelingTime;
			_averagePatience = averagePatience;
			_serviceRate = intervalLength / averageHandelingTime;
			_individualAbandonRate = intervalLength / averagePatience;
			_serviceLevelSeconds = serviceLevelSeconds;
			_intervalLength = intervalLength;
			_trafficIntensity = _callsPerInterval / _serviceRate;
		}

		public List<double> CalculateSteadyStateDistribution(int numberOfAgents)  //From http://ie.technion.ac.il/serveng/References/Erlang_A.pdf p.9 
		{
			var steadyStateDistribution = new List<double>();
			if (Math.Abs(_callsPerInterval) < 0.00001 || Math.Abs(_averageHandelingTime) < 0.00001)
			{
				steadyStateDistribution.Add(1);
				return steadyStateDistribution;
			}

			var blockingProbability = BlockingProbability(numberOfAgents);
			if (Math.Abs(blockingProbability) < 0.00001)
			{
				steadyStateDistribution.Add(1);
				return steadyStateDistribution;
			}

			var steadyStateN = SteadyStateN(numberOfAgents, blockingProbability);

			var numerator = MathFormulas.Factorial(numberOfAgents);
			var denominator1 = new Tuple<double, double>(1, 0);
			var denominator2 = MathFormulas.PowerOf(_callsPerInterval / _serviceRate, numberOfAgents);
			var differensInDevidedNumbers = denominator1.Item2 + denominator2.Item2 - numerator.Item2;
			var quota = steadyStateN * numerator.Item1 / denominator1.Item1 / denominator2.Item1 / (Math.Pow(10000, differensInDevidedNumbers));
			steadyStateDistribution.Add(quota);

			for (var i = 1; i <= numberOfAgents; i++)
			{
				denominator1 = MathFormulas.Multiply(denominator1, i);
				denominator2 = MathFormulas.DevidedBy(denominator2, _callsPerInterval / _serviceRate);
				differensInDevidedNumbers = denominator1.Item2 + denominator2.Item2 - numerator.Item2;

				quota = steadyStateN * numerator.Item1 / denominator1.Item1 / denominator2.Item1 / (Math.Pow(10000, differensInDevidedNumbers));

				if (!quota.Equals(Double.NaN))
				{
					steadyStateDistribution.Add(quota);
				}
				else
				{
					return steadyStateDistribution;
				}
			}

			var itteration = numberOfAgents + 1;
			var sum = steadyStateDistribution.Sum();
			numerator = new Tuple<double, double>(1, 0);
			var denominator = new Tuple<double, double>(1, 0);
			while (sum < 0.9999 && itteration < numberOfAgents + 1000)
			{
				numerator = MathFormulas.Multiply(numerator, _callsPerInterval / _individualAbandonRate);
				denominator = MathFormulas.Multiply(denominator, numberOfAgents * _serviceRate / _individualAbandonRate + itteration - numberOfAgents);

				quota = steadyStateN * numerator.Item1 / denominator.Item1 / Math.Pow(10000, denominator.Item2 - numerator.Item2);

				if (!quota.Equals(Double.NaN))
				{
					steadyStateDistribution.Add(quota);
				}
				else
				{
					return steadyStateDistribution;
				}
				sum += quota;
				itteration++;
			}
			return steadyStateDistribution;
		}

		private double SteadyStateN(int numberOfAgents, double blockingProbability)
		{
			return blockingProbability / (1 + (MethodA(numberOfAgents * _serviceRate / _individualAbandonRate, _callsPerInterval / _individualAbandonRate) - 1) * blockingProbability);
		}

		private double BlockingProbability(int numberOfAgents)
		{
			double blockingProbability = 1;

			for (int i = 1; i <= numberOfAgents; i++)
			{
				blockingProbability = _callsPerInterval / _serviceRate / i * blockingProbability / (1 + _callsPerInterval / _serviceRate / i * blockingProbability);
			}
			if (blockingProbability > 1)
				return 1;
			return blockingProbability < 0 ? 0 : blockingProbability;
		}

		private static double MethodA(double x, double y)
		{

			var itteration = 1;
			var numerator = new Tuple<double, double>(y, 0);
			var denominator = new Tuple<double, double>(x + itteration, 0);
			var quota = numerator.Item1 / denominator.Item1 * Math.Pow(10000, numerator.Item2 - denominator.Item2);
			double a = 1 + quota;
			while (quota / a > 0.00001 && itteration < 10000) //Can be adjusted for better performance
			{
				itteration++;
				numerator = MathFormulas.Multiply(numerator, y);
				denominator = MathFormulas.Multiply(denominator, x + itteration);
				quota = numerator.Item1 / denominator.Item1 * Math.Pow(10000, numerator.Item2 - denominator.Item2);
				a += quota;
			}
			return a;
		}

		public double ServiceLevelPercentage(int numberOfAgents, List<double> steadyStateDistribution)
		{
			if (Math.Abs(_averageHandelingTime) < 0.00001 || Math.Abs(_callsPerInterval) < 0.00001)
				return 1;
			if (numberOfAgents == 0)
				return 0;
			if (steadyStateDistribution.Count == 1)
				return 1;

			var serviceLevelPercentage = steadyStateDistribution.GetRange(0, numberOfAgents).Sum();
			if (Math.Abs(_averagePatience) < 0.00001)
				return serviceLevelPercentage;

			for (var k = 0; k < steadyStateDistribution.Count - numberOfAgents; k++)
			{
				if (steadyStateDistribution[k + numberOfAgents] < 0.0001)
					continue;

				var serviceIntensity = (_serviceRate * numberOfAgents + k * _individualAbandonRate / 2) / _intervalLength;

				double exponentialfactor = Math.Exp(-(serviceIntensity + _individualAbandonRate / _intervalLength) * _serviceLevelSeconds);
				double sum = exponentialfactor;
				var denominator = new Tuple<double, double>(1, 0);
				var numerator = new Tuple<double, double>(1, 0);

				if (Math.Abs(exponentialfactor) < double.Epsilon)
				{
					sum = 0;
				}
				else
				{
					for (int j = 1; j <= k; j++)
					{
						numerator = MathFormulas.ItterativPowerOf(numerator, _serviceLevelSeconds * (serviceIntensity + _individualAbandonRate / _intervalLength));
						denominator = MathFormulas.Multiply(denominator, j);

						sum += exponentialfactor * numerator.Item1 / denominator.Item1 * Math.Pow(10000, numerator.Item2 - denominator.Item2);
						if (Math.Abs(1 - sum) < 0.0001)
							break;
					}
				}

				var adjustedServiceLevelPercentage = steadyStateDistribution[k + numberOfAgents] * Math.Pow(serviceIntensity / (serviceIntensity + _individualAbandonRate
					/ _intervalLength), k + 1) * (1 - sum);

				serviceLevelPercentage += steadyStateDistribution[k + numberOfAgents] * Math.Pow(serviceIntensity / (serviceIntensity + _individualAbandonRate
					/ _intervalLength), k + 1) * (1 - sum);
				if (Math.Abs(adjustedServiceLevelPercentage) < 0.0001)
					break;
			}
			if (serviceLevelPercentage > 1)
				return 1;
			return serviceLevelPercentage < 0 ? 0 : serviceLevelPercentage;
		}

		public double Occupancy(double numberOfAgents, List<double> steadyStateDistribution)
		{
			if (Math.Abs(_callsPerInterval) < 0.00001 || Math.Abs(_averageHandelingTime) < 0.00001)
				return 0;

			var occupancy = _trafficIntensity * (1 - AbandonPercentage((int)Math.Ceiling(numberOfAgents), steadyStateDistribution)) / numberOfAgents;
			if (occupancy > 1)
				return 1;
			return occupancy < 0 ? 0 : occupancy;
		}

		public double AbandonPercentage(int numberOfAgents, List<double> steadyStateDistribution)
		{
			if (Math.Abs(_callsPerInterval) < 0.00001 || Math.Abs(_averageHandelingTime) < 0.00001)
				return 0;
			if (numberOfAgents < 1)
				return 1;
			double abandonPercentage = 0;
			for (var i = numberOfAgents; i < steadyStateDistribution.Count; i++)
			{
				abandonPercentage += steadyStateDistribution[i] * (i - numberOfAgents + 1) * _individualAbandonRate /
									 (numberOfAgents * _serviceRate + (i - numberOfAgents + 1) * _individualAbandonRate);
			}
			if (abandonPercentage < 0)
				return 0;
			return abandonPercentage > 1 ? 1 : abandonPercentage;
		}
	}
}