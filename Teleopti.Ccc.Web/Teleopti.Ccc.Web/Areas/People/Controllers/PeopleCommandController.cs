using System;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleCommandController : ApiController
	{
		private readonly IPeopleSkillUpdater _peopleSkillUpdater;

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
				result.SuccessCount = InternalUpdateSkills(model);
				result.Success = true;
			}
			catch (Exception e)
			{
				result.Success = false;
				result.ErrorMsg = e.Message;
			}

			return Json(result);
		}

		[UnitOfWork]
		protected virtual int InternalUpdateSkills(PeopleCommandInput model)
		{
			return _peopleSkillUpdater.UpdateSkills(model);
		}
	}
}