using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfo
	{

		IEnumerable<IPerson> GroupMembers { get; }
		string Name { get; }
		IEnumerable<IScheduleMatrixPro> MatrixesForGroup();
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupMember(int index);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndDate(DateOnly dateOnly);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndPeriod(DateOnlyPeriod period);

		IScheduleMatrixPro MatrixForMemberAndDate(IPerson groupMember, DateOnly dateOnly);
		IEnumerable<IScheduleMatrixPro> MatrixesForMemberAndPeriod(IPerson groupMember, DateOnlyPeriod period);
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

		public IEnumerable<IPerson> GroupMembers
		{
			get
			{
				return _groupPerson.GroupMembers;
			}
		}

		public string Name
		{
			get { return _groupPerson.Name.ToString(); }
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

		public IScheduleMatrixPro MatrixForMemberAndDate(IPerson groupMember, DateOnly dateOnly)
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

		public IEnumerable<IScheduleMatrixPro> MatrixesForMemberAndPeriod(IPerson groupMember, DateOnlyPeriod period)
		{
			IList<IScheduleMatrixPro> ret = new List<IScheduleMatrixPro>();
			var groupList = MatrixesForGroupAndPeriod(period);
			foreach (var matrixPro in groupList)
			{
				if (matrixPro.Person.Equals(groupMember))
					ret.Add(matrixPro);
			}

			return ret;
		}

		public override int GetHashCode()
		{
			int combinedHash = 0;
			foreach (var groupMember in GroupMembers)
			{
				combinedHash = combinedHash ^ groupMember.GetHashCode();
			}
			return combinedHash;
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

			return (GetHashCode() == other.GetHashCode());
		}
	}
}