using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a list of <see cref="PersonDto"/> by employment numbers.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonsByEmploymentNumbersQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person employment numbers.
		/// </summary>
		/// <value>The people's employment numbers.</value>
		[DataMember]
		public ICollection<string> EmploymentNumbers
		{
			get; set;
		}
	}
}