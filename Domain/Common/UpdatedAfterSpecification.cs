using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Common
{
    public class UpdatedAfterSpecification : Specification<IChangeInfo>
    {
        public DateTime DateTime { get; private set; }

        public UpdatedAfterSpecification(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public override bool IsSatisfiedBy(IChangeInfo obj)
        {
            DateTime? toCompare = obj.UpdatedOn;
            if (toCompare == null) return true;
            InParameter.MustBeTrue(nameof(obj), ((DateTime)toCompare).Kind == DateTime.Kind);
            return toCompare > DateTime;
        }
    }
}
