using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public interface ITeamViewModelFactory
	{
		IEnumerable<SelectBase> CreateTeamOrGroupOptionsViewModel(DateOnly date);
		IEnumerable<SelectOptionItem> CreateTeamOptionsViewModel(DateOnly date, string applicationFunctionPath);
	}
}