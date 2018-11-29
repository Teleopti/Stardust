using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SetSourceOnPersonAssignment : IPreCommitHook
	{
		private readonly ICurrentSchedulingSource _currentSchedulingSource;

		public SetSourceOnPersonAssignment(ICurrentSchedulingSource currentSchedulingSource)
		{
			_currentSchedulingSource = currentSchedulingSource;
		}

		public void BeforeCommit(object root, IEnumerable<string> propertyNames, object[] currentState)
		{
			var personAssignment = root as IPersonAssignment;
			if (personAssignment != null)
			{
				var propertyIndex = getPropertyIndex(propertyNames);
				currentState[propertyIndex[nameof(IPersonAssignment.Source)]] = _currentSchedulingSource.Current();
				personAssignment.Source = _currentSchedulingSource.Current();
			}
		}

		private static IDictionary<string, int> getPropertyIndex(IEnumerable<string> properties)
		{
			var listOfProperties = properties.ToList();

			IDictionary<string, int> result = new Dictionary<string, int>();

			int index =
				listOfProperties.FindIndex(p => nameof(IPersonAssignment.Source).Equals(p, StringComparison.InvariantCultureIgnoreCase));
			if (index >= 0) result.Add(nameof(IPersonAssignment.Source), index);

			return result;
		}
	}
}