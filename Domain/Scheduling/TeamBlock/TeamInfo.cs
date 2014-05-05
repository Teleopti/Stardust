using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfo
	{

		IEnumerable<IPerson> GroupMembers { get; }
		IEnumerable<IPerson> UnLockedMembers();
		void LockMember(IPerson member);
		string Name { get; }
		IEnumerable<IScheduleMatrixPro> MatrixesForGroup();
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupMember(int index);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndDate(DateOnly dateOnly);
		IEnumerable<IScheduleMatrixPro> MatrixesForGroupAndPeriod(DateOnlyPeriod period);
		IScheduleMatrixPro MatrixForMemberAndDate(IPerson groupMember, DateOnly dateOnly);
		IEnumerable<IScheduleMatrixPro> MatrixesForMemberAndPeriod(IPerson groupMember, DateOnlyPeriod period);
		void ClearLocks();
	}

	public class TeamInfo : ITeamInfo
	{
		private readonly IList<IList<IScheduleMatrixPro>> _matrixesForMembers;
		private readonly IList<IPerson> _groupMembers= new List<IPerson>();
		private readonly IList<IPerson> _lockedMembers = new List<IPerson>();
		private readonly string _name;
			 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public TeamInfo(Group group, IList<IList<IScheduleMatrixPro>> matrixesForMembers)
		{
			_name = group.Name;
			foreach (var member in group.GroupMembers)
			{
				_groupMembers.Add(member);
			}
			_matrixesForMembers = matrixesForMembers;
		}

		public IEnumerable<IPerson> GroupMembers
		{
			get
			{
				return _groupMembers;
			}
		}

		public void LockMember(IPerson member)
		{
			if(!GroupMembers.Contains(member))
				throw new ArgumentOutOfRangeException("member", "Must be within group members");

			_lockedMembers.Add(member);
		}

		public IEnumerable<IPerson> UnLockedMembers()
		{
			var ret = new List<IPerson>();
			foreach (var groupMember in GroupMembers)
			{
				if (!_lockedMembers.Contains(groupMember))
					ret.Add(groupMember);
			}

			return ret;
		}

		public string Name
		{
			get { return _name; }
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

		public void ClearLocks()
		{
			_lockedMembers.Clear();
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