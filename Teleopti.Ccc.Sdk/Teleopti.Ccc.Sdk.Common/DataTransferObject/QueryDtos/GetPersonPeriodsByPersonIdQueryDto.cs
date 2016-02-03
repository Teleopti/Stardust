using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// A query to get person periods for a batch of people.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/02/")]
	public class GetPersonPeriodsByPersonIdQueryDto : QueryDto
	{
		private ICollection<Guid> _personIdCollection;

		public GetPersonPeriodsByPersonIdQueryDto()
		{
			PersonIdCollection = new Collection<Guid>();
		}

		/// <summary>
		/// Gets or sets the id's of people to get person periods for.
		/// </summary>
		/// <value>The people Id's.</value>
		/// <remarks>The list of people is limited to 50 items.</remarks>
		[DataMember]
		public ICollection<Guid> PersonIdCollection
		{
			get { return _personIdCollection; }
			private set
			{
				if (value != null)
				{
					_personIdCollection = new List<Guid>(value);
				}
			}
		}

		/// <summary>
		/// The date range to fetch person periods for.
		/// </summary>
		[DataMember]
		public DateOnlyPeriodDto Range { get; set; }
	}
}