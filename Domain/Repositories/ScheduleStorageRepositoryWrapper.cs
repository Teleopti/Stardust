using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class ScheduleStorageRepositoryWrapper : IScheduleStorageRepositoryWrapper
	{
		private readonly IDictionary<Type, Func<object>> _mappings;

		public ScheduleStorageRepositoryWrapper(Func<IPersonAssignmentRepository> personAssignmentRepository,
			Func<IPersonAbsenceRepository> personAbsenceRepository,
			Func<IPreferenceDayRepository> preferenceDayRepository, Func<INoteRepository> noteRepository,
			Func<IPublicNoteRepository> publicNoteRepository,
			Func<IStudentAvailabilityDayRepository> studentAvailabilityDayRepository,
			Func<IAgentDayScheduleTagRepository> agentDayScheduleTagRepository,
			Func<IOvertimeAvailabilityRepository> overtimeAvailabilityRepository)
		{
			_mappings = new Dictionary<Type, Func<object>>
			{
				{typeof(IPersonAssignment), personAssignmentRepository},
				{typeof(IPersonAbsence), personAbsenceRepository},
				{typeof(IPreferenceDay), preferenceDayRepository},
				{typeof(INote), noteRepository},
				{typeof(IStudentAvailabilityDay), studentAvailabilityDayRepository},
				{typeof(IPublicNote), () => publicNoteRepository},
				{typeof(IAgentDayScheduleTag), agentDayScheduleTagRepository},
				{typeof(IOvertimeAvailability), overtimeAvailabilityRepository}
			};
		}

		private object getRepository(Type type)
		{
			if (!typeof(IPersistableScheduleData).IsAssignableFrom(type))
				throw new ArgumentException("Only IPersistableScheduleData types are allowed");
			var mappableType = _mappings.Keys.FirstOrDefault(x => x.IsAssignableFrom(type));
			if (mappableType == null)
				throw new NotImplementedException($"Missing repository definition for type {type}");

			return _mappings[mappableType]();
		}

		public void Add(IPersistableScheduleData item)
		{
			if (item == null) return;
			var repository = getRepository(item.GetType());

			var method = makeGenericMethod(typeof(IRepository<>), item.GetType(), nameof(IRepository<int>.Add));
			method.Invoke(repository, new object[] { item });
		}

		public void Remove(IPersistableScheduleData item)
		{
			if (item == null) return;
			var repository = getRepository(item.GetType());

			var method = makeGenericMethod(typeof(IRepository<>), item.GetType(), nameof(IRepository<int>.Remove));
			method.Invoke(repository, new object[] { item });
		}

		public IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id)
		{
			var repository = getRepository(scheduleDataType);
			var method = makeGenericMethod(typeof(ILoadAggregateFromBroker<>), scheduleDataType, nameof(ILoadAggregateFromBroker<int>.LoadAggregate));
			return (IPersistableScheduleData)method.Invoke(repository, new object[] { id });
		}

		public IPersistableScheduleData Get(Type scheduleDataType, Guid id)
		{
			var repository = getRepository(scheduleDataType);
			var method = makeGenericMethod(typeof(IRepository<>), scheduleDataType, nameof(IRepository<int>.Get));
			return (IPersistableScheduleData)method.Invoke(repository, new object[] { id });
		}

		private static MethodInfo makeGenericMethod(Type repositoryType, Type type, string methodName)
		{
			Type genericType;
			if (type.IsInterface)
			{
				genericType = repositoryType.MakeGenericType(type);
			}
			else
			{
				var interfaceType = type.GetInterfaces().FirstOrDefault(x => x.Name == $"I{type.Name}");
				genericType = repositoryType.MakeGenericType(interfaceType);
			}
			return genericType.GetMethod(methodName);
		}
	}
}