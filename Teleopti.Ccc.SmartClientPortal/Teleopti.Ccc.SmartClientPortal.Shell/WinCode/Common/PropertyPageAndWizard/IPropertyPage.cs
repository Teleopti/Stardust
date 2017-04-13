using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Interface for property pages in wizards etc.
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public interface IPropertyPage
    {
        /// <summary>
        /// Populates this instance.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        void Populate(IAggregateRoot aggregateRoot);

        /// <summary>
        /// Depopulates this instance.
        /// </summary>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        bool Depopulate(IAggregateRoot aggregateRoot);

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        void SetEditMode();

        /// <summary>
        /// Gets the name of the page.
        /// </summary>
        /// <value>The name of the page.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        string PageName { get; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-05-27
        /// </remarks>
        void Dispose();
    }
}