using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    /// <summary>
    /// A composite specification for "and" operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AndSpecification<T> : CompositeSpecification<T>
    {
        internal AndSpecification(ISpecification<T> first, ISpecification<T> second)
            : base(first, second)
        {
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _first.IsSatisfiedBy(obj) && _second.IsSatisfiedBy(obj);
        }
    }
}