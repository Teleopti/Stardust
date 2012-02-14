using System;
using System.Collections.Generic;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class ScheduleLoader
    {
        private IList<Agent> _agentsToLoad;
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private ServiceApplication _service;
        private TimeZoneInfo _timeZoneInfo;


        public ScheduleLoader(ServiceApplication service, IList<Agent> agentsToLoad, 
            DateTime dateFrom, DateTime dateTo, TimeZoneInfo timeZoneInfo)
        {
            _service = service;
            _agentsToLoad = agentsToLoad;
            _dateFrom = dateFrom;
            _dateTo = dateTo;
            _timeZoneInfo = timeZoneInfo;
        }

        public IList<AgentDay> Load()
        {
            IList<AgentDay> ret = new List<AgentDay>();
            DateOnlyDto dateOnly = new DateOnlyDto();
            dateOnly.DateTime = _dateFrom;
            dateOnly.DateTimeSpecified = true;

            foreach (var agent in _agentsToLoad)
            {
                SchedulePartDto schedulePartDto = _service.SchedulingService.GetSchedulePart(agent.Dto, dateOnly,
                                                                                 _timeZoneInfo.Id);
                AgentDay agentDay = new AgentDay(agent,schedulePartDto);
                IList<PersonAssignmentDto> personAssignmentDtos = new List<PersonAssignmentDto>(schedulePartDto.PersonAssignmentCollection);

                foreach (var personAssignmentDto in personAssignmentDtos)
                {
                    if (personAssignmentDto.MainShift != null)
                    {
                        MainShift mainShift = new MainShift(personAssignmentDto.MainShift,personAssignmentDto);
                        agentDay.MainShift = mainShift;
                    }
                        
                    if (personAssignmentDto.OvertimeShiftCollection.Length>0)
                    {
                        OvertimeShift overtimeShift = new OvertimeShift(personAssignmentDto.OvertimeShiftCollection[0]);
                        agentDay.OvertimeShift = overtimeShift;
                    }
                }
                ret.Add(agentDay);
            }
            return ret;
        }

        public void LoadPublicNotes(IList<AgentDay> agentDays)
        {
            foreach (AgentDay agentDay in agentDays)
            {
                var loadOption = new PublicNoteLoadOptionDto();
                loadOption.LoadPerson = new PersonDto {Id = agentDay.Dto.PersonId};
                IList<PublicNoteDto> publicNotes = _service.SchedulingService.GetPublicNotes(loadOption, agentDay.Dto.Date, agentDay.Dto.Date);
                if (publicNotes != null && publicNotes.Count > 0)
                    agentDay.PublicNote = publicNotes[0].ScheduleNote;
            }
        }
    }
}
