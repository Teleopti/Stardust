using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.SimpleSample.Repositories
{
    public class ActivityRepository
    {
        private Dictionary<Guid, ActivityDto> _activityDictionary;

        public void Initialize(ITeleoptiSchedulingService teleoptiSchedulingService)
        {
            var activities = teleoptiSchedulingService.GetActivities(new LoadOptionDto { LoadDeleted = true });
            _activityDictionary = activities.ToDictionary(k => k.Id.GetValueOrDefault(), v => v);
        }

        public ActivityDto GetById(Guid id)
        {
            ActivityDto activityDto;
            if (!_activityDictionary.TryGetValue(id, out activityDto))
            {
                activityDto = null;
            }
            return activityDto;
        }
    }
}