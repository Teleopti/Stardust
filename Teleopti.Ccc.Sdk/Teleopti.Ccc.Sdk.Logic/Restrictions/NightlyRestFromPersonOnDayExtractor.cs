using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
	public interface INightlyRestFromPersonOnDayExtractor
	{
		TimeSpan NightlyRestOnDay(DateOnlyDto dateOnlyDto);
	}

	public class NightlyRestFromPersonOnDayExtractor : INightlyRestFromPersonOnDayExtractor
	{
		private readonly IPerson _person;

		public NightlyRestFromPersonOnDayExtractor(IPerson person)
		{
			_person = person;
		}

		public TimeSpan NightlyRestOnDay(DateOnlyDto dateOnlyDto)
		{
			var dateOnly = dateOnlyDto.AsDateOnly();
			if (!dateOnly.HasValue) return TimeSpan.Zero;

			var period = _person.Period(dateOnly.Value);
			return period != null ? period.PersonContract.Contract.WorkTimeDirective.NightlyRest : TimeSpan.Zero;
		}
	}
}