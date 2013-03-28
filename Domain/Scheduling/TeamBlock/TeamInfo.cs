using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfo
	{
		IGroupPerson GroupPerson { get;	}
		IEnumerable<IScheduleMatrixPro> MatrixesForGroup();
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupMember(int index);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndDate(DateOnly dateOnly);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndPeriod(DateOnlyPeriod period);

		IScheduleMatrixPro MatrixesForMemberAndDate(IPerson groupMember, DateOnly dateOnly);
	}

	public class TeamInfo : ITeamInfo
	{
		private readonly IGroupPerson _groupPerson;
		private readonly IList<IList<IScheduleMatrixPro>> _matrixesForMembers;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
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

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndDate(DateOnly dateOnly)
		{
			IList<IScheduleMatrixPro> ret = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in MatrixesForGroup())
			{
				if(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Contains(dateOnly))
					ret.Add(scheduleMatrixPro);
			}

			return ret;
		}

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndPeriod(DateOnlyPeriod period)
		{
			IList<IScheduleMatrixPro> ret = new List<IScheduleMatrixPro>();
			foreach (var scheduleMatrixPro in MatrixesForGroup())
			{
				if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(period) != null)
					ret.Add(scheduleMatrixPro);
			}

			return ret;
		}

		public IScheduleMatrixPro MatrixesForMemberAndDate(IPerson groupMember, DateOnly dateOnly)
		{
			IScheduleMatrixPro matrix = null;
			var groupList = MatrixesForGroupAndDate(dateOnly);
			foreach (var matrixPro in groupList)
			{
				if (matrixPro.Person.Equals(groupMember))
				{
					matrix = matrixPro;
					break;
				}
			}

			return matrix;
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