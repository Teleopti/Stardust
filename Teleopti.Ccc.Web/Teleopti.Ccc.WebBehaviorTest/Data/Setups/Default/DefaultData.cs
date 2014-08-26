using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultData : IEnumerable<IHashableDataSetup>
	{
		private readonly IEnumerable<IHashableDataSetup> setups = new IHashableDataSetup[]
		{
			new DefaultPersonThatCreatesDbData(),
			new DefaultLicense(),
			new DefaultBusinessUnit(),
			new DefaultScenario(),
			new DefaultRaptorApplicationFunctions(),
			new DefaultMatrixApplicationFunctions()
		};

		private int? _hashValue;

		public int HashValue
		{
			get
			{
				if (!_hashValue.HasValue)
				{
					_hashValue = calculateDataHash();
				}
				return _hashValue.Value;
			}
		}

		private int calculateDataHash()
		{
			return setups.Aggregate(37, (current, setup) => current ^ setup.HashValue());
		}

		public IEnumerator<IHashableDataSetup> GetEnumerator()
		{
			return setups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}