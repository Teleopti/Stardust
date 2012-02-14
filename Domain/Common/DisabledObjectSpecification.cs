using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Teleopti.Ccc.Domain.Common
{
    public class DisabledObjectSpecification<T>: Specification.Specification<T>
    {
        private readonly string _propertyName;

        public DisabledObjectSpecification(string propertyName)
        {
            _propertyName = propertyName;
        }

        public override bool IsSatisfiedBy(T obj)
        {
            PropertyInfo disabledProperty = typeof (T).GetProperty(_propertyName);
            if (disabledProperty != null)
            {
                return (bool) disabledProperty.GetValue(obj, null);
            }
            return false;
        }
    }
}
