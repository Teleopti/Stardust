using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class PersonAssignmentAssembler : ScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto>
	{
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private readonly IActivityLayerAssembler<MainShiftLayer> _mainActivityLayerAssembler;
		private readonly IActivityLayerAssembler<PersonalShiftLayer> _personalActivityLayerAssembler;
		private readonly IOvertimeLayerAssembler _overtimeShiftLayerAssembler;

		public PersonAssignmentAssembler(IShiftCategoryRepository shiftCategoryRepository, IActivityLayerAssembler<MainShiftLayer> mainActivityLayerAssembler, IActivityLayerAssembler<PersonalShiftLayer> personalActivityLayerAssembler, IOvertimeLayerAssembler overtimeShiftLayerAssembler)
		{
			_shiftCategoryRepository = shiftCategoryRepository;
			_mainActivityLayerAssembler = mainActivityLayerAssembler;
			_personalActivityLayerAssembler = personalActivityLayerAssembler;
			_overtimeShiftLayerAssembler = overtimeShiftLayerAssembler;
		}

		public override PersonAssignmentDto DomainEntityToDto(IPersonAssignment entity)
		{
			if (!entity.ShiftLayers.Any())
				return null;
			var retDto = new PersonAssignmentDto
			{
				Id = entity.Id,
				Version = entity.Version.GetValueOrDefault(0)
			};
			if (entity.ShiftCategory != null)
			{
				retDto.MainShift = CreateMainShiftDto(entity.MainActivities(), entity.ShiftCategory, entity.Person);
			}
	        var personalLayers = entity.PersonalActivities();
			if (personalLayers.Any())
			{
				var personalShift = CreatePersonalShiftDto(personalLayers, entity.Person);
				retDto.PersonalShiftCollection.Add(personalShift);
			}
	        var overtimeLayers = entity.OvertimeActivities();
			if (overtimeLayers.Any())
			{
				var overtimeShift = CreateOvertimeShiftDto(overtimeLayers, entity.Person);
				retDto.OvertimeShiftCollection.Add(overtimeShift);
			}

			return retDto;
		}

		protected override IPersonAssignment DtoToDomainEntityAfterValidation(PersonAssignmentDto dto)
		{
			//todo: not correct date - just to be able to compile
			IPersonAssignment ass = new PersonAssignment(Person, DefaultScenario, PartDate);
			ass.SetId(dto.Id);
			//rk - hack
            typeof(AggregateRoot_Events_ChangeInfo_Versioned).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(ass, dto.Version);
			addMainShift(ass, dto);
			addPersonalShift(ass, dto);
			if (dto.OvertimeShiftCollection != null)
				addOvertimeShift(ass, dto);
			return ass;
		}

		private void addPersonalShift(IPersonAssignment assignment, PersonAssignmentDto dto)
		{
			foreach (var persShiftDto in dto.PersonalShiftCollection)
			{
				var layersDomain = _personalActivityLayerAssembler.DtosToDomainEntities(persShiftDto.LayerCollection);
				foreach (var layer in layersDomain)
				{
		            assignment.AddPersonalActivity(layer.Payload, layer.Period);
				}
			}
		}

		private void addOvertimeShift(IPersonAssignment assignment, PersonAssignmentDto dto)
		{
			foreach (var overtimeShiftDto in dto.OvertimeShiftCollection)
			{
				var layersDomain = _overtimeShiftLayerAssembler.DtosToDomainEntities(overtimeShiftDto.LayerCollection.OfType<OvertimeLayerDto>());
				foreach (var layer in layersDomain)
				{
		            assignment.AddOvertimeActivity(layer.Payload, layer.Period, layer.DefinitionSet);
				}
			}
		}

		private void addMainShift(IPersonAssignment assignment,
								  PersonAssignmentDto dto)
		{
			if (dto.MainShift != null)
			{
				var shiftCategory = _shiftCategoryRepository.Load(dto.MainShift.ShiftCategoryId);
				foreach (var layer in _mainActivityLayerAssembler.DtosToDomainEntities(dto.MainShift.LayerCollection))
				{
		            assignment.AddActivity(layer.Payload, layer.Period);
				}
				assignment.SetShiftCategory(shiftCategory);
			}
		}

		private MainShiftDto CreateMainShiftDto(IEnumerable<MainShiftLayer> mainShiftLayers, IShiftCategory shiftCategory, IPerson shiftOwner)
		{
			var retDto = new MainShiftDto
				{
					ShiftCategoryId = shiftCategory.Id.Value,
					ShiftCategoryName = shiftCategory.Description.Name,
					ShiftCategoryShortName = shiftCategory.Description.ShortName
				};

			_mainActivityLayerAssembler.SetCurrentPerson(shiftOwner);
			var layers = _mainActivityLayerAssembler.DomainEntitiesToDtos(mainShiftLayers);
			layers.ForEach(retDto.LayerCollection.Add);

			return retDto;
		}

		private ShiftDto CreatePersonalShiftDto(IEnumerable<PersonalShiftLayer> personalLayers, IPerson shiftOwner)
		{
			ShiftDto retDto = new ShiftDto();

			_personalActivityLayerAssembler.SetCurrentPerson(shiftOwner);
			var layers = _personalActivityLayerAssembler.DomainEntitiesToDtos(personalLayers);
			layers.ForEach(retDto.LayerCollection.Add);

			return retDto;
		}

		private ShiftDto CreateOvertimeShiftDto(IEnumerable<OvertimeShiftLayer> overtimeLayers, IPerson shiftOwner)
		{
			ShiftDto retDto = new ShiftDto();

			_overtimeShiftLayerAssembler.SetCurrentPerson(shiftOwner);
			var layers = _overtimeShiftLayerAssembler.DomainEntitiesToDtos(overtimeLayers);
			layers.ForEach(retDto.LayerCollection.Add);

			return retDto;
		}
	}
}