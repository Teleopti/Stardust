using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetAllScenariosQueryHandler : IHandleQuery<GetAllScenariosQueryDto,ICollection<ScenarioDto>>
	{
		private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetAllScenariosQueryHandler(IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_scenarioRepository = scenarioRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<ScenarioDto> Handle(GetAllScenariosQueryDto query)
		{
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				return
					_scenarioRepository.FindAllSorted().Select(
						s =>
						new ScenarioDto
							{Id = s.Id, Name = s.Description.Name, ShortName = s.Description.ShortName, DefaultScenario = s.DefaultScenario})
						.ToList();
			}
		}
	}
}