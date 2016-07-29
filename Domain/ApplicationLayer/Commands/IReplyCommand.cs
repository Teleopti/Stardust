using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	interface IReplyCommand
	{
		string ReplyMessage { get; set; }
		bool IsReplySuccess { get; set; }
	}
}
