using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPeopleSkillUpdater
	{
		void UpdateSkills(PeopleCommandInput model);
	}
}