using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class AbsenceRepository
    {
    	private readonly ITeleoptiSchedulingService _teleoptiSchedulingService;
    	private Dictionary<Guid, AbsenceDto> _absenceDictionary;

		public AbsenceRepository(ITeleoptiSchedulingService teleoptiSchedulingService)
		{
			_teleoptiSchedulingService = teleoptiSchedulingService;
		}

    	public void Initialize()
		{
			var absences = _teleoptiSchedulingService.GetAbsences(new AbsenceLoadOptionDto { LoadDeleted = true });
			_absenceDictionary = absences.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
		}

        public AbsenceDto GetById(Guid id)
        {
            AbsenceDto absenceDto;
            if (!_absenceDictionary.TryGetValue(id, out absenceDto))
            {
                absenceDto = null;
            }
            return absenceDto;
        }
    }
}