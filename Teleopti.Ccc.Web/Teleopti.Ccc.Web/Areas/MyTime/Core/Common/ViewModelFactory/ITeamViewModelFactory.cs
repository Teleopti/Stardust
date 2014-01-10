using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory
{
	public interface ITeamViewModelFactory
	{
		IEnumerable<ISelectOption> CreateTeamOrGroupOptionsViewModel(DateOnly date);
		IEnumerable<ISelectOption> CreateTeamOptionsViewModel(DateOnly date);
	}
}