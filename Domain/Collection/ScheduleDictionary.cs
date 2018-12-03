using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Collection
{

	/// <summary>
	/// Holds multiple schedule range objects.
	/// A schedulerange object will automatically be created if accessed
	/// and not in the collection
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-02-12
	/// </remarks>
	public class ScheduleDictionary : IScheduleDictionary, IPermissionCheck
	{
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IScheduleDateTimePeriod _period;
		private readonly IScenario _scenario;
		private readonly IDictionary<IPerson, IScheduleRange> _dictionary;
		private const string NotSupportedOperation = "Cannot set schedule directly.";
		private IUndoRedoContainer _undoRedo;
		private bool _permissionEnabled = true;
		private readonly object _permissionLockObject = new object();
		private readonly IPersistableScheduleDataPermissionChecker _dataPermissionChecker;
		private readonly ICurrentAuthorization _authorization;
		private const string SetPermissionExMessage = "Can't reset _permissionEnabled to the same value ({0}). Threading issue?";
		public ICollection<IPersonAbsenceAccount> ModifiedPersonAccounts { get; private set; }

		protected ScheduleDictionary(IScenario scenario,
									IScheduleDateTimePeriod period,
									IDictionary<IPerson, IScheduleRange> dictionary, IPersistableScheduleDataPermissionChecker dataPermissionChecker, ICurrentAuthorization authorization)
			: this(scenario, period, dataPermissionChecker, authorization)
		{
			_dictionary = dictionary;
		}

		public ScheduleDictionary(IScenario scenario,
								IScheduleDateTimePeriod period,
								IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IPersistableScheduleDataPermissionChecker dataPermissionChecker, ICurrentAuthorization authorization)
		{
			ModifiedPersonAccounts = new HashSet<IPersonAbsenceAccount>();
			_scenario = scenario;
			_period = period;
			_dictionary = new Dictionary<IPerson, IScheduleRange>();
			_differenceCollectionService = differenceCollectionService;
			_dataPermissionChecker = dataPermissionChecker;
			_authorization = authorization;
		}

		public ScheduleDictionary(IScenario scenario, IScheduleDateTimePeriod period, IPersistableScheduleDataPermissionChecker dataPermissionChecker, ICurrentAuthorization authorization)
			: this(scenario, period, new DifferenceEntityCollectionService<IPersistableScheduleData>(), dataPermissionChecker, authorization)
		{
		}

		/// <summary>
		/// Gets the period.
		/// </summary>
		/// <value>The period.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-12
		/// </remarks>
		public IScheduleDateTimePeriod Period => _period;

		/// <summary>
		/// Gets the scenario.
		/// </summary>
		/// <value>The scenario.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-12
		/// </remarks>
		public IScenario Scenario => _scenario;


		public event EventHandler<ModifyEventArgs> PartModified;

		public void SetUndoRedoContainer(IUndoRedoContainer container)
		{
			_undoRedo = container;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-28
		/// </remarks>
		public object Clone()
		{
			var dicClone = _dictionary.ToDictionary(k => k.Key, v => (IScheduleRange)v.Value.Clone());
			return new ScheduleDictionary(Scenario, Period, dicClone, _dataPermissionChecker, _authorization);
		}

		/// <summary>
		/// Deletes from data source.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-06-17
		/// </remarks>
		public IPersistableScheduleData DeleteFromBroker(Guid id)
		{
			IPersistableScheduleData returnValue = null;

			foreach (KeyValuePair<IPerson, IScheduleRange> pair in _dictionary)
			{
				IPersistableScheduleData retVal = ((ScheduleRange)pair.Value).SolveConflictBecauseOfExternalDeletion(id, true);
				if (retVal != null)
				{
					OnPartModified(new ModifyEventArgs(ScheduleModifier.MessageBroker, retVal.Person, retVal.Period, null));

					returnValue = retVal;
				}
			}

			return returnValue;
		}

		public void DeleteMeetingFromBroker(Guid id)
		{
			IList<ModifyEventArgs> delayedEventArgs = new List<ModifyEventArgs>();
			foreach (KeyValuePair<IPerson, IScheduleRange> pair in _dictionary)
			{
				var range = ((ScheduleRange)pair.Value);
				IList<IPersonMeeting> personMeetings = range.DeleteMeetingFromDataSource(id);
				foreach (var personMeeting in personMeetings)
				{
					delayedEventArgs.Add(new ModifyEventArgs(ScheduleModifier.MessageBroker, range.Person,
															 personMeeting.Period, null));
				}
			}

			//Done this way to avoid errors when refreshing selected still having references to the meeting
			foreach (ModifyEventArgs delayedEventArg in delayedEventArgs)
			{
				OnPartModified(delayedEventArg);
			}
		}

		/// <summary>
		/// Changes made since last snapshot.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-29
		/// </remarks>
		public IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot()
		{
			var ret = new DifferenceCollection<IPersistableScheduleData>();
			foreach (var range in _dictionary.Values)
			{
				range.DifferenceSinceSnapshot(_differenceCollectionService).ForEach(ret.Add);
			}
			return ret;
		}

		public void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period)
		{
			_dictionary.Values.ForEach(scheduleRange => scheduleRange.ExtractAllScheduleData(extractor, period));
		}

		protected virtual IEnumerable<IBusinessRuleResponse> CheckIfCanModify(Dictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRules)
		{
			var failedRules = new List<IBusinessRuleResponse>();

			//do changes
			scheduleParts.GroupBy(s => s.Person).ForEach(part =>
			{
				var range = (ScheduleRange)rangeClones[part.Key];
				part.ForEach(s => range.ModifyInternal(s));
			});

			IEnumerable<IBusinessRuleResponse> responseList = newBusinessRules.CheckRules(rangeClones, scheduleParts);
			foreach (var response in responseList)
			{
				failedRules.Add(response);
			}

			return failedRules;
		}

		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection, bool forceModify = false)
		{
			return Modify(ScheduleModifier.NotApplicable, new List<IScheduleDay> { scheduleDay }, newBusinessRuleCollection, new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter(), forceModify);
		}

		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			return Modify(ScheduleModifier.NotApplicable, new List<IScheduleDay> { scheduleDay }, NewBusinessRuleCollection.Minimum(), scheduleDayChangeCallback, new NoScheduleTagSetter());
		}

		public IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier, IScheduleDay schedulePart, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTagSetter scheduleTagSetter)
		{
			return Modify(modifier, new List<IScheduleDay> { schedulePart }, newBusinessRuleCollection, scheduleDayChangeCallback, scheduleTagSetter);
		}

		public IEnumerable<IBusinessRuleResponse> CheckBusinessRules(IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			var lstErrors = new List<IBusinessRuleResponse>();
			var rangeClones = new Dictionary<IPerson, IScheduleRange>();
			var persList = new HashSet<IPerson>();
			foreach (var part in scheduleParts)
			{
				persList.Add(part.Person);
			}
			foreach (var person in persList)
			{
				rangeClones.Add(person, (IScheduleRange)this[person].Clone());
			}

			lstErrors.AddRange(CheckIfCanModify(rangeClones, scheduleParts, newBusinessRuleCollection));
			return lstErrors;
		}

		public IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTagSetter scheduleTagSetter, bool forceModify = false, bool isSystemModifying = false)
		{
			var lstErrors = new List<IBusinessRuleResponse>();

			using (PerformanceOutput.ForOperation("Modifying " + scheduleParts.Count() + " schedule(s)"))
			{
				if (!isSystemModifying && isScenarioRestrictedAndNotPermitted())
				{
					lstErrors.Add(new BusinessRuleResponse(Resources.CanNotApproveOrDenyRequestDueToNoPermissionToModifyRestrictedScenarios, true));
					return lstErrors;
				}

				if (treatScheduleAsWriteProtected(scheduleParts))
					return lstErrors;

				var rangeClones = new Dictionary<IPerson, IScheduleRange>();
				var notOverriddenErrors = new List<IBusinessRuleResponse>();

				if (notInUndoRedo())
				{
					//create clones on all persons involved
					var persList = new HashSet<IPerson>();
					foreach (var part in scheduleParts)
					{
						persList.Add(part.Person);
					}
					foreach (var person in persList)
					{
						rangeClones.Add(person, (IScheduleRange)this[person].Clone());
					}

					lstErrors.AddRange(CheckIfCanModify(rangeClones, scheduleParts, newBusinessRuleCollection));

					foreach (var businessRuleResponse in lstErrors)
					{
						if (!businessRuleResponse.Overridden || businessRuleResponse.Mandatory)
							notOverriddenErrors.Add(businessRuleResponse);
					}

					if (notOverriddenErrors.Count == 0 || forceModify)
					{
						if (_undoRedo != null)
						{
							scheduleParts.ForEach(_undoRedo.SaveState);
						}
					}
				}

				if (notOverriddenErrors.Count == 0 || forceModify)
				{
					lock (_permissionLockObject)
					{
						scheduleTagSetter.SetTagOnScheduleDays(modifier, scheduleParts);
						scheduleDayChangeCallback.ScheduleDayBeforeChanging();

						foreach (var part in scheduleParts)
						{
							var range = (ScheduleRange)this[part.Person];
							var partBefore = range.ScheduledDay(part.DateOnlyAsPeriod.DateOnly, true);
							var absences =
								partBefore.PersonAbsenceCollection().Concat(part.PersonAbsenceCollection())
									.Select(a => a.Period.ToDateOnlyPeriod(part.Person.PermissionInformation.DefaultTimeZone()));
							if (absences.Any())
							{
								var partsBefore = range.ScheduledDayCollection(absences.OrderByDescending(a => a.EndDate).First().Inflate(1));
								range.ModifyInternal(part);
								foreach (var scheduleDay in partsBefore)
								{
									scheduleDayChangeCallback.ScheduleDayChanged(scheduleDay);
								}
								var partAfter2 = range.ScheduledDay(part.DateOnlyAsPeriod.DateOnly, true);
								OnPartModified(new ModifyEventArgs(modifier, partAfter2.Person, partAfter2.Period, part));
							}
							else
							{
								range.ModifyInternal(part);
								scheduleDayChangeCallback.ScheduleDayChanged(partBefore);
								OnPartModified(new ModifyEventArgs(modifier, part.Person, part.Period, part));
							}
						}
					}

					foreach (var rangeClone in rangeClones.Values)
					{
						IScheduleRange range = this[rangeClone.Person];
						range.BusinessRuleResponseInternalCollection.Clear();
						foreach (var businessRuleResponse in rangeClone.BusinessRuleResponseInternalCollection)
						{
							range.BusinessRuleResponseInternalCollection.Add(businessRuleResponse);
						}
					}
				}
				return lstErrors;
			}
		}

		private bool notInUndoRedo()
		{
			return _undoRedo == null || !_undoRedo.InUndoRedo;
		}

		private bool treatScheduleAsWriteProtected(IEnumerable<IScheduleDay> scheduleParts)
		{
			if (notPermittedToModifyWriteProtectedSchedule())
			{
				try
				{
					checkWriteProtection(scheduleParts);
				}
				catch (PermissionException)
				{
					return true;
				}
			}
			return false;
		}

		private bool notPermittedToModifyWriteProtectedSchedule()
		{
			return !_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
		}

		private bool isScenarioRestrictedAndNotPermitted()
		{
			return _scenario.Restricted && !_authorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
		}

		private static void checkWriteProtection(IEnumerable<IScheduleDay> scheduleParts)
		{
			foreach (IScheduleDay part in scheduleParts)
			{
				DateOnly partDate = new DateOnly(part.Period.EndDateTime);
				if (part.Person.PersonWriteProtection.IsWriteProtected(partDate))
				{
					string exString = string.Concat(part.Person,
												" is write protected until ",
												part.Person.PersonWriteProtection.WriteProtectedUntil(),
												". You don't have permission to change this schedule.");
					throw new PermissionException(exString);
				}
			}
		}


		/// <summary>
		/// Gets all the schedules for the specified period.
		/// </summary>
		/// <param name="dateOnly">The date.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-02-26
		/// </remarks>
		public IEnumerable<IScheduleDay> SchedulesForDay(DateOnly dateOnly)
		{
			return _dictionary.Select(pair => pair.Value.ScheduledDay(dateOnly)).ToArray();
		}

		public IEnumerable<IScheduleDay> SchedulesForPeriod(DateOnlyPeriod period, params IPerson[] agents)
		{
			return agents.SelectMany(a => this[a].ScheduledDayCollection(period)).ToArray();
		}

		/// <summary>
		/// Takes the snapshot for later use when checking what has been changed.
		/// Is not supposed to be called explicitly in normal cases.
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-05-28
		/// </remarks>
		public void TakeSnapshot()
		{
			foreach (IScheduleRange range in _dictionary.Values)
			{
				range.TakeSnapshot();
			}
		}

		/// <summary>
		/// Updates this instance from data source/message broker.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="repository">The repository.</param>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-06-12
		/// </remarks>
		public IPersistableScheduleData UpdateFromBroker<T>(ILoadAggregateFromBroker<T> repository, Guid id) where T : IPersistableScheduleData
		{
			IPersistableScheduleData updatedData = repository.LoadAggregate(id);
			IPersistableScheduleData returnData = null;
			if (updatedData != null)
			{
				if (updatedData.Scenario != null && !_scenario.Equals(updatedData.Scenario)) return null;
				if (Keys.Contains(updatedData.Person))
				{
					ScheduleRange range = ((ScheduleRange)this[updatedData.Person]);
					if (range.WithinRange(updatedData.Period))
					{
						using (TurnoffPermissionScope.For(this))
							range.SolveConflictBecauseOfExternalUpdate(updatedData, true);
						OnPartModified(new ModifyEventArgs(ScheduleModifier.MessageBroker, updatedData.Person, updatedData.Period, null));
						returnData = updatedData;
					}
				}
			}
			return returnData;
		}

		public void MeetingUpdateFromBroker<T>(ILoadAggregateFromBroker<T> repository, Guid id) where T : IMeeting
		{
			IMeeting updatedData = repository.LoadAggregate(id);
			if (updatedData == null) return;
			if (!_scenario.Equals(updatedData.Scenario)) return;

			var personMeetings = updatedData.GetPersonMeetings(Period.LoadedPeriod(), Keys.ToArray());

			foreach (var personMeeting in personMeetings)
			{
				var range = (ScheduleRange)this[personMeeting.Person];
				if (!range.WithinRange(personMeeting.Period)) continue;
				using (TurnoffPermissionScope.For(this)) range.SolveConflictBecauseOfExternalUpdate(personMeeting, true);
				OnPartModified(
					new ModifyEventArgs(ScheduleModifier.MessageBroker, personMeeting.Person, personMeeting.Period, null));
			}
		}

		public void ValidateBusinessRulesOnPersons(IEnumerable<IPerson> people, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			using (PerformanceOutput.ForOperation("Validating " + people.Count() + " person(s)"))
			{
				lock (people)
				{
					validateAsync(people, newBusinessRuleCollection);
					//Keep for debug
					//validateSync(people, businessRules);
				}
			}
		}

		////Keep for debug
		// ReSharper disable UnusedMember.Local
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		private void validateSync(IEnumerable<IPerson> people, INewBusinessRuleCollection newBusinessRuleCollection)
		// ReSharper restore UnusedMember.Local
		{
			foreach (IPerson person in people)
			{
				var range = (IValidateScheduleRange)this[person];
				range.ValidateBusinessRules(newBusinessRuleCollection);
			}
		}

		private delegate void ValidateBusinessRulesDelegate(INewBusinessRuleCollection newBusinessRuleCollection);

		private void validateAsync(IEnumerable<IPerson> people, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			IDictionary<ValidateBusinessRulesDelegate, IAsyncResult> runnableList = new Dictionary<ValidateBusinessRulesDelegate, IAsyncResult>();

			// kick off each persons validation in its own thread
			foreach (IPerson person in people)
			{
				var range = (IValidateScheduleRange)this[person];
				ValidateBusinessRulesDelegate toRun = range.ValidateBusinessRules;
				IAsyncResult result = toRun.BeginInvoke(newBusinessRuleCollection, null, null);
				runnableList.Add(toRun, result);
			}

			//Sync all threads
			try
			{
				foreach (KeyValuePair<ValidateBusinessRulesDelegate, IAsyncResult> thread in runnableList)
				{
					thread.Key.EndInvoke(thread.Value);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.Message);
				throw;
			}

		}

		/// <summary>
		/// Raises modified event
		/// </summary>
		/// <param name="e"></param>
		// ReSharper disable InconsistentNaming
		private void OnPartModified(ModifyEventArgs e)
		// ReSharper restore InconsistentNaming
		{
			PartModified?.Invoke(this, e);
		}

		#region IDictionary<T,T>

		public bool ContainsKey(IPerson key)
		{
			return _dictionary.ContainsKey(key);
		}

		public IScheduleRange this[IPerson key]
		{
			get => getOrCreateScheduleRange(key);
			set => throw new NotSupportedException(NotSupportedOperation);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		private IScheduleRange getOrCreateScheduleRange(IPerson agent)
		{
			if (_dictionary.TryGetValue(agent, out var scheduleRange)) 
				return scheduleRange;
			
			var newScheduleRange = new ScheduleRange(this, 
				new ScheduleParameters(Scenario, agent, Period.RangeToLoadCalculator.SchedulerRangeToLoad(agent)), _dataPermissionChecker, _authorization);
			_dictionary.Add(agent, newScheduleRange);
			return newScheduleRange;
		}
		
		public bool Remove(IPerson key)
		{
			return _dictionary.Remove(key);
		}

		public bool Remove(KeyValuePair<IPerson, IScheduleRange> item)
		{
			return _dictionary.Remove(item);
		}

		public bool TryGetValue(IPerson key, out IScheduleRange value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		public ICollection<IPerson> Keys => _dictionary.Keys;

		public ICollection<IScheduleRange> Values => _dictionary.Values;

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool Contains(KeyValuePair<IPerson, IScheduleRange> item)
		{
			return _dictionary.Contains(item);
		}

		public void CopyTo(KeyValuePair<IPerson, IScheduleRange>[] array, int arrayIndex)
		{
			_dictionary.CopyTo(array, arrayIndex);
		}

		public int Count => _dictionary.Count;

		public bool IsReadOnly => false;

		IEnumerator<KeyValuePair<IPerson, IScheduleRange>> IEnumerable<KeyValuePair<IPerson, IScheduleRange>>.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		public void Add(IPerson key, IScheduleRange value)
		{
			throw new NotSupportedException(NotSupportedOperation);
		}

		public void Add(KeyValuePair<IPerson, IScheduleRange> item)
		{
			throw new NotSupportedException(NotSupportedOperation);
		}

		#endregion

		public bool PermissionsEnabled => _permissionEnabled;

		public void UsePermissions(bool enabled)
		{
			if (enabled == _permissionEnabled)
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, SetPermissionExMessage, enabled));
			_permissionEnabled = enabled;
		}

		public object SynchronizationObject => _permissionLockObject;

		protected IDictionary<IPerson, IScheduleRange> BaseDictionary => _dictionary;

		public DateTime ScheduleLoadedTime { get; set; }
	}

	public interface IReadOnlyScheduleDictionary : IScheduleDictionary
	{
		void MakeEditable();
	}

	public class ReadOnlyScheduleDictionary : ScheduleDictionary, IReadOnlyScheduleDictionary
	{
		private bool _editable;

		public void MakeEditable()
		{
			_editable = true;
		}
		public ReadOnlyScheduleDictionary(IScenario scenario, IScheduleDateTimePeriod scheduleDateTimePeriod, IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService, IPersistableScheduleDataPermissionChecker dataPermissionChecker, ICurrentAuthorization authorization)
			: base(scenario, scheduleDateTimePeriod, differenceCollectionService, dataPermissionChecker, authorization)
		{
		}

		protected override IEnumerable<IBusinessRuleResponse> CheckIfCanModify(Dictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRules)
		{
			if (!_editable)
				throw new NotSupportedException("A read only schedule dictionary cannot be modified.");

			return base.CheckIfCanModify(rangeClones, scheduleParts, newBusinessRules);
		}
	}
}
