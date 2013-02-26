

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ITeamInfo
	{
		IGroupPerson GroupPerson { get;	}
		IEnumerable<IScheduleMatrixPro> MatrixesForGroup { get; }
	}

	public class TeamInfo : ITeamInfo
	{
		private readonly IGroupPerson _groupPerson;
		private readonly IList<IScheduleMatrixPro> _matrixesForGroup;

		public TeamInfo(IGroupPerson groupPerson, IList<IScheduleMatrixPro> matrixesForGroup)
		{
			_groupPerson = groupPerson;
			_matrixesForGroup = matrixesForGroup;
		}

		public IGroupPerson GroupPerson
		{
			get { return _groupPerson; }
		}

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroup
		{
			get { return _matrixesForGroup; }
		}
	}
}