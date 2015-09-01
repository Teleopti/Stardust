using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPersonInfoUpdater
	{
		int UpdatePersonInfo(PeopleCommandInput model);
	}
}