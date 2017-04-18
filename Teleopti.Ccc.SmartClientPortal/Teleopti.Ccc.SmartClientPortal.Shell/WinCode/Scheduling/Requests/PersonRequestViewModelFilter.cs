using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
    /// <summary>
    /// Responsible for filtering PersonRequestViewModels based on time from now
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-11-16
    /// </remarks>
    public class PersonRequestViewModelFilter : Specification<PersonRequestViewModel>, IPersonRequestViewModelFilter
    {

        public DateTime FilterDateTime { get; private set; }
        public ISpecification<IChangeInfo> InnerSpecification { get; private set;}

        public PersonRequestViewModelFilter(TimeSpan timeSpan)
        {
            FilterDateTime = DateTime.UtcNow.Subtract(timeSpan);
            InnerSpecification = new UpdatedAfterSpecification(FilterDateTime);
        }

        public PersonRequestViewModelFilter(ISpecification<IChangeInfo> innerSpecification)
        {
            InnerSpecification = innerSpecification;
        }

        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// Returns true if the item should be filtered (not shown)
        /// </summary>
        public override bool IsSatisfiedBy(PersonRequestViewModel obj)
        {
            return !obj.IsPending && !InnerSpecification.IsSatisfiedBy(obj.PersonRequest);
        }
    }
}
