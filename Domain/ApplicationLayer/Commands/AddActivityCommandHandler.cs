using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private readonly IProxyForId<IActivity> _activityForId;
		private readonly IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> _personAssignmentRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IProxyForId<IPerson> _personForId;
		private readonly IUserTimeZone _timeZone;
		private readonly IShiftCategoryRepository _shiftCategoryRepository;

		public AddActivityCommandHandler(IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey> personAssignmentRepository, ICurrentScenario currentScenario, IProxyForId<IActivity> activityForId, IProxyForId<IPerson> personForId, IUserTimeZone timeZone, IShiftCategoryRepository shiftCategoryRepository)
		{
			_activityForId = activityForId;
			_personAssignmentRepository = personAssignmentRepository;
			_currentScenario = currentScenario;
			_personForId = personForId;
			_timeZone = timeZone;
			_shiftCategoryRepository = shiftCategoryRepository;
		}

		public void Handle(AddActivityCommand command)
		{
			var activity = _activityForId.Load(command.ActivityId);
			var person = _personForId.Load(command.PersonId);
			var scenario = _currentScenario.Current();
			var personAssignment = _personAssignmentRepository.LoadAggregate(new PersonAssignmentKey
				{
					Date = command.Date,
					Scenario = scenario,
					Person = person
				});
			
			var period = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()), TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			if (personAssignment == null)
			{
				var newPersonAssignment = new PersonAssignment(person, scenario, command.Date);
				newPersonAssignment.AddActivity(activity, period, command.TrackedCommandInfo);
				var shiftCategories = _shiftCategoryRepository.FindAll().ToList();
				shiftCategories.Sort(new ShiftCategorySorter());
				var shiftCategory = shiftCategories.FirstOrDefault();
				if (shiftCategory != null)
				{
					newPersonAssignment.SetShiftCategory(shiftCategory);
					_personAssignmentRepository.Add(newPersonAssignment);
				}
			}
			else
			{
				personAssignment.AddActivity(activity, period, command.TrackedCommandInfo);
			}
		}
	}
}