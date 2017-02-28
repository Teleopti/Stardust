namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for templated days
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-08-27
    /// </remarks>
    public interface ITemplateDay
    {
        /// <summary>
        /// Gets the template reference.
        /// </summary>
        /// <value>The template reference.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-09
        /// </remarks>
        ITemplateReference TemplateReference { get; }
    }
}