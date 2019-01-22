
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for types holding layer collections.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILayerCollectionOwner<T> : ILayerCollectionOwner
    {

        /// <summary>
        /// Gets the layer collection.
        /// </summary>
        /// <value>The layer collection.</value>
        ILayerCollection<T> LayerCollection { get; }

        /// <summary>
        /// Called before layer is added to collection.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        void OnAdd(ILayer<T> layer);

		/// <summary>
		/// Called before layer is added to collection.
		/// </summary>
		/// <param name="layer">The layer.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-01-25
		/// </remarks>
		void OnAdd(IEnumerable<ILayer<T>> layers);
	}

	/// <summary>
	/// 
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ILayerCollectionOwner 
	{

	}
}