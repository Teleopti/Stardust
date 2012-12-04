using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query for <see cref="MultiplicatorDto"/> for overtime.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/09/")]
    public class GetMultiplicatorOvertimeQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets include deleted.
		/// </summary>
		/// <remarks>Default is false, deleted items will not be included in the result.</remarks>
		[DataMember]
		public bool LoadDeleted
		{
			get;
			set;
		}
	}
}
