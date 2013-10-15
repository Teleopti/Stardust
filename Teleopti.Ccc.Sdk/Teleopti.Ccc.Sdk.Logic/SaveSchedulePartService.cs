﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic
{
    public interface ISaveSchedulePartService
    {
		void Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag);
    }

    public class SaveSchedulePartService : ISaveSchedulePartService
    {
        private readonly IScheduleDictionarySaver _scheduleDictionarySaver;
        private readonly IScheduleRepository _scheduleRepository;
    	private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private readonly ICurrentUnitOfWork _unitOfWorkFactory;

    	public SaveSchedulePartService(IScheduleDictionarySaver scheduleDictionarySaver, IScheduleRepository scheduleRepository, IPersonAbsenceAccountRepository personAbsenceAccountRepository, ICurrentUnitOfWork unitOfWorkFactory)
        {
            _scheduleDictionarySaver = scheduleDictionarySaver;
            _scheduleRepository = scheduleRepository;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
    		_unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Save(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTag scheduleTag)
        {
            var dic = (IReadOnlyScheduleDictionary)scheduleDay.Owner;
            dic.MakeEditable();

			IEnumerable<IBusinessRuleResponse> invalidList = dic.Modify(ScheduleModifier.Scheduler,
			                                     scheduleDay, newBusinessRuleCollection, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(scheduleTag));

            if (invalidList != null && invalidList.Any())
			{
				throw new FaultException(
					string.Format(System.Globalization.CultureInfo.InvariantCulture, "At least one business rule was broken. Messages are: {0}{1}", Environment.NewLine,
					              string.Join(Environment.NewLine,
					                          invalidList.Select(i => i.Message).Distinct().ToArray())));
			}

			_scheduleDictionarySaver.MarkForPersist(_unitOfWorkFactory.Current(), _scheduleRepository, dic.DifferenceSinceSnapshot());
			_personAbsenceAccountRepository.AddRange(dic.ModifiedPersonAccounts);
        }
    }
}
