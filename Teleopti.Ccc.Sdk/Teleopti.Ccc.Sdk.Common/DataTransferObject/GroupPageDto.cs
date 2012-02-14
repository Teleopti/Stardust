using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/09/")]
	public class GroupPageDto : Dto
	{
		[DataMember]
		public string PageName { get; set; }
	}

	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/09/")]
	public class GroupPageGroupDto : Dto
	{
		[DataMember]
		public string GroupName { get; set; }
	}
}