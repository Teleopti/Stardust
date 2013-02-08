using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class SkillRepository
    {
    	private readonly ITeleoptiForecastingService _teleoptiForecastingService;
    	private Dictionary<Guid, SkillDto> _skillDictionary;

    	public SkillRepository(ITeleoptiForecastingService teleoptiForecastingService)
    	{
    		_teleoptiForecastingService = teleoptiForecastingService;
    	}

    	public void Initialize()
        {
            var skills = _teleoptiForecastingService.GetSkills();
            _skillDictionary = skills.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public SkillDto GetById(Guid id)
        {
            SkillDto skillDto;
            if (!_skillDictionary.TryGetValue(id, out skillDto))
            {
                skillDto = new SkillDto();
            }
            return skillDto;
        }
    }
}