using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Definition set for overtime (must be included when creating new overtime layers)
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
	public class DefinitionSetDto : Dto
	{

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the deleted flag.
		/// </summary>
		/// <remarks>Indicates whether this overtime definition set should not be used anymore.</remarks>
		[DataMember]
		public bool IsDeleted { get; set; }

		private ICollection<DefinitionSetLayerDto> _layerCollection = new List<DefinitionSetLayerDto>();

		/// <summary>
		/// Gets the collection of layers with details about this shift.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
			Justification = "The setter is used in serialization."), DataMember]
		public ICollection<DefinitionSetLayerDto> LayerCollection
		{
			get { return _layerCollection; }
			private set
			{
				if (value != null)
				{
					_layerCollection = new List<DefinitionSetLayerDto>(value);
				}
			}
		}

	}
}
