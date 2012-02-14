using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    /// <summary>
    /// A composite specification for "or" operations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OrSpecification<T> : CompositeSpecification<T>
    {
        internal OrSpecification(ISpecification<T> first, ISpecification<T> second)
            : base(first, second)
        {
        }

        public override bool IsSatisfiedBy(T obj)
        {
            return _first.IsSatisfiedBy(obj) || _second.IsSatisfiedBy(obj);
        }
    }
}