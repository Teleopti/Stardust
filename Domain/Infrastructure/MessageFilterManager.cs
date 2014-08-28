// ReSharper restore MemberCanBeMadeStatic

using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Resources;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class MessageFilterManager : IMessageFilterManager
	{
		private static readonly Lazy<MessageFilterManager> _messageFilterManager = new Lazy<MessageFilterManager>(() => new MessageFilterManager(Initialise()));
		private readonly IDictionary<Type, IList<Type>> _aggregateRoots;
		private readonly ReaderWriterLock _readerWriterLock = new ReaderWriterLock();
		private int _timeOut = 20;

		private MessageFilterManager(IDictionary<Type, IList<Type>> aggregateRoots)
		{
			_aggregateRoots = aggregateRoots;
		}

		public static MessageFilterManager Instance
		{
			get
			{
				return _messageFilterManager.Value;
			}
		}

	    public bool HasType(Type type)
	    {
	        return _aggregateRoots.ContainsKey(type);
	    }

	    public string LookupTypeToSend(Type domainObjectType)
	    {
            return lookupType(domainObjectType, list => list[list.Count - 1]).AssemblyQualifiedName;
	    }
        
        public Type LookupType(Type domainObjectType)
        {
            return lookupType(domainObjectType, list => list[0]);
        }

	    /// <summary>
	    /// Looks up type.
	    /// </summary>
	    /// <param name="domainObjectType">Type of the domain object.</param>
	    /// <param name="typeFinder"></param>
	    /// <returns></returns>
	    private Type lookupType(Type domainObjectType, Func<IList<Type>, Type> typeFinder)
		{
			try
			{
				_readerWriterLock.AcquireReaderLock(_timeOut);
				try
				{
					IList<Type> foundTypes;
					if (_aggregateRoots.TryGetValue(domainObjectType,out foundTypes))
					{
						return typeFinder(foundTypes);
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
				lookupType(domainObjectType, typeFinder);
			}
			throw new DomainObjectNotInFilterException("Cannot find type " + domainObjectType.AssemblyQualifiedName);
		}

		private static IDictionary<Type, IList<Type>> Initialise()
		{
			IDictionary<Type, IList<Type>> aggregateRoots = new Dictionary<Type, IList<Type>>();
			AddTypeFilterExceptions(aggregateRoots);
			return aggregateRoots;
		}

		private static void AddTypeFilterExceptions(IDictionary<Type, IList<Type>> aggregateRoots)
		{
			aggregateRoots.Add(typeof (IStatisticTask), new List<Type> {typeof (IStatisticTask)});
			aggregateRoots.Add(typeof (IActualAgentState), new List<Type> {typeof (IActualAgentState)});
			aggregateRoots.Add(typeof (IJobResultProgress), new List<Type> {typeof (IJobResultProgress)});
			aggregateRoots.Add(typeof (IMeetingChangedEntity), new List<Type> {typeof (IMeetingChangedEntity)});
			aggregateRoots.Add(typeof (MeetingChangedEntity), new List<Type> {typeof (IMeetingChangedEntity)});
			aggregateRoots.Add(typeof (IScheduleChangedInDefaultScenario),
							   new List<Type> {typeof (IScheduleChangedInDefaultScenario)});
			aggregateRoots.Add(typeof(ITeleoptiDiagnosticsInformation),
							   new List<Type> { typeof(ITeleoptiDiagnosticsInformation) });
			aggregateRoots.Add(typeof (IPersonScheduleDayReadModel),
							   new List<Type> {typeof (IPersonScheduleDayReadModel)});
			aggregateRoots.Add(typeof (IScheduleChangedEvent), new List<Type> {typeof (IScheduleChangedEvent)});
	        aggregateRoots.Add(typeof (IScheduledResourcesReadModel),
	                           new List<Type> {typeof (IScheduledResourcesReadModel)});

			aggregateRoots.Add(typeof (Person), new List<Type> {typeof (IPerson)});
			aggregateRoots.Add(typeof (Scenario), new List<Type> {typeof (IScenario)});

			aggregateRoots.Add(typeof (Skill), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (Workload), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (SkillDay), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (MultisiteDay), new List<Type> {typeof (IForecastData)});

			aggregateRoots.Add(typeof(Note), new List<Type> { typeof(IPersistableScheduleData), typeof(INote)});
			aggregateRoots.Add(typeof(AgentDayScheduleTag), new List<Type> { typeof(IPersistableScheduleData), typeof(IAgentDayScheduleTag) });
			//aggregateRoots.Add(typeof(PersonAssignment), new List<Type> { typeof(IPersistableScheduleData) });
			//aggregateRoots.Add(typeof(PersonAbsence), new List<Type> { typeof(IPersistableScheduleData) });
			aggregateRoots.Add(typeof(PublicNote), new List<Type> { typeof(IPersistableScheduleData), typeof(IPublicNote) });
			aggregateRoots.Add(typeof(PreferenceDay), new List<Type> { typeof(IPersistableScheduleData), typeof(IPreferenceDay) });
			aggregateRoots.Add(typeof(StudentAvailabilityDay), new List<Type> { typeof(IPersistableScheduleData), typeof(IStudentAvailabilityDay) });

			aggregateRoots.Add(typeof (Multiplicator), new List<Type> {typeof (IMultiplicator)});
			aggregateRoots.Add(typeof (MultiplicatorDefinitionSet),
							   new List<Type> {typeof (IMultiplicatorDefinitionSet)});
			aggregateRoots.Add(typeof (DayOffTemplate), new List<Type> {typeof (IDayOffTemplate)});
			aggregateRoots.Add(typeof (ShiftCategory), new List<Type> {typeof (IShiftCategory)});
			aggregateRoots.Add(typeof (Meeting), new List<Type> {typeof (IMeeting)});
			aggregateRoots.Add(typeof (PersonRequest), new List<Type> {typeof (IPersonRequest)});
			aggregateRoots.Add(typeof (RtaStateGroup), new List<Type> {typeof (IRtaStateGroup)});
			aggregateRoots.Add(typeof (Activity), new List<Type> {typeof (IActivity)});
			aggregateRoots.Add(typeof (AlarmType), new List<Type> {typeof (IAlarmType)});
			aggregateRoots.Add(typeof (Team), new List<Type> {typeof (ITeam)});
			aggregateRoots.Add(typeof (Site), new List<Type> {typeof (ISite)});
			aggregateRoots.Add(typeof (Scorecard), new List<Type> {typeof (IScorecard)});
			aggregateRoots.Add(typeof (PayrollResult), new List<Type> {typeof (IPayrollResult)});
			aggregateRoots.Add(typeof (PushMessageDialogue), new List<Type> {typeof (IPushMessageDialogue)});
		}
	}
}