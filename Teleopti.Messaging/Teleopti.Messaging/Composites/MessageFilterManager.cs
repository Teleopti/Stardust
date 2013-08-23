// ReSharper restore MemberCanBeMadeStatic
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.Composites
{
    public class MessageFilterManager : IMessageFilterManager
    {
        private const string TeleoptiDomainConstant = "Teleopti.Ccc.Domain";
        private const string TeleoptiInterfacesConstant = "Teleopti.Interfaces";
        private static readonly object _lockObject = new object();
        private static MessageFilterManager _messageFilterManager;
        private IDictionary<Type, IList<Type>> _aggregateRoots;
        private readonly ReaderWriterLock _readerWriterLock = new ReaderWriterLock();
        private int _timeOut = 20;

        public MessageFilterManager()
        {
        }

        private MessageFilterManager(IDictionary<Type, IList<Type>> aggregateRoots)
        {
            _aggregateRoots = aggregateRoots;
        }

        public static MessageFilterManager Instance
        {
            get
            {
                lock (_lockObject)
                {
                    if (_messageFilterManager == null)
                    {
                        _messageFilterManager = new MessageFilterManager(Initialise());
                    }
                }
                return _messageFilterManager;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IDictionary<Type, IList<Type>> FilterDictionary
        {
            get { return _aggregateRoots; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public void InitializeTypeFilter(IDictionary<Type, IList<Type>> typeFilter)
        {
            _aggregateRoots = typeFilter;
        }

        /// <summary>
        /// Looks up type.
        /// </summary>
        /// <param name="domainObjectType">Type of the domain object.</param>
        /// <returns></returns>
        public string LookupType(Type domainObjectType)
        {
            try
            {
                _readerWriterLock.AcquireReaderLock(_timeOut);
                try
                {
                    IList<Type> foundTypes;
                    if (_aggregateRoots.TryGetValue(domainObjectType,out foundTypes))
                    {
                        return foundTypes[0].AssemblyQualifiedName;
                    }
                }
                finally
                {
                    // Ensure that the lock is released.
                    _readerWriterLock.ReleaseReaderLock();
                }
            }
            catch (ApplicationException)
            {
                // The reader lock request timed out.
                Interlocked.Increment(ref _timeOut);
                LookupType(domainObjectType);
            }
            throw new DomainObjectNotInFilterException("Cannot find type " + domainObjectType.AssemblyQualifiedName);
        }

        private static IDictionary<Type, IList<Type>> Initialise()
        {
            IDictionary<Type, IList<Type>> aggregateRoots = new Dictionary<Type, IList<Type>>();
            IList<Type> types = new List<Type>();
            Assembly assembly = Assembly.Load(TeleoptiDomainConstant);
            // Gets all types from the domain assembly
            GetTypesFromAssembly(assembly, types);
            // that are assignable from IAggregateRoot
            GetAggregateRoots(types, aggregateRoots);
            // Get all interfaces for the aggregate root implementers
            GetInterfacesForAggregateRoots(aggregateRoots);
            // Remove interfaces that clients do not have and remove
            // the IAggregateRoot interface itself, and furthermore
            // remove generic interfaces.
            GetRelevantInterfaces(aggregateRoots);
            // Remove types with empty interface lists.
            GetNonEmptyInterfaceLists(aggregateRoots);
            // Last but not least, 
            // add interfaces that do not inherit IAggregate root.
            AddTypeFilterExceptions(aggregateRoots);
            return aggregateRoots;
        }

        private static void AddTypeFilterExceptions(IDictionary<Type, IList<Type>> aggregateRoots)
        {
            aggregateRoots.Add(typeof(IStatisticTask), new List<Type> { typeof(IStatisticTask) });
            aggregateRoots.Add(typeof(IActualAgentState), new List<Type> { typeof(IActualAgentState) });
            aggregateRoots.Add(typeof(IJobResultProgress), new List<Type> { typeof(IJobResultProgress) });
			aggregateRoots.Add(typeof(IMeetingChangedEntity),new List<Type>{typeof(IMeetingChangedEntity)});
			aggregateRoots.Add(typeof(MeetingChangedEntity),new List<Type>{typeof(IMeetingChangedEntity)});
			aggregateRoots.Add(typeof(IScheduleChangedInDefaultScenario), new List<Type> { typeof(IScheduleChangedInDefaultScenario) });
			aggregateRoots.Add(typeof(IPersonScheduleDayReadModel), new List<Type> { typeof(IPersonScheduleDayReadModel) });
		}

        private static void GetNonEmptyInterfaceLists(ICollection<KeyValuePair<Type, IList<Type>>> aggregateRoots)
        {

            IDictionary<Type, IList<Type>> rootsToRemove = new Dictionary<Type, IList<Type>>();

            foreach (KeyValuePair<Type, IList<Type>> keyValuePair in aggregateRoots)
            {
                if (keyValuePair.Value.Count == 0)
                    rootsToRemove.Add(keyValuePair);
            }

            foreach (KeyValuePair<Type, IList<Type>> keyValuePair in rootsToRemove)
                aggregateRoots.Remove(keyValuePair);

        }

        private static void GetRelevantInterfaces(IEnumerable<KeyValuePair<Type, IList<Type>>> aggregateRoots)
        {
            foreach (KeyValuePair<Type, IList<Type>> kvp in aggregateRoots)
            {
                IList<Type> list = kvp.Value;

                RemoveNonAggregateRootInheritedInterfaces(list);

                RemoveNonDistributedInterfaces(list);

                RemoveAggregateRootInterface(list);
            }
        }

        private static void GetInterfacesForAggregateRoots(IDictionary<Type, IList<Type>> aggregateRoots)
        {
            foreach (Type aggregateRoot in aggregateRoots.Keys)
            {
                IList<Type> list = aggregateRoots[aggregateRoot];
                foreach (Type intrfce in aggregateRoot.GetInterfaces())
                    list.Add(intrfce);
            }
        }

        private static void GetAggregateRoots(IEnumerable<Type> types, IDictionary<Type, IList<Type>> aggregateRoots)
        {
            foreach (Type typ in types)
                if (typeof(IAggregateRoot).IsAssignableFrom(typ))
                    aggregateRoots.Add(typ, new List<Type>());
        }

        private static void GetTypesFromAssembly(Assembly assembly, ICollection<Type> types)
        {
            foreach (Type typ in assembly.GetTypes())
                types.Add(typ);
        }

        private static void RemoveNonDistributedInterfaces(ICollection<Type> list)
        {
            IList<Type> typesToRemove = new List<Type>();
            foreach (Type typ in list)
            {
                if (!typ.FullName.StartsWith(TeleoptiInterfacesConstant, StringComparison.CurrentCulture))
                    typesToRemove.Add(typ);
            }
            foreach (Type typ in typesToRemove)
                list.Remove(typ);
        }

        private static void RemoveNonAggregateRootInheritedInterfaces(ICollection<Type> list)
        {
            IList<Type> typesToRemove = new List<Type>();
            foreach (Type type in list)
            {
                if (!typeof(IAggregateRoot).IsAssignableFrom(type) || type.IsGenericType)
                    typesToRemove.Add(type);
            }
            foreach (Type typ in typesToRemove)
                list.Remove(typ);
        }

        private static void RemoveAggregateRootInterface(ICollection<Type> list)
        {
            IList<Type> typesToRemove = new List<Type>();
            foreach (Type type in list)
            {
                if (type.Name == "IAggregateRoot")
                    typesToRemove.Add(type);
            }
            foreach (Type typ in typesToRemove)
                list.Remove(typ);
        }

    }
}