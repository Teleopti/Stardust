﻿using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class UpdateStaffingLevelReadModelHandler : IHandleEvent<UpdateStaffingLevelReadModelEvent>, IRunOnStardust
	{
		private readonly IUpdateStaffingLevelReadModel _updateStaffingLevelReadModel;
		private readonly ICurrentUnitOfWorkFactory _currentFactory;
		private readonly INow _now;

		public UpdateStaffingLevelReadModelHandler(IUpdateStaffingLevelReadModel updateStaffingLevelReadModel, ICurrentUnitOfWorkFactory current, INow now)
		{
			_updateStaffingLevelReadModel = updateStaffingLevelReadModel;
			_currentFactory = current;
			_now = now;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(UpdateStaffingLevelReadModelEvent @event)
		{
			//start thread with time and set time to now + 1 min (or 2?)
			var period = new DateTimePeriod(_now.UtcDateTime().AddDays(-1).AddHours(-1), _now.UtcDateTime().AddDays(@event.Days).AddHours(1));

			_updateStaffingLevelReadModel.Update(period);
			
			var current = _currentFactory.Current().CurrentUnitOfWork();
			//an ugly solution for bug 39594
			if (current.IsDirty())
				current.Clear();
		}
	}
}
