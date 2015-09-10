using System;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleCommandController : ApiController
	{
		private readonly IPersonInfoUpdater _personInfoUpdater;

		public PeopleCommandController(IPersonInfoUpdater personInfoUpdater)
		{
			_personInfoUpdater = personInfoUpdater;
		}

		[HttpPost, Route("api/PeopleCommand/updateSkill")]
		public virtual JsonResult<ResultModel> UpdatePersonSkills(PeopleSkillCommandInput model)
		{
			var result = new ResultModel();
			try
			{
				result.SuccessCount = InternalUpdatePersonSkill(model);

				result.Success = true;
			}
			catch (Exception e)
			{
				result.Success = false;
				result.ErrorMsg = e.Message;
			}

			return Json(result);
		}
		[HttpPost, Route("api/PeopleCommand/updateShiftBag")]
		public virtual JsonResult<ResultModel> UpdatePersonShiftBag(PeopleShiftBagCommandInput model)
		{
			var result = new ResultModel();
			try
			{
				result.SuccessCount = InternalUpdatePersonShiftBag(model);

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
		protected virtual int InternalUpdatePersonSkill(PeopleSkillCommandInput model)
		{
			return _personInfoUpdater.UpdatePersonSkill(model);
		}

		[UnitOfWork]
		protected virtual int InternalUpdatePersonShiftBag(PeopleShiftBagCommandInput model)
		{
			return _personInfoUpdater.UpdatePersonShiftBag(model);
		}
	}
}