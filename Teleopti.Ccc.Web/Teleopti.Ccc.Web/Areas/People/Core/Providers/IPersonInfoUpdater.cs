using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPersonInfoUpdater
	{
		int UpdatePersonSkill(PeopleSkillCommandInput model);
		int UpdatePersonShiftBag(PeopleShiftBagCommandInput model);
	}
}