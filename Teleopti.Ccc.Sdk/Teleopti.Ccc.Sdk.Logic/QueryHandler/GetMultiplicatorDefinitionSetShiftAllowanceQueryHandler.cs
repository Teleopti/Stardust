using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler : IHandleQuery<GetMultiplicatorDefinitionSetShiftAllowanceDto, ICollection<DefinitionSetDto>>
	{
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly IDateTimePeriodAssembler _dateTimePeriodAssembler;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public GetMultiplicatorDefinitionSetShiftAllowanceQueryHandler(IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, IDateTimePeriodAssembler dateTimePeriodAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_dateTimePeriodAssembler = dateTimePeriodAssembler;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ICollection<DefinitionSetDto> Handle(GetMultiplicatorDefinitionSetShiftAllowanceDto query)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSetList;
				using (unitOfWork.LoadDeletedIfSpecified(query.LoadDeleted))
				{
					multiplicatorDefinitionSetList = _multiplicatorDefinitionSetRepository.FindAllShiftAllowanceDefinitions();
				}
				var definitionSetDtoList = new List<DefinitionSetDto>();

				var period = query.Period.ToDateOnlyPeriod();
				var timeZone = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
				_dateTimePeriodAssembler.TimeZone = timeZone;

				foreach (var multiplicatorDefinitionSet in multiplicatorDefinitionSetList)
				{
					var projectedLayers = multiplicatorDefinitionSet.CreateProjectionForPeriod(period,timeZone);

					var projectLayersCollection = getProjectedLayerCollection(projectedLayers);

					var definitonSetDto = new DefinitionSetDto
					                      	{
					                      		Name = multiplicatorDefinitionSet.Name,
					                      		Id = multiplicatorDefinitionSet.Id,
					                      		IsDeleted = multiplicatorDefinitionSet.IsDeleted,
					                      	};
					projectLayersCollection.ForEach(definitonSetDto.LayerCollection.Add);

					definitionSetDtoList.Add(definitonSetDto);
				}

				return definitionSetDtoList;
			}
		}

		private IEnumerable<DefinitionSetLayerDto> getProjectedLayerCollection(IEnumerable<IMultiplicatorLayer> projectedLayers)
		{
			return projectedLayers.Select(multiplicatorLayer => new DefinitionSetLayerDto
				{
					MultiplicatorId = multiplicatorLayer.Payload.Id.GetValueOrDefault(), 
					Period = _dateTimePeriodAssembler.DomainEntityToDto(multiplicatorLayer.Period)
				}).ToList();
		}
	}
}
