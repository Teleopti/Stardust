﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class CancelOvertimeCommandHandler : IHandleCommand<CancelOvertimeCommandDto>
    {
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISaveSchedulePartService _saveSchedulePartService;
    	private readonly IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
    	private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

    	public CancelOvertimeCommandHandler(IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IScenarioRepository scenarioRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISaveSchedulePartService saveSchedulePartService, IMessageBrokerEnablerFactory messageBrokerEnablerFactory, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
        {
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scheduleRepository = scheduleRepository;
            _personRepository = personRepository;
            _scenarioRepository = scenarioRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _saveSchedulePartService = saveSchedulePartService;
        	_messageBrokerEnablerFactory = messageBrokerEnablerFactory;
    		_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Handle(CancelOvertimeCommandDto command)
        {
            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                var person = _personRepository.Load(command.PersonId);
                var scenario = getDesiredScenario(command);
                var dateTimePeriod = _dateTimePeriodAssembler.DtoToDomainEntity(command.Period);
            	var startDate = new DateOnly(command.Date.DateTime);
            	var timeZone = person.PermissionInformation.DefaultTimeZone();

                var scheduleDictionary =
                    _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
                        new PersonProvider(new[] {person}), new ScheduleDictionaryLoadOptions(false, false),
                        new DateOnlyPeriod(startDate, startDate.AddDays(1)).ToDateTimePeriod(timeZone), scenario);

				var scheduleRange = scheduleDictionary[person];
				var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
                var scheduleDay = scheduleRange.ScheduledDay(startDate);
                var personAssignmentCollection = scheduleDay.PersonAssignmentCollection();

            	foreach (var personAssignment in personAssignmentCollection)
            	{
            		var overtimeShifts = personAssignment.OvertimeShiftCollection.ToList();
            		foreach (var overtimeShift in overtimeShifts)
            		{
            			cancelOvertime(overtimeShift, dateTimePeriod);
						if (overtimeShift.LayerCollection.Count==0)
						{
							personAssignment.RemoveOvertimeShift(overtimeShift);
						}
            		}
            	}

                _saveSchedulePartService.Save(scheduleDay, rules);
                using (_messageBrokerEnablerFactory.NewMessageBrokerEnabler())
                {
                    uow.PersistAll();
                }

            }
			command.Result = new CommandResultDto { AffectedId = command.PersonId, AffectedItems = 1 };
        }

    	private IScenario getDesiredScenario(CancelOvertimeCommandDto command)
    	{
    		return command.ScenarioId.HasValue ? _scenarioRepository.Get(command.ScenarioId.Value) : _scenarioRepository.LoadDefaultScenario();
    	}

    	private static void cancelOvertime(IOvertimeShift overtimeShift, DateTimePeriod period)
		{
			var layers = overtimeShift.LayerCollection.ToList();
    		foreach (IOvertimeShiftActivityLayer layer in layers)
			{
				var layerPeriod = layer.Period;
				if (!layerPeriod.Intersect(period)) continue;

				overtimeShift.LayerCollection.Remove(layer);

				var newPeriods = layerPeriod.ExcludeDateTimePeriod(period);
				foreach (var dateTimePeriod in newPeriods)
				{
					if (dateTimePeriod.ElapsedTime()>TimeSpan.Zero)
					{
						var newLayer = new OvertimeShiftActivityLayer(layer.Payload, dateTimePeriod, layer.DefinitionSet);
						overtimeShift.LayerCollection.Add(newLayer);
					}
				}
			}
    	}
    }
}
