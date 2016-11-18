using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class ScheduleStorageRepositoryWrapper : IScheduleStorageRepositoryWrapper
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		private readonly IDictionary<Type, Func<object>> _mappings;

		public ScheduleStorageRepositoryWrapper(IRepositoryFactory repositoryFactory, ICurrentUnitOfWork currentUnitOfWork)
		{
			_repositoryFactory = repositoryFactory;
			_currentUnitOfWork = currentUnitOfWork;

			_mappings = new Dictionary<Type, Func<object>>
			{
				{typeof(IPersonAssignment), () => _repositoryFactory.CreatePersonAssignmentRepository(_currentUnitOfWork.Current())},
				{typeof(IPersonAbsence), () => _repositoryFactory.CreatePersonAbsenceRepository(_currentUnitOfWork.Current())},
				{typeof(IPreferenceDay), () => _repositoryFactory.CreatePreferenceDayRepository(_currentUnitOfWork.Current())},
				{typeof(INote), () => _repositoryFactory.CreateNoteRepository(_currentUnitOfWork.Current())},
				{typeof(IStudentAvailabilityDay), () => _repositoryFactory.CreateStudentAvailabilityDayRepository(_currentUnitOfWork.Current())},
				{typeof(IPublicNote), () => _repositoryFactory.CreatePublicNoteRepository(_currentUnitOfWork.Current())},
				{typeof(IAgentDayScheduleTag), () => _repositoryFactory.CreateAgentDayScheduleTagRepository(_currentUnitOfWork.Current())},
				{typeof(IOvertimeAvailability), () => _repositoryFactory.CreateOvertimeAvailabilityRepository(_currentUnitOfWork.Current())}
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