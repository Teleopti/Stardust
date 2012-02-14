using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    /// <summary>
    /// Abstract base class for all specifications
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract bool IsSatisfiedBy(T obj);

        public ISpecification<T> And(ISpecification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public ISpecification<T> Or(ISpecification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public ISpecification<T> AndNot(ISpecification<T> specification)
        {
            return new AndNotSpecification<T>(this, specification);
        }
    }
}