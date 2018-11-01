﻿using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;


/* DO NOT USE! 
It only contains vary simple logic. 
Used for staffhub integration only until further notice */

//TODO personal account 
namespace Teleopti.Wfm.Api.Command
{
	public class AddIntradayAbsenceRequestHandler : ICommandHandler<AddIntradayAbsenceRequestDto>
	{
		private readonly IAbsenceRequestPersister _absenceRequestPersister;
		
		public AddIntradayAbsenceRequestHandler(IAbsenceRequestPersister absenceRequestPersister)
		{
			_absenceRequestPersister = absenceRequestPersister;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(AddIntradayAbsenceRequestDto command)
		{
			if (command.UtcStartTime >= command.UtcEndTime)
				return new ResultDto
				{
					Successful = false,
					Message = "UtcEndTime must be greater than UtcStartTime"
				};
			
			var dateTimePeriod = new DateTimePeriod(command.UtcStartTime.Utc(), command.UtcEndTime.Utc());

			var request = _absenceRequestPersister.Persist(new AbsenceRequestModel
			{
				AbsenceId = command.AbsenceId,
				FullDay = false,
				Message = command.Message,
				Period = dateTimePeriod,
				Subject = command.Subject,
				PersonId = command.PersonId
			});

			return new ResultDto
			{
				Id = request.Id,
				Successful = true
			};
		}
	}
}