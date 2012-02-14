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
			var ret = TimeSpan.FromMinutes(0);
			if(dateOnlyDto != null)
			{
				var period = _person.Period(new DateOnly(dateOnlyDto.DateTime));
				if (period != null)
				{
					ret = period.PersonContract.Contract.WorkTimeDirective.NightlyRest;
				}
			}
			
			return ret;
		}
	}
}