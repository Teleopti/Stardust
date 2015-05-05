﻿using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class VerifyTerminalDate : IVerifyTerminalDate
	{
		private readonly IApplicationData _applicationData;

		public VerifyTerminalDate(IApplicationData applicationData)
		{
			_applicationData = applicationData;
		}

		public bool IsTerminated(string tenantName, Guid personId)
		{
			var tenant = _applicationData.DataSource(tenantName);
			using (var uow = tenant.Application.CreateAndOpenUnitOfWork())
			{
				var person = new PersonRepository(uow).Get(personId);
				var terminalDate = person.TerminalDate;
				return terminalDate.HasValue && terminalDate.Value < DateOnly.Today;
			}
		}
	}
}