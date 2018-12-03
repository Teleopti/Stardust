using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class SiteOpenHour : VersionedAggregateRoot, ISiteOpenHour
	{
		private ISite _parent;
		private DayOfWeek _weekDay;
		private TimePeriod _timePeriod;
		private bool _isClosed;
		public virtual ISite Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public virtual DayOfWeek WeekDay
		{
			get { return _weekDay; }
			set { _weekDay = value; }
		}

		public virtual TimePeriod TimePeriod
		{
			get { return _timePeriod; }
			set { _timePeriod = value; }
		}

		public virtual bool IsClosed
		{
			get { return _isClosed; }
			set { _isClosed = value; }
		}
	}
}
