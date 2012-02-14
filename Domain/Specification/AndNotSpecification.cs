using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    /// <summary>
    /// A composite specification for "and not" operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class AndNotSpecification<T> : CompositeSpecification<T>
    {
        internal AndNotSpecification(ISpecification<T> first, ISpecification<T> second)
            : base(first, second)
        {
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _first.IsSatisfiedBy(obj) && !_second.IsSatisfiedBy(obj);
        }
    }
}