﻿using System;
using System.Linq;
using Microsoft.Owin.Logging;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Wfm.Api.Command
{
	public class SetMainShiftHandler : ICommandHandler<SetMainShiftDto>
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(SetMainShiftHandler));

		public SetMainShiftHandler(IScenarioRepository scenarioRepository, 
			IPersonRepository personRepository, IActivityRepository activityRepository, 
			IPersonAssignmentRepository personAssignmentRepository, 
			IShiftCategoryRepository shiftCategoryRepository)
		{
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_activityRepository = activityRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_shiftCategoryRepository = shiftCategoryRepository;
		}

		[UnitOfWork]
		public virtual ResultDto Handle(SetMainShiftDto command)
		{
			try
			{
				var scenario = command.ScenarioId == null
					? _scenarioRepository.LoadDefaultScenario()
					: _scenarioRepository.Load(command.ScenarioId.GetValueOrDefault());
				var person = _personRepository.Load(command.PersonId);
				var dateOnly = command.Date.ToDateOnly();

				var assignment = getPersonAssignment(person, dateOnly, scenario, command.ShiftCategory);
				assignment.ClearMainActivities();

				foreach (var layer in command.LayerCollection)
				{
					var activity = _activityRepository.Load(layer.ActivityId);
					assignment.AddActivity(activity, new DateTimePeriod(layer.UtcStartDateTime.Utc(), layer.UtcEndDateTime.Utc()),
						true);
				}

				throw new Exception("this is a message");
				return new ResultDto
				{
					Successful = true
				};
			}
			catch (Exception e)
			{
				logger.Error(e.Message + e.StackTrace);
				return new ResultDto
				{
					Successful = false
				};
			}
		}

		private IPersonAssignment getPersonAssignment(IPerson person, DateOnly dateOnly, IScenario scenario, Guid shiftCategoryId)
		{
			IPersonAssignment assignment;
			var currentAssignments = _personAssignmentRepository.Find(new[] { person }, dateOnly.ToDateOnlyPeriod(), scenario);
			if (currentAssignments.IsEmpty())
			{
				assignment = new PersonAssignment(person, scenario, dateOnly);
				_personAssignmentRepository.Add(assignment);
			}
			else
			{
				assignment = currentAssignments.First();
			}

			var shiftCategory = _shiftCategoryRepository.Get(shiftCategoryId);
			if (shiftCategory != null)
				assignment.SetShiftCategory(shiftCategory);
			return assignment;
		}
	}
}