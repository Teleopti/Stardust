using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Specify a query to get a <see cref="PersonDto"/> by user name.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class GetPersonByUserNameQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the mandatory person user name.
		/// </summary>
		/// <value>The person's user name.</value>
		[DataMember]
		public string UserName
		{
			get;
			set;
		}
	}
	/// <summary>
	/// Specify a query to get <see cref="PersonDto"/> that are users.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/08/")]
	public class GetUsersQueryDto : QueryDto
	{
		/// <summary>
		/// Gets and sets the flag to indicate if deleted users are included.
		/// </summary>
		/// <value>True or false.</value>
		[DataMember]
		public bool LoadDeleted { get; set; }

		/// <summary>
		/// Get and sets the date to search for users for. Defaults to todays date when not specified.
		/// </summary>
		[DataMember]
		public DateOnlyDto Date { get; set; }
	}
}