using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Internal command for Teleopti WFM that forces a check if there's some schedule data waiting for denormalization.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
	public class DenormalizeNotificationCommandDto : CommandDto
	{
	}
}