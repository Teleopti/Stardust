using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IGroupPersonBuilderBasedOnContractTime
    {
        IList<IGroupPerson> SplitTeams(IList<IPerson > groupMembers, DateOnly dateOnly);
    }

    public  class GroupPersonBuilderBasedOnContractTime : IGroupPersonBuilderBasedOnContractTime
    {
        
        private readonly IGroupPersonFactory _groupPersonFactory;
        public GroupPersonBuilderBasedOnContractTime(IGroupPersonFactory groupPersonFactory)
        {
            _groupPersonFactory = groupPersonFactory;
        }

        public IList<IGroupPerson> SplitTeams(IList<IPerson> groupMembers, DateOnly dateOnly)
        {
            var groupPersonList = new List<IGroupPerson>();
            var differentContractTimes = new Dictionary<TimeSpan,List<IPerson >>();
            if (groupMembers.Count > 0)
                foreach (var person in groupMembers)
                {
                    var personPeriod= person.Period(dateOnly);
                    if (personPeriod == null) continue ;
                    var contract = personPeriod.PersonContract.Contract;
                    var contractTime = new TimeSpan( );
                    switch (contract.WorkTimeSource)
                    {
                        case WorkTimeSource.FromContract:
                            contractTime= contract.WorkTime.AvgWorkTimePerDay;
                            break;
                        case WorkTimeSource.FromSchedulePeriod:
                            {
                                var schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
                                contractTime =  schedulePeriod == null
                                                    ? WorkTime.DefaultWorkTime.AvgWorkTimePerDay
                                                    : schedulePeriod.AverageWorkTimePerDay;
                            }
                            break;
                    }
                    if(!differentContractTimes.ContainsKey(contractTime ))
                        differentContractTimes.Add(contractTime, new List<IPerson>() );
                    differentContractTimes[contractTime ].Add(person );
                   
                }
            foreach(var personList in differentContractTimes.Values   )
            {
                //var guid = new Guid();
                groupPersonList.Add(_groupPersonFactory.CreateGroupPerson(personList, dateOnly, "TestName", null));
            }
            return groupPersonList;
        }

       
    }
}