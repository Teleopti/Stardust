// ReSharper restore MemberCanBeMadeStatic

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private static readonly object _lockObject = new object();
		private static MessageFilterManager _messageFilterManager;
		private IDictionary<Type, IList<Type>> _aggregateRoots;
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
			aggregateRoots.Add(typeof (IPersonScheduleDayReadModel),
							   new List<Type> {typeof (IPersonScheduleDayReadModel)});
			aggregateRoots.Add(typeof (IScheduleChangedEvent), new List<Type> {typeof (IScheduleChangedEvent)});

			aggregateRoots.Add(typeof (Person), new List<Type> {typeof (IPerson)});
			aggregateRoots.Add(typeof (Scenario), new List<Type> {typeof (IScenario)});

			aggregateRoots.Add(typeof (Skill), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (Workload), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (SkillDay), new List<Type> {typeof (IForecastData)});
			aggregateRoots.Add(typeof (MultisiteDay), new List<Type> {typeof (IForecastData)});

			aggregateRoots.Add(typeof(Note), new List<Type> { typeof(IPersistableScheduleData) });
			aggregateRoots.Add(typeof(AgentDayScheduleTag), new List<Type> { typeof(IPersistableScheduleData) });
			//aggregateRoots.Add(typeof(PersonAssignment), new List<Type> { typeof(IPersistableScheduleData) });
			//aggregateRoots.Add(typeof(PersonAbsence), new List<Type> { typeof(IPersistableScheduleData) });
			aggregateRoots.Add(typeof(PublicNote), new List<Type> { typeof(IPersistableScheduleData) });
			aggregateRoots.Add(typeof(PreferenceDay), new List<Type> { typeof(IPersistableScheduleData) });
			aggregateRoots.Add(typeof(StudentAvailabilityDay), new List<Type> { typeof(IPersistableScheduleData) });

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