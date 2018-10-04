using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.TestLogic
{
	public class PlanTestParameters : IEnumerable<object>, IEquatable<PlanTestParameters>
	{
		private readonly IEnumerable<object> _parameters;

		public PlanTestParameters(IEnumerable<Toggles> toggles, SeperateWebRequest? seperateWebRequest)
		{
			var parameters = new List<object>();
			if(seperateWebRequest.HasValue)
			{
				parameters.Add(seperateWebRequest);
			}
			toggles.ForEach(x => parameters.Add(x));
			_parameters = parameters;
		}

		
		public override string ToString()
		{
			var seperateWebRequestOutput = new StringBuilder();
			var togglesOutput = new StringBuilder();
			if (_parameters.Any())
			{
				foreach (var parameter in _parameters)
				{
					if (parameter is SeperateWebRequest seperateWebRequest)
					{
						seperateWebRequestOutput.Append(seperateWebRequest);
					}
					else
					{
						togglesOutput.Append(parameter + ", ");
					}
				}
			}

			if (togglesOutput.Length > 0 )
			{
				if (seperateWebRequestOutput.Length > 0)
				{
					seperateWebRequestOutput.Append(" :: ");					
				}
				togglesOutput.Insert(0, "Enabled toggles: ");
				togglesOutput.Remove(togglesOutput.Length - 2, 2);
			}
			
			return seperateWebRequestOutput.ToString() + togglesOutput.ToString();
		}

		public IEnumerator<object> GetEnumerator()
		{
			return _parameters.GetEnumerator();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{	
			return GetEnumerator();
		}

		public bool Equals(PlanTestParameters other)
		{
			return _parameters.All(other._parameters.Contains);
		}

		public override bool Equals(object obj)
		{
			return Equals((PlanTestParameters) obj);
		}

		public override int GetHashCode()
		{
			return _parameters.Count();
		}

		public bool SimulateSecondRequest()
		{
			return _parameters.Any(x =>
				x is SeperateWebRequest seperateWebRequest &&
				seperateWebRequest == SeperateWebRequest.SimulateSecondRequestOrScheduler);
		}

		public void EnableToggles(FakeToggleManager toggleManager)
		{
			foreach (var parameter in _parameters)
			{
				if (parameter is Toggles toggle)
				{
					toggleManager.Enable(toggle);
				}
			}
		}
	}
}