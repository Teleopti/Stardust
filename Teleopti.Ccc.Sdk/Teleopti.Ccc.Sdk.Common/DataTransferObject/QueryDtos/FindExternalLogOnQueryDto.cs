using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for <see cref="ExternalLogOnDto"/>.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/11/")]
	public class FindExternalLogOnQueryDto : QueryDto
	{
		private ICollection<string> _externalLogOnCollection;

		public FindExternalLogOnQueryDto()
		{
			ExternalLogOnCollection = new Collection<string>();
		}

		/// <summary>
		/// Gets or sets the ExternalLogOns to search for
		/// </summary>
		/// <value>The external logons.</value>
		/// <remarks>The list of external logons is limited to 50 items.</remarks>
		[DataMember]
		public ICollection<string> ExternalLogOnCollection
		{
			get { return _externalLogOnCollection; }
			private set
			{
				if (value != null)
				{
					_externalLogOnCollection = new List<string>(value);
				}
			}
		}
	}
}