using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
	public interface IGeneticDayOffBackToLegalStateSolver
	{
		HashSet<DayOffArray> Execute(int crLength, int numberOfDaysOff, IList<IDayOffLegalStateValidator> validatorList);
	}

	public class GeneticDayOffBackToLegalStateSolver : IGeneticDayOffBackToLegalStateSolver
	{
		private readonly Random _random = new Random();
		private const int maxIterations = 1000;

		public HashSet<DayOffArray> Execute(int crLength, int numberOfDaysOff, IList<IDayOffLegalStateValidator> validatorList)
		{
			//randomly generate 100 stone age persons
			var population = new List<DayOffArray>();
			for (int i = 0; i < 100; i++)
			{
				var workingArray = new DayOffArray(crLength);
				var trueBits = 0;
				while (trueBits < numberOfDaysOff)
				{
					var rndPlace = _random.Next(0, workingArray.Length);
					if(!workingArray.Get(rndPlace))
					{
						workingArray.Set(rndPlace, true);
						trueBits++;
					}
				}

				population.Add(workingArray);
			}
			//calculate value for each person, value calculator will be important
			calculateAndSetValues(validatorList, population);

			for (int i = 2; i < 100; i++)
			{
				//Debug.Print(i + " generation");
				population = advanceOneGeneration(crLength, numberOfDaysOff, validatorList, population);
			}

			var iterations = 99;
			while (population.Count(v => v.Value() == numberOfDaysOff) == 0 && iterations < maxIterations)
			{
				population = advanceOneGeneration(crLength, numberOfDaysOff, validatorList, population);
				iterations++;
			}

			return new HashSet<DayOffArray>(population.Where(v => v.Value() == numberOfDaysOff));
		}

		private List<DayOffArray> advanceOneGeneration(int crLength, int numberOfDaysOff, IList<IDayOffLegalStateValidator> validatorList, List<DayOffArray> population)
		{
			var childPopulation = new List<DayOffArray>();
			foreach (var parent in population)
			{
				if(parent.Value() == numberOfDaysOff)
					continue;

				childPopulation.Add(createClonedAndMutant(parent, crLength, numberOfDaysOff, 0));
				childPopulation.Add(createClonedAndMutant(parent, crLength, numberOfDaysOff, 1));
				childPopulation.Add(createClonedAndMutant(parent, crLength, numberOfDaysOff, 2));
				childPopulation.Add(createClonedAndMutant(parent, crLength, numberOfDaysOff, 3));
			}

			//calculate value for each child, value calculator will be important
			calculateAndSetValues(validatorList, childPopulation);
			population.AddRange(childPopulation);
			//kill everyone but the top 100 persons by value
			population = population.OrderByDescending(v => v.Value()).Take(100).ToList();
			
			return population;
		}

		private DayOffArray createClonedAndMutant(DayOffArray parent, int crLength, int numberOfDaysOff, int randomUnlockNumber)
		{
			var child = new DayOffArray(crLength);
			//keep the good genes
			for (int i = 0; i < crLength; i++)
			{
				if (parent.Locked(i))
					child.Set(i, true);
			}

			//add random genes
			var trueBits = child.DaysOffCount();
			while (trueBits < numberOfDaysOff)
			{
				var rndPlace = _random.Next(0, child.Length);
				if (!child.Get(rndPlace))
				{
					child.Set(rndPlace, true);
					trueBits++;
				}
			}

			//mutant random genes
			trueBits = 0;
			while (trueBits < randomUnlockNumber)
			{
				var rndPlace = _random.Next(0, child.Length);
				if (child.Get(rndPlace))
				{
					child.Set(rndPlace, false);
					trueBits++;
				}
			}
			trueBits = child.DaysOffCount();
			while (trueBits < numberOfDaysOff)
			{
				var rndPlace = _random.Next(0, child.Length);
				if (!child.Get(rndPlace))
				{
					child.Set(rndPlace, true);
					trueBits++;
				}
			}

			return child;
		}

		private void calculateAndSetValues(IList<IDayOffLegalStateValidator> validatorList, List<DayOffArray> population)
		{
			foreach (var dayOffArray in population)
			{
				for (int i = 0; i < dayOffArray.Length; i++)
				{
					if (dayOffArray.Get(i))
					{
						bool allValidatorsOk = true;
						foreach (var dayOffLegalStateValidator in validatorList)
						{
							bool ok = dayOffLegalStateValidator.IsValid(convertForValidator(dayOffArray.HostedArray), i + 7);
							if (!ok)
								allValidatorsOk = false;
						}
						if(allValidatorsOk)
							dayOffArray.Lock(i);
					}
				}
			}
		}

		private BitArray convertForValidator(BitArray workingArray)
		{
			var result = new BitArray(workingArray.Length + 14, false);
			for (int i = 0; i < workingArray.Length; i++)
			{
				result.Set(i+7,workingArray.Get(i));
			}
			return result;
		}
	}

	public class DayOffArray
	{
		private readonly BitArray _hostedArray;
		private readonly BitArray _lockedArray;

		public DayOffArray(int length)
		{
			_hostedArray = new BitArray(length, false);
			_lockedArray = new BitArray(length, false);
		}

		public void Lock(int index)
		{
			_lockedArray.Set(index, true);
		}

		public bool Locked(int index)
		{
			return _lockedArray.Get(index);
		}

		public int DaysOffCount()
		{
			return _hostedArray.Cast<object>().Count(bit => (bool) bit);
		}

		public int Value()
		{
			var cnt = 0;
			foreach (var bit in _lockedArray)
			{
				if ((bool)bit)
					cnt++;
			}
			return cnt;
		}

		public void Set(int index, bool value)
		{
			HostedArray.Set(index, value);
		}

		public bool Get(int index)
		{
			return _hostedArray.Get(index);
		}

		public int Length
		{
			get { return HostedArray.Length; }
		}

		public BitArray HostedArray
		{
			get { return _hostedArray; }
		}

		public override string ToString()
		{
			var builder = new StringBuilder(HostedArray.Length);
			for (int i = 0; i < HostedArray.Length; i++)
			{
				builder.Append(HostedArray.Get(i) ? "1" : "0");
			}
			builder.Append(",");
			builder.Append(Value());
			return builder.ToString();
		}

		public override bool Equals(object obj)
		{
			var item = obj as DayOffArray;
			if (item == null)
				return false;

			return GetHashCode().Equals(item.GetHashCode());
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
	}
}