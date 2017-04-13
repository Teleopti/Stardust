using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
    /// <summary>
    /// Selected Entity for setting pages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 1/19/2009
    /// </remarks>
    public class SelectedEntity<T> where T:IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedEntity&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="selectedEntityObject">The selected entity object.</param>
        /// <param name="type">The type.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        public SelectedEntity(T selectedEntityObject, ViewType type)
        {
            SelectedEntityObject = selectedEntityObject;
            ViewType = type;
        }

        /// <summary>
        /// Gets or sets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        public ViewType ViewType { get; set; }

        public T SelectedEntityObject { get; set; }
    }
}
