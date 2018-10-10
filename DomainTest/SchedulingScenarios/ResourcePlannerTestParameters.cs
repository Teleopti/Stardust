using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class ResourcePlannerTestParameters : IEnumerable<object>, IEquatable<ResourcePlannerTestParameters>
	{
		private readonly IEnumerable<object> _parameters;

		public ResourcePlannerTestParameters(IEnumerable<Toggles> toggles, SeparateWebRequest? separateWebRequest)
		{
			var parameters = new List<object>();
			if(separateWebRequest.HasValue)
			{
				parameters.Add(separateWebRequest);
			}
			toggles.OrderBy(x => x.ToString()).ForEach(x => parameters.Add(x));
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
					if (parameter is SeparateWebRequest separateWebRequest)
					{
						seperateWebRequestOutput.Append(separateWebRequest);
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

		public bool Equals(ResourcePlannerTestParameters other)
		{
			return _parameters.SequenceEqual(other._parameters);
		}

		public override bool Equals(object obj)
		{
			return Equals((ResourcePlannerTestParameters) obj);
		}

		public override int GetHashCode()
		{
			return _parameters.Count();
		}

		public void SimulateNewRequest(IIoCTestContext iocTestContext)
		{
			if (_parameters.Any(x => x is SeparateWebRequest separateWebRequest &&
				separateWebRequest == SeparateWebRequest.SimulateSecondRequestOrScheduler))
			{
				iocTestContext.SimulateNewRequest();
			}
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
		
		public bool IsEnabled(Toggles toggle)
		{
			return _parameters.Contains(toggle);
		}
	}
}