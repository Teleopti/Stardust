using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class PartTimePercentageRepository
    {
        private Dictionary<Guid, PartTimePercentageDto> _partTimePercentageDictionary;

        public void Initialize(ITeleoptiOrganizationService teleoptiOrganizationService)
        {
            var partTimePercentages = teleoptiOrganizationService.GetPartTimePercentages(new LoadOptionDto { LoadDeleted = true });
            _partTimePercentageDictionary = partTimePercentages.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public PartTimePercentageDto GetById(Guid id)
        {
            PartTimePercentageDto partTimePercentageDto;
            if (!_partTimePercentageDictionary.TryGetValue(id, out partTimePercentageDto))
            {
                partTimePercentageDto = new PartTimePercentageDto();
            }
            return partTimePercentageDto;
        }
    }
}