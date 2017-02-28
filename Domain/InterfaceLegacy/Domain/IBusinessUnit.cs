using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// BusinessUnit
    /// </summary>
    public interface IBusinessUnit : IAggregateRoot, 
                                        IChangeInfo
    {
        /// <summary>
        /// Set/Get for description
        /// </summary>     
        Description Description { get; set; }

        /// <summary>
        /// Name Property
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// ShortName Property
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Gets the Sites.
        /// Read only wrapper around the actual site list.
        /// </summary>
        /// <value>The agents list.</value>
        ReadOnlyCollection<ISite> SiteCollection { get; }

        /// <summary>
        /// Adds a Site.
        /// </summary>
        /// <param name="site">The site.</param>
        void AddSite(ISite site);

        /// <summary>
        /// Removes the site.
        /// </summary>
        /// <param name="site">The site.</param>
        void RemoveSite(ISite site);
    }
}
