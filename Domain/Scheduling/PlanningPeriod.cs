using System.Globalization;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PlanningPeriod : NonversionedAggregateRootWithBusinessUnit, IPlanningPeriod
	{
		private readonly DateOnlyPeriod _range;

		protected PlanningPeriod()
		{
			
		}

		public PlanningPeriod(INow now)
		{
			var date = now.LocalDateTime();
			var firstDateOfMonth = DateHelper.GetLastDateInMonth(date, CultureInfo.CurrentCulture).AddDays(1);
			var lastDateOfMonth = DateHelper.GetLastDateInMonth(firstDateOfMonth, CultureInfo.CurrentCulture);
			_range = new DateOnlyPeriod(new DateOnly(firstDateOfMonth), new DateOnly(lastDateOfMonth));
		}

		public virtual DateOnlyPeriod Range
		{
			get { return _range; }
		}
	}
}