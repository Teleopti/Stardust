using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SkillsController : ApiController
	{
		private readonly SkillViewModelBuilder _viewModelBuilder;
		private readonly ISkillRepository _skillRepository;

		public SkillsController(SkillViewModelBuilder viewModelBuilder, ISkillRepository skillRepository)
		{
			_viewModelBuilder = viewModelBuilder;
			_skillRepository = skillRepository;
		}

		[UnitOfWork, HttpGet, Route("api/Skills")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_viewModelBuilder.Build());
		}

		[UnitOfWork, HttpGet, Route("api/Skills/NameFor")]
		public virtual IHttpActionResult NameFor(Guid skillId)
		{
			return Json(_skillRepository.Load(skillId).Name);
		}
	}
}