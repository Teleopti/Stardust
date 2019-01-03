using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public class BankHolidayCalendar : NonversionedAggregateRootWithBusinessUnit, IBankHolidayCalendar, IAggregateRootWithEvents
	{
		private string _name;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private bool _isDeleted;

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}
		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
