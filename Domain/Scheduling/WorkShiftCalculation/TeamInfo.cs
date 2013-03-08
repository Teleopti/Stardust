

using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ITeamInfo
	{
		IGroupPerson GroupPerson { get;	}
		IEnumerable<IScheduleMatrixPro> MatrixesForGroup();
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupMember(int index);
	}

	public class TeamInfo : ITeamInfo
	{
		private readonly IGroupPerson _groupPerson;
		private readonly IList<IList<IScheduleMatrixPro>> _matrixesForMembers;

		public TeamInfo(IGroupPerson groupPerson, IList<IList<IScheduleMatrixPro>> matrixesForMembers)
		{
			_groupPerson = groupPerson;
			_matrixesForMembers = matrixesForMembers;
		}

		public IGroupPerson GroupPerson
		{
			get { return _groupPerson; }
		}

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroup()
		{
			IList<IScheduleMatrixPro> ret = new List<IScheduleMatrixPro>();
			foreach (var matrixesForMember in _matrixesForMembers)
			{
				foreach (var scheduleMatrixPro in matrixesForMember)
				{
					ret.Add(scheduleMatrixPro);
				}
			}

			return ret;
		}

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroupMember(int index)
		{
			return _matrixesForMembers[index];
		}

		public override int GetHashCode()
		{
			return _groupPerson.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			ITeamInfo ent = obj as ITeamInfo;
			if (ent == null)
				return false;
			return Equals(ent);
		}

		public virtual bool Equals(ITeamInfo other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;
			if (!other.GroupPerson.Id.HasValue || !GroupPerson.Id.HasValue)
				return false;

			return (GroupPerson.Id.Value == other.GroupPerson.Id.Value);
		}
	}
}