using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonAssignmentAssembler : ScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto>
    {
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IActivityLayerAssembler<IMainShiftLayer> _mainActivityLayerAssembler;
        private readonly IActivityLayerAssembler<IPersonalShiftActivityLayer> _personalActivityLayerAssembler;
        private readonly IOvertimeLayerAssembler _overtimeShiftLayerAssembler;

        public PersonAssignmentAssembler(IShiftCategoryRepository shiftCategoryRepository, IActivityLayerAssembler<IMainShiftLayer> mainActivityLayerAssembler, IActivityLayerAssembler<IPersonalShiftActivityLayer> personalActivityLayerAssembler, IOvertimeLayerAssembler overtimeShiftLayerAssembler)
        {
            _shiftCategoryRepository = shiftCategoryRepository;
            _mainActivityLayerAssembler = mainActivityLayerAssembler;
            _personalActivityLayerAssembler = personalActivityLayerAssembler;
            _overtimeShiftLayerAssembler = overtimeShiftLayerAssembler;
        }

        public override PersonAssignmentDto DomainEntityToDto(IPersonAssignment entity)
        {
            PersonAssignmentDto retDto = new PersonAssignmentDto
            {
                Id = entity.Id,
                Version = entity.Version.GetValueOrDefault(0)
            };
					if (entity.ShiftCategory != null)
					{
						retDto.MainShift = CreateMainShiftDto(entity.MainShiftLayers, entity.ShiftCategory, entity.Person);						
					}
            foreach (IPersonalShift personalShift in entity.PersonalShiftCollection)
            {
                retDto.PersonalShiftCollection.Add(CreatePersonalShiftDto(personalShift, entity.Person));
            }
            foreach (IOvertimeShift overtimeShift in entity.OvertimeShiftCollection)
            {
                retDto.OvertimeShiftCollection.Add(CreateOvertimeShiftDto(overtimeShift, entity.Person));
            }

            return retDto;
        }

        protected override IPersonAssignment DtoToDomainEntityAfterValidation(PersonAssignmentDto dto)
        {
					//todo: not correct date - just to be able to compile
            IPersonAssignment ass = new PersonAssignment(Person, DefaultScenario, PartDate);
            ass.SetId(dto.Id);
            //rk - hack
            typeof(AggregateRoot).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ass, dto.Version);
            addMainShift(ass, dto);
            addPersonalShift(ass, dto);
            if (dto.OvertimeShiftCollection!=null)
                addOvertimeShift(ass,dto);
            return ass;
        }

        private void addPersonalShift(IPersonAssignment assignment, PersonAssignmentDto dto)
        {
            foreach (var persShiftDto in dto.PersonalShiftCollection)
            {
                IPersonalShift shift = new PersonalShift();
                shift.SetId(persShiftDto.Id);
                addLayersToPersonalShift(shift, persShiftDto.LayerCollection);
                assignment.AddPersonalShift(shift);
            }
        }

        private void addOvertimeShift(IPersonAssignment assignment, PersonAssignmentDto dto)
        {
            foreach (var overtimeShiftDto in dto.OvertimeShiftCollection)
            {
                IOvertimeShift shift = new OvertimeShift();
                shift.SetId(overtimeShiftDto.Id);
                assignment.AddOvertimeShift(shift);
                addLayersToOvertimeShift(shift, overtimeShiftDto.LayerCollection.OfType<OvertimeLayerDto>());
            }
        }

        private void addMainShift(IPersonAssignment assignment, 
                                  PersonAssignmentDto dto)
        {
            if(dto.MainShift!=null)
            {
                var shiftCategory = _shiftCategoryRepository.Load(dto.MainShift.ShiftCategoryId);
	            var layers = new List<IMainShiftLayer>();
	            _mainActivityLayerAssembler.DtosToDomainEntities(dto.MainShift.LayerCollection).ForEach(layers.Add);
							assignment.SetMainShiftLayers(layers, shiftCategory);
            }
        }

        //merge this one with addLaysersToMainshift when we only have one type of activitylayer
        private void addLayersToPersonalShift(IPersonalShift personalShift, IEnumerable<ActivityLayerDto> layerDtos)
        {
            _personalActivityLayerAssembler.DtosToDomainEntities(layerDtos).ForEach(personalShift.LayerCollection.Add);
        }

        private void addLayersToOvertimeShift(IOvertimeShift overtimeShift, IEnumerable<OvertimeLayerDto> layerDtos)
        {
            _overtimeShiftLayerAssembler.DtosToDomainEntities(layerDtos).ForEach(overtimeShift.LayerCollection.Add);
        }

        private MainShiftDto CreateMainShiftDto(IEnumerable<IMainShiftLayer> mainShiftLayers, IShiftCategory shiftCategory, IPerson shiftOwner)
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

        private ShiftDto CreatePersonalShiftDto(IPersonalShift personalShift, IPerson shiftOwner)
        {
            ShiftDto retDto = new ShiftDto();
            retDto.Id = personalShift.Id.Value;

            _personalActivityLayerAssembler.SetCurrentPerson(shiftOwner);
            var layers = _personalActivityLayerAssembler.DomainEntitiesToDtos(personalShift.LayerCollection.OfType<IPersonalShiftActivityLayer>());
            layers.ForEach(retDto.LayerCollection.Add);

            return retDto;
        }

        private ShiftDto CreateOvertimeShiftDto(IOvertimeShift overtimeShift, IPerson shiftOwner)
        {
            ShiftDto retDto = new ShiftDto();
            retDto.Id = overtimeShift.Id.Value;
            
            _overtimeShiftLayerAssembler.SetCurrentPerson(shiftOwner);
            var layers = _overtimeShiftLayerAssembler.DomainEntitiesToDtos(overtimeShift.LayerCollection.OfType<IOvertimeShiftActivityLayer>());
            layers.ForEach(retDto.LayerCollection.Add);

            return retDto;
        }
    }
}