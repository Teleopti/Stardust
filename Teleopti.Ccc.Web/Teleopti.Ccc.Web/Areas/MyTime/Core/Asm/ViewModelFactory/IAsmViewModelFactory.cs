using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.ViewModelFactory
{
	public interface IAsmViewModelFactory
	{
		AsmViewModel CreateViewModel(DateTime asmZeroLocal);
	}
}