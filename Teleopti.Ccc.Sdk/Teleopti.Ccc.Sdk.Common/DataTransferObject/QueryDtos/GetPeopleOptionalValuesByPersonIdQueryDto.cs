using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get <see cref="PersonOptionalValuesDto"/> by person Id.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2013/03/")]
	public class GetPeopleOptionalValuesByPersonIdQueryDto : QueryDto
	{
		private ICollection<Guid> _personIdCollection;

		public GetPeopleOptionalValuesByPersonIdQueryDto()
		{
			PersonIdCollection = new Collection<Guid>();
		}

		/// <summary>
		/// Gets and sets the id's of people to get optional values for.
		/// </summary>
		/// <value>The people Id's.</value>
		/// <remarks>The list of people is limited to 50 items.</remarks>
		[DataMember]
		public ICollection<Guid> PersonIdCollection
		{
			get { return _personIdCollection; }
			private set
			{
				if (value!=null)
				{
					_personIdCollection = new List<Guid>(value);
				}
			}
		}
	}
}