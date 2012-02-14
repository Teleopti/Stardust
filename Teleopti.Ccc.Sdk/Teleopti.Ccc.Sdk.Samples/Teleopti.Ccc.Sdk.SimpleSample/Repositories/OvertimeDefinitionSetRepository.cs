using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class OvertimeDefinitionSetRepository
    {
        private Dictionary<Guid, OvertimeDefinitionSetDto> _overtimeDefinitionSetDictionary;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            var overtimeDefinitions = teleoptiOrganizationService.GetOvertimeDefinitions(new LoadOptionDto { LoadDeleted = true });
            _overtimeDefinitionSetDictionary = overtimeDefinitions.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public OvertimeDefinitionSetDto GetById(Guid id)
        {
            OvertimeDefinitionSetDto overtimeDefinitionSetDto;
            if (!_overtimeDefinitionSetDictionary.TryGetValue(id, out overtimeDefinitionSetDto))
            {
                overtimeDefinitionSetDto = new OvertimeDefinitionSetDto();
            }
            return overtimeDefinitionSetDto;
        }
    }
}