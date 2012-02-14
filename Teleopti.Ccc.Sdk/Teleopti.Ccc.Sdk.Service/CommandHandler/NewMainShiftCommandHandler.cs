﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class NewMainShiftCommandHandler : IHandleCommand<NewMainShiftCommandDto>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IShiftCategoryRepository _shiftCategoryRepository;
        private readonly IActivityLayerAssembler<IMainShiftActivityLayer> _mainActivityLayerAssembler;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISaveSchedulePartService _saveSchedulePartService;

        public NewMainShiftCommandHandler(IUnitOfWorkFactory unitOfWorkFactory,IShiftCategoryRepository shiftCategoryRepository,IActivityLayerAssembler<IMainShiftActivityLayer> mainActivityLayerAssembler, IScheduleRepository scheduleRepository, IScenarioRepository scenarioRepository, IPersonRepository personRepository, ISaveSchedulePartService saveSchedulePartService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _shiftCategoryRepository = shiftCategoryRepository;
            _mainActivityLayerAssembler = mainActivityLayerAssembler;
            _scheduleRepository = scheduleRepository;
            _scenarioRepository = scenarioRepository;
            _personRepository = personRepository;
            _saveSchedulePartService = saveSchedulePartService;
        }

        public CommandResultDto Handle(NewMainShiftCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = _scenarioRepository.LoadDefaultScenario();
                var startDate = new DateOnly(command.Date.DateTime);
                var timeZone = person.PermissionInformation.DefaultTimeZone();
                var scheduleDictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                    new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(false, false),
                    new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario);
                var scheduleDay = scheduleDictionary[person].ScheduledDay(startDate);
                var shiftCategory = _shiftCategoryRepository.Load(command.ShiftCategoryId);
                var mainShift = new MainShift(shiftCategory);
                addLayersToMainShift(mainShift, command.LayerCollection);
                scheduleDay.AddMainShift(mainShift);
                _saveSchedulePartService.Save(uow, scheduleDay);
                using (new MessageBrokerSendEnabler())
                {
                    uow.PersistAll();
                }
            }
            return new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

        private void addLayersToMainShift(IMainShift mainShift, IEnumerable<ActivityLayerDto> layerDtos)
        {
            _mainActivityLayerAssembler.DtosToDomainEntities(layerDtos).ForEach(mainShift.LayerCollection.Add);
        }
    }
}
