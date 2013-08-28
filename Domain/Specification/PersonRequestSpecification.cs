using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    public abstract class PersonRequestSpecification<T> : IPersonRequestSpecification<T>
    {
        public abstract IValidatedRequest IsSatisfied(T obj);
    }
}
