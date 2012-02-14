﻿using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
{
    /// <summary>
    /// The definition if someone should be able to change the status of the personrequest
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-11-19
    /// </remarks>
    public class PersonRequestViewModelIsEditableSpecification:Specification<IPersonRequestViewModel>
    {
        private readonly IPersonRequestCheckAuthorization _authorization;

        public PersonRequestViewModelIsEditableSpecification(IPersonRequestCheckAuthorization authorization)
        {
            _authorization = authorization;
        }

        public override bool IsSatisfiedBy(IPersonRequestViewModel obj)
        {
            return obj.IsEditable && obj.IsSelected && obj.IsWithinSchedulePeriod &&
                   _authorization.HasEditRequestPermission(obj.PersonRequest);
        }
    }
}
