using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Specification
{
    /// <summary>
    /// Base class for composite specifications
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class CompositeSpecification<T> : Specification<T>
    {
        protected readonly ISpecification<T> _first;
        protected readonly ISpecification<T> _second;

        protected CompositeSpecification(ISpecification<T> first, ISpecification<T> second)
        {
            _first = first;
            _second = second;
        }
    }
}