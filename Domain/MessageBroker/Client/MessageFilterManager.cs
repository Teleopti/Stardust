// ReSharper restore MemberCanBeMadeStatic

using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public class MessageFilterManager : IMessageFilterManager
	{
		private static readonly Lazy<MessageFilterManager> _messageFilterManager = new Lazy<MessageFilterManager>(() => new MessageFilterManager(initialise()));
		private readonly IDictionary<Type, IList<Type>> _aggregateRoots;
		private readonly ReaderWriterLock _readerWriterLock = new ReaderWriterLock();
		private int _timeOut = 20;

		private MessageFilterManager(IDictionary<Type, IList<Type>> aggregateRoots)
		{
			_aggregateRoots = aggregateRoots;
		}

		public static MessageFilterManager Instance => _messageFilterManager.Value;

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

		private static IDictionary<Type, IList<Type>> initialise()
		{
			var aggregateRoots = new Dictionary<Type, IList<Type>>
			{
				{ typeof(IStatisticTask), new List<Type> { typeof(IStatisticTask) } },
				{ typeof(IJobResultProgress), new List<Type> { typeof(IJobResultProgress) } },
				{ typeof(IMeetingChangedEntity), new List<Type> { typeof(IMeetingChangedEntity) } },
				{ typeof(MeetingChangedEntity), new List<Type> { typeof(IMeetingChangedEntity) } },
				{
					typeof(IScheduleChangedInDefaultScenario),
					new List<Type> { typeof(IScheduleChangedInDefaultScenario) }
				},
				{
					typeof(IShiftTradeScheduleChangedInDefaultScenario),
					new List<Type> { typeof(IShiftTradeScheduleChangedInDefaultScenario) }
				},
				{
					typeof(ITeamScheduleWeekViewChangedInDefaultScenario),
					new List<Type> { typeof(ITeamScheduleWeekViewChangedInDefaultScenario) }
				},
				{
					typeof(IRunRequestWaitlistEventMessage),
					new List<Type> { typeof(IRunRequestWaitlistEventMessage) }
				},
				{
					typeof(IApproveRequestsWithValidatorsEventMessage),
					new List<Type> { typeof(IApproveRequestsWithValidatorsEventMessage) }
				},
				{
					typeof(ITeleoptiDiagnosticsInformation),
					new List<Type> { typeof(ITeleoptiDiagnosticsInformation) }
				},
				{
					typeof(IPersonScheduleDayReadModel),
					new List<Type> { typeof(IPersonScheduleDayReadModel) }
				},
				{ typeof(IScheduleChangedEvent), new List<Type> { typeof(IScheduleChangedEvent) } },

				{ typeof(Scenario), new List<Type> { typeof(IScenario) } },

				{ typeof(Note), new List<Type> { typeof(IPersistableScheduleData), typeof(INote) } },
				{ typeof(AgentDayScheduleTag), new List<Type> { typeof(IPersistableScheduleData), typeof(IAgentDayScheduleTag) } },

				{ typeof(PublicNote), new List<Type> { typeof(IPersistableScheduleData), typeof(IPublicNote) } },
				{ typeof(PreferenceDay), new List<Type> { typeof(IPersistableScheduleData), typeof(IPreferenceDay) } },
				{ typeof(StudentAvailabilityDay), new List<Type> { typeof(IPersistableScheduleData), typeof(IStudentAvailabilityDay) } },

				{ typeof(Multiplicator), new List<Type> { typeof(IMultiplicator) } },
				{
					typeof(MultiplicatorDefinitionSet),
					new List<Type> { typeof(IMultiplicatorDefinitionSet) }
				},
				{ typeof(DayOffTemplate), new List<Type> { typeof(IDayOffTemplate) } },
				{ typeof(ShiftCategory), new List<Type> { typeof(IShiftCategory) } },
				{ typeof(Meeting), new List<Type> { typeof(IMeeting) } },
				{ typeof(PersonRequest), new List<Type> { typeof(IPersonRequest) } },
				{ typeof(Activity), new List<Type> { typeof(IActivity) } },
				{ typeof(Team), new List<Type> { typeof(ITeam) } },
				{ typeof(Site), new List<Type> { typeof(ISite) } },
				{ typeof(Scorecard), new List<Type> { typeof(IScorecard) } },
				{ typeof(PayrollResult), new List<Type> { typeof(IPayrollResult) } },
				{ typeof(PushMessageDialogue), new List<Type> { typeof(IPushMessageDialogue) } }
			};
			return aggregateRoots;
		}
	}
}