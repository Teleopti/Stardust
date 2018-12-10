using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class ScheduleFactory
    {
        private readonly IScheduleStorage _scheduleStorage;
	    private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
    	private readonly IPersonRepository _personRepository;
        private readonly IAssembler<IScheduleDay, SchedulePartDto> _scheduleDayAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
        private readonly ICurrentScenario _scenarioRepository;
		
	    public ScheduleFactory(IScheduleStorage scheduleStorage,
			  ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IAssembler<IScheduleDay, 
			  SchedulePartDto> scheduleDayAssembler, IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler,
			  ICurrentScenario scenarioRepository)
        {
            _scheduleStorage = scheduleStorage;
		    _unitOfWorkFactory = unitOfWorkFactory;
        	_personRepository = personRepository;
            _scheduleDayAssembler = scheduleDayAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
            _scenarioRepository = scenarioRepository;
        }

        internal ICollection<MultiplicatorDataDto> CreateMultiplicatorData(ICollection<PersonDto> personCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneInfoId)
        {
            IList<MultiplicatorDataDto> multiplicatorDataDtos = new List<MultiplicatorDataDto>();

            var datePeriod = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
            var period = new DateOnlyPeriod(datePeriod.StartDate, datePeriod.EndDate.AddDays(1));
            
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var personList = _personRepository.FindPeople(personCollection.Select(p => p.Id.GetValueOrDefault()).ToArray());
				IScheduleDictionary scheduleDictonary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(false, false), period, _scenarioRepository.Current());

                //rk don't know if I break stuff here...
                //scheduleDictonary.SetTimeZone(timeZone);
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictonary[person];
                    foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(datePeriod))
                    {
                        var multiplicatorProjectionService = new MultiplicatorProjectionService(scheduleDay, scheduleDay.DateOnlyAsPeriod.DateOnly);
                        //Create Activity Layers
                        foreach (IMultiplicatorLayer layer in multiplicatorProjectionService.CreateProjection())
                        {
                            var multiplicatorDataDto = new MultiplicatorDataDto
                                                           {
                                                               Date = scheduleDay.DateOnlyAsPeriod.DateOnly.Date,
                                                               ActualDate =
                                                                   layer.Period.StartDateTimeLocal(scheduleDay.TimeZone)
                                                                   .Date,
                                                               Amount = layer.Period.ElapsedTime(),
                                                               Multiplicator = new MultiplicatorDto{	
																										Id = layer.Payload.Id, 
																										PayrollCode = layer.Payload.ExportCode,
															                                            Color = new ColorDto(layer.Payload.DisplayColor),
																										Multiplicator = layer.Payload.MultiplicatorValue,
																										MultiplicatorType = (MultiplicatorTypeDto) layer.Payload.MultiplicatorType,
																										Name = layer.Payload.Description.Name
																								     },
																										
                                                               PersonId = person.Id
                                                           };

                            multiplicatorDataDtos.Add(multiplicatorDataDto);
                        }
                    }
                }
            }

            return multiplicatorDataDtos;
        }

		public ICollection<SchedulePartDto> CreateSchedulePartCollection(IEnumerable<PersonDto> personCollection, 
																		DateOnlyDto startDate, 
																		DateOnlyDto endDate, 
																		string timeZoneInfoId,
																		string specialProjection)
        {
            IList<SchedulePartDto> returnList = new List<SchedulePartDto>();

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
            var datePeriod = new DateOnlyPeriod(startDate.ToDateOnly(), endDate.ToDateOnly());
            var period = new DateOnlyPeriod(datePeriod.StartDate,datePeriod.EndDate.AddDays(1));

            ((DateTimePeriodAssembler) _dateTimePeriodAssembler).TimeZone = timeZone;
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                IList<IPerson> personList = _personRepository.FindPeople(personCollection.Select(p => p.Id.GetValueOrDefault())).ToList();

				IScheduleDictionary scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(true, false), period, _scenarioRepository.Current());
                foreach (IPerson person in personList)
                {
                    IScheduleRange scheduleRange = scheduleDictionary[person];
                    foreach (var part in scheduleRange.ScheduledDayCollection(datePeriod))
                    {
                    	var schedAss = (SchedulePartAssembler) _scheduleDayAssembler;
						schedAss.SpecialProjection = specialProjection;
                    	schedAss.TimeZone = timeZone;
                        returnList.Add(_scheduleDayAssembler.DomainEntityToDto(part));
                    }
                }
            }

            return returnList;
        }
    }
}
