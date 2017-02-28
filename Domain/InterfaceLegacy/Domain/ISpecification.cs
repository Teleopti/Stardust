namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for all Specifications
    /// </summary>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Determines whether the obj satisfies the specification.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>
        /// 	<c>true</c> if [is satisfied by] [the specified obj]; otherwise, <c>false</c>.
        /// </returns>
        bool IsSatisfiedBy(T obj);

        /// <summary>
        /// Combine this specification with another.
        /// Both specification must be fulfilled.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> And(ISpecification<T> specification);

        /// <summary>
        /// Combine this specification with another.
        /// One specification must be fulfilled.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> Or(ISpecification<T> specification);

        /// <summary>
        /// Combine this specification with another.
        /// First specification must be fulfilled but not the other.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns></returns>
        ISpecification<T> AndNot(ISpecification<T> specification);
    }
}