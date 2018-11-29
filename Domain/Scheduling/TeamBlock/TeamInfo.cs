using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamInfo
	{

		IEnumerable<IPerson> GroupMembers { get; }
		IEnumerable<IPerson> UnLockedMembers(DateOnly dateOnly);
		void LockMember(DateOnlyPeriod period, IPerson member);
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
		private readonly List<IPerson> _groupMembers= new List<IPerson>();
		private readonly string _name;
		private readonly Dictionary<DateOnly, IList<IPerson>> _lockedMemberDictionary = new Dictionary<DateOnly, IList<IPerson>>();
			 
		public TeamInfo(Group group, IList<IList<IScheduleMatrixPro>> matrixesForMembers)
		{
			_name = group.Name;
			_groupMembers.AddRange(group.GroupMembers);
			_matrixesForMembers = matrixesForMembers;
		}

		public IEnumerable<IPerson> GroupMembers => _groupMembers;

		public void LockMember(DateOnlyPeriod period, IPerson member)
		{
			if(!GroupMembers.Contains(member))
				throw new ArgumentOutOfRangeException(nameof(member), "Must be within group members");


			foreach (var dateOnly in period.DayCollection())
			{
				if (_lockedMemberDictionary.TryGetValue(dateOnly, out var personList))
				{
					if(!personList.Contains(member))
						personList.Add(member);
				}

				else
				{
					_lockedMemberDictionary.Add(dateOnly, new List<IPerson> {member});
				}
			}
		}

		public IEnumerable<IPerson> UnLockedMembers(DateOnly dateOnly)
		{
			IList<IPerson> ret = new List<IPerson>();
			foreach (var groupMember in GroupMembers)
			{
				if (!_lockedMemberDictionary.TryGetValue(dateOnly, out var members))
				{
					ret.Add(groupMember);
				}
				else if (!members.Contains(groupMember))
				{
					ret.Add(groupMember);
				}
			}

			return ret;
		}

		public string Name => _name;

		public IEnumerable<IScheduleMatrixPro> MatrixesForGroup()
		{
			return _matrixesForMembers.SelectMany(m => m).ToArray();
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
				if (scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.Intersection(period).HasValue)
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
			_lockedMemberDictionary.Clear();
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
			return obj is ITeamInfo ent && Equals(ent);
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