using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping
{
	public interface IAsmViewModelMapper
	{
		AsmViewModel Map(DateTime asmZeroLocal, IEnumerable<IScheduleDay> scheduleDays, int unreadMessageCount);
	}
}