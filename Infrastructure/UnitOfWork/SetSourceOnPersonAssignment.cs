using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class SetSourceOnPersonAssignment : IPreCommitHook
	{
		private readonly ICurrentSchedulingSource _currentSchedulingSource;

		public SetSourceOnPersonAssignment(ICurrentSchedulingSource currentSchedulingSource)
		{
			_currentSchedulingSource = currentSchedulingSource;
		}

		public void BeforeCommit(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var modifiedPersonAssignments = modifiedRoots.Select(x => x.Root).OfType<IPersonAssignment>();
			foreach (var personAssignment in modifiedPersonAssignments.Where(pa => pa.Source != _currentSchedulingSource.Current()))
			{
				personAssignment.Source = _currentSchedulingSource.Current();
			}
		}
	}
}