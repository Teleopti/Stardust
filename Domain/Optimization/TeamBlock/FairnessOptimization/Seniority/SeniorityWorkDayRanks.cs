using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	public class SeniorityWorkDayRanks : AggregateRoot_Events_ChangeInfo_BusinessUnit, ISeniorityWorkDayRanks
	{
		private int _monday;
		private int _tuesday;
		private int _wednesday;
		private int _thursday;
		private int _friday;
		private int _saturday;
		private int _sunday;

		public SeniorityWorkDayRanks()
		{	
			setDefaultValues();
		}

		public virtual IList<ISeniorityWorkDay> SeniorityWorkDays()
		{
			var list = new List<ISeniorityWorkDay>();
			list.Add(new SeniorityWorkDay(DayOfWeek.Monday, Monday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Tuesday, Tuesday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Wednesday, Wednesday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Thursday, Thursday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Friday, Friday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Saturday, Saturday));
			list.Add(new SeniorityWorkDay(DayOfWeek.Sunday, Sunday));

			return list.OrderBy(x => x.Rank).ToList();
		}

		public virtual int Monday
		{
			get { return _monday; }
			set { _monday = value; }
		}

		public virtual int Tuesday
		{
			get { return _tuesday; }
			set { _tuesday = value; }
		}
		public virtual int Wednesday
		{
			get { return _wednesday; }
			set { _wednesday = value; }
		}
		public virtual int Thursday
		{
			get { return _thursday; }
			set { _thursday = value; }
		}
		public virtual int Friday
		{
			get { return _friday; }
			set { _friday = value; }
		}
		public virtual int Saturday
		{
			get { return _saturday; }
			set { _saturday = value; }
		}
		public virtual int Sunday
		{
			get { return _sunday; }
			set { _sunday = value; }
		}

		private void setDefaultValues()
		{
			Monday = 1;
			Tuesday = 2;
			Wednesday = 3;
			Thursday = 4;
			Friday = 5;
			Saturday = 6;
			Sunday = 7;
		}
	}
}
