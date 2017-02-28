using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Requests
{
	//only still here to handle "old" messages still in the database when patching customer
	public class NewAbsenceRequestCreated : MessageWithLogOnContext
	{
		public Guid PersonRequestId { get; set; }

		public override Guid Identity
		{
			get { return PersonRequestId; }
		}
	}
}