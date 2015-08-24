using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleCommandController : ApiController
	{

		private IPeopleSkillUpdater _peopleSkillUpdater;

		public PeopleCommandController(IPeopleSkillUpdater peopleSkillUpdater)
		{
			_peopleSkillUpdater = peopleSkillUpdater;
		}

		[HttpPost, Route("api/PeopleCommand/updateSkillOnPersons")]
		public virtual JsonResult<ResultModel> UpdateSkill(PeopleCommandInput model)
		{
			var result = new ResultModel();
			try
			{
				InternalUpdateSkills(model);
				result.Success = true;
			}
			catch (Exception)
			{
				result.Success = false;
			}

			return Json(result);
		}

		[UnitOfWork]
		protected virtual void InternalUpdateSkills(PeopleCommandInput model)
		{
			_peopleSkillUpdater.UpdateSkills(model);
		}
	}
}