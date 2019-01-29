using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ExtractedSchedule : Schedule, IScheduleDay
	{
		private readonly ICurrentAuthorization _currentAuthorization;

		public ISignificantPartService ServiceForSignificantPart { get; set; }
		public ISignificantPartService ServiceForSignificantPartForDisplay { get; set; }

		private ExtractedSchedule(IScheduleDictionary owner, IScheduleParameters parameters, ICurrentAuthorization currentAuthorization)
			: base(owner, parameters, currentAuthorization)
		{
			_currentAuthorization = currentAuthorization;
			TimeZone = Person.PermissionInformation.DefaultTimeZone();
		}

		public static IScheduleDay CreateScheduleDay(IScheduleDictionary owner, IPerson person, DateOnly dateOnly, ICurrentAuthorization currentAuthorization)
		{
			var dateAndPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			return CreateScheduleDay(owner, person, dateAndPeriod, currentAuthorization);
		}

		public static IScheduleDay CreateScheduleDay(IScheduleDictionary owner, IPerson person, IDateOnlyAsDateTimePeriod dateAndPeriod, ICurrentAuthorization currentAuthorization)
		{
			var param = new ScheduleParameters(owner.Scenario, person, dateAndPeriod.Period());
			var ret = new ExtractedSchedule(owner, param, currentAuthorization) { DateOnlyAsPeriod = dateAndPeriod };
			return ret;
		}

		public TimeZoneInfo TimeZone { get; }

		public IDateOnlyAsDateTimePeriod DateOnlyAsPeriod { get; private set; }

		public bool IsFullyPublished { get; set; }

		public IEnumerable<IPersistableScheduleData> PersistableScheduleDataCollection()
		{
			return PersistableScheduleDataInternalCollection();
		}

		public bool FullAccess { get; set; }

		public IList<IBusinessRuleResponse> BusinessRuleResponseCollection => BusinessRuleResponseInternalCollection;

		public virtual SchedulePartView SignificantPart()
		{
			if (ServiceForSignificantPart == null)
				ServiceForSignificantPart = SignificantPartService.CreateService(this);
			return ServiceForSignificantPart.SignificantPart();
		}

		public virtual SchedulePartView SignificantPartForDisplay()
		{
			if (ServiceForSignificantPartForDisplay == null)
				ServiceForSignificantPartForDisplay = SignificantPartService.CreateServiceForDisplay(this);
			return ServiceForSignificantPartForDisplay.SignificantPart();
		}

		public IEditableShift GetEditorShift()
		{
			var personAssignment = PersonAssignment();
			if (personAssignment == null)
				return null;

			return new EditableShiftMapper().CreateEditorShift(personAssignment);
		}

		public bool IsScheduled()
		{
			return new IsDayScheduled().Check(this);
		}

		public IPersonAssignment PersonAssignment(bool createIfNotExists = false)
		{
			var currentAss = ScheduleDataInternalCollection().OfType<IPersonAssignment>().SingleOrDefault();
			if (createIfNotExists)
			{
				if (currentAss == null)
				{
					currentAss = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
					Add(currentAss);
				}
			}
			return currentAss;
		}

		public IScheduleDay ReFetch()
		{
			return Owner[Person].ReFetch(this);
		}

		public IPersonAbsence[] PersonAbsenceCollection()
		{
			return PersonAbsenceCollection(false);
		}

		public IPersonAbsence[] PersonAbsenceCollection(bool includeOutsideActualDay)
		{
			var sorter = new PersonAbsenceSorter();
			var org = ScheduleDataInternalCollection().OfType<IPersonAbsence>().ToArray();
			Array.Sort(org, sorter);
			if (includeOutsideActualDay || org.IsEmpty())
				return org;
			return excludeDataOutsideDayOrShift(org, sorter);
		}

		private T[] excludeDataOutsideDayOrShift<T>(IEnumerable<T> org, IComparer<T> sorter) where T : IScheduleData
		{
			var period = DateOnlyAsPeriod.Period();
			var projPeriod = ProjectionService().CreateProjection().Period();

			var ret = org.Where(o =>
					(period.Intersect(o.Period) ||
					 projPeriod.HasValue && o.Period.Intersect(projPeriod.Value)))
				.ToArray();
			Array.Sort(ret, sorter);
			return ret;
		}

		public IPersonMeeting[] PersonMeetingCollection()
		{
			return PersonMeetingCollection(false);
		}

		public IOvertimeAvailability[] OvertimeAvailablityCollection()
		{
			return ScheduleDataInternalCollection().OfType<IOvertimeAvailability>().ToArray();
		}

		public IPersonMeeting[] PersonMeetingCollection(bool includeOutsideActualDay)
		{
			var sorter = new PersonMeetingByDateSorter();
			var org = ScheduleDataInternalCollection().OfType<IPersonMeeting>().ToArray();
			Array.Sort(org, sorter);
			if (includeOutsideActualDay || org.IsEmpty())
				return org;
			return excludeDataOutsideDayOrShift(org, sorter);
		}

		public IScheduleData[] PersonRestrictionCollection()
		{
			// temporärt så länge båda finns
			var scheduleDataInternalCollection = ScheduleDataInternalCollection().ToArray();
			var dataRestrictions = scheduleDataInternalCollection.OfType<IScheduleDataRestriction>();

			var persistRestrictions = scheduleDataInternalCollection.OfType<PreferenceDay>();
			var studentRestrictions = scheduleDataInternalCollection.OfType<StudentAvailabilityDay>();
			return dataRestrictions.Cast<IScheduleData>().Concat(persistRestrictions).Concat(studentRestrictions).ToArray();
		}

		public IEnumerable<IRestrictionBase> RestrictionCollection()
		{
			var dataRestrictions = ScheduleDataInternalCollection().OfType<IRestrictionOwner>();
			var retList = new List<IRestrictionBase>();
			dataRestrictions.ForEach(dataRestriction => retList.AddRange(dataRestriction.RestrictionBaseCollection));
			return retList;
		}

		public INote[] NoteCollection()
		{
			var retList = ScheduleDataInternalCollection().OfType<INote>().ToArray();
			Array.Sort(retList, new NoteByDateSorter());
			return retList;
		}

		public IPublicNote[] PublicNoteCollection()
		{
			var retList = ScheduleDataInternalCollection().OfType<IPublicNote>().ToArray();
			Array.Sort(retList, new PublicNoteByDateSorter());
			return retList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")] //rk fixar snart
		public void Clear<T>() where T : IScheduleData
		{
			IList<T> filterList = new List<T>(ScheduleDataInternalCollection().OfType<T>());

			foreach (var data in filterList)
			{
				Remove(data);
			}
		}

		protected override bool CheckPermission(IScheduleData persistableScheduleData)
		{
			return true;
		}

		public IProjectionService ProjectionService()
		{
			return new ScheduleProjectionService(this, new ProjectionPayloadMerger());
		}

		public bool HasProjection()
		{
			var internalCollection = ScheduleDataInternalCollection();
			return (internalCollection.OfType<IPersonAssignment>().Any(p => p.ShiftLayers.Any() || p.DayOff() != null) ||
					internalCollection.OfType<IPersonAbsence>().Any());
		}

		public void Merge(IScheduleDay source, bool isDelete)
		{
			Merge(source, isDelete, false);
		}

		public void Merge(IScheduleDay source, bool isDelete, bool ignoreTimeZoneChanges,
			bool ignoreAssignmentPermission = false)
		{
			var view = source.SignificantPartForDisplay();

			switch (view)
			{
				case SchedulePartView.DayOff:
					if (isDelete) DeleteDayOff(); else mergeDayOff(source, true, ignoreAssignmentPermission);
					break;

				case SchedulePartView.ContractDayOff:
					if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source);
					break;

				case SchedulePartView.FullDayAbsence:
					if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source);
					break;

				case SchedulePartView.Absence:
					if (isDelete) DeleteAbsence(false); else mergeAbsence(source);
					break;

				case SchedulePartView.MainShift:
					if (isDelete) DeleteMainShift(); else mergeMainShift(source, ignoreTimeZoneChanges, true);
					break;

				case SchedulePartView.PersonalShift:
					if (isDelete) DeletePersonalStuff(); else mergePersonalStuff(source, ignoreTimeZoneChanges);
					break;

				case SchedulePartView.PreferenceRestriction:
					if (isDelete) DeletePreferenceRestriction(); else mergePreferenceRestriction(source);
					break;

				case SchedulePartView.StudentAvailabilityRestriction:
					if (isDelete) DeleteStudentAvailabilityRestriction(); else mergeStudentAvailabilityRestriction(source);
					break;
			}
		}

		public void DeletePreferenceRestriction()
		{
			Clear<IPreferenceDay>();
		}

		public void DeleteNote()
		{
			Clear<INote>();
		}

		public void DeletePublicNote()
		{
			Clear<IPublicNote>();
		}

		public bool HasDayOff()
		{
			var ass = PersonAssignment();
			return ass?.DayOff() != null;
		}

		private void mergePreferenceRestriction(IScheduleDay source)
		{
			var diff = CalculatePeriodOffset(source.Period);
			var period = source.Period.MovePeriod(diff);
			var date = new DateOnly(period.StartDateTimeLocal(TimeZone));

			foreach (IPreferenceRestriction preferenceRestriction in source.RestrictionCollection().OfType<IPreferenceRestriction>())
			{
				Clear<IPreferenceDay>();
				Add(new PreferenceDay(Person, date, preferenceRestriction.NoneEntityClone()));
			}
		}

		public void DeleteStudentAvailabilityRestriction()
		{
			Clear<IStudentAvailabilityDay>();
		}

		public void DeleteOvertimeAvailability()
		{
			Clear<IOvertimeAvailability>();
		}

		private void mergeStudentAvailabilityRestriction(IScheduleDay source)
		{
			var diff = CalculatePeriodOffset(source.Period);
			var period = source.Period.MovePeriod(diff);
			var date = new DateOnly(period.StartDateTimeLocal(TimeZone));

			foreach (IStudentAvailabilityRestriction studentAvailabilityRestriction in source.RestrictionCollection().OfType<IStudentAvailabilityRestriction>())
			{
				Clear<IStudentAvailabilityDay>();
				Add(new StudentAvailabilityDay(Person, date, new List<IStudentAvailabilityRestriction> { studentAvailabilityRestriction.NoneEntityClone() }));
			}
		}

		//borde tas bort!
		public void DeleteDayOff(TrackedCommandInfo trackedCommandInfo = null)
		{
			var ass = PersonAssignment();
			ass?.SetDayOff(null, false, trackedCommandInfo);
		}

		private void mergeDayOff(IScheduleDay source, bool deleteAbsence, bool ignoreAssignmentPermission)
		{
			if (!_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment) && !ignoreAssignmentPermission)
				return;

			if (!PersonAbsenceCollection().IsEmpty() && !_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				return;

			var diff = CalculatePeriodOffset(source.Period);
			IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
			var period = source.Period.MovePeriod(diff);

			if (deleteAbsence)
			{
				//loop absences
				foreach (IPersonAbsence personAbsence in PersonAbsenceCollection())
				{
					if (crossNightAbsence(source, personAbsence))
					{
						splitList.Add(personAbsence);
					}
					else
					{
						personAbsence.Split(period).ForEach(splitList.Add);
					}
				}
				IList<IPersonAbsence> filterList = new List<IPersonAbsence>(ScheduleDataInternalCollection().OfType<IPersonAbsence>());
				foreach (IPersonAbsence data in filterList)
				{
					if (data.Period.Intersect(Period))
						Remove(data);
				}

				splitList.ForEach(Add);
			}

			var thisAss = PersonAssignment(true);
			thisAss.ClearMainActivities();
			source.PersonAssignment().SetThisAssignmentsDayOffOn(thisAss);
		}

		public void DeleteFullDayAbsence(IScheduleDay source)
		{
			

			foreach (var personAbsence in PersonAbsenceCollection())
			{
				IPersonAbsence personAbsenceUpForDelete = null;
				if (SignificantPart() == SchedulePartView.FullDayAbsence || SignificantPartForDisplay() == SchedulePartView.ContractDayOff && Period.Intersect(personAbsence.Layer.Period) || Period.Contains(personAbsence.Period.StartDateTime))
				{
					personAbsenceUpForDelete = personAbsence;
				}

				if (personAbsenceUpForDelete == null) continue;

				var splitList = new List<IPersonAbsence>();
				var period = AbsenceSplitPeriod(source);
				var start = period.StartDateTime;
				var end = period.EndDateTime;
				if (personAbsenceUpForDelete.Period.EndDateTime <= start) continue;
				if (Period.Contains(personAbsence.Period.StartDateTime)) start = source.Period.StartDateTime;
				personAbsenceUpForDelete.Split(new DateTimePeriod(start, end)).ForEach(splitList.Add);
				Remove(personAbsenceUpForDelete);
				splitList.ForEach(Add);
			}
		}

		public DateTimePeriod AbsenceSplitPeriod(IScheduleDay scheduleDay)
		{
			var dayBefore = Owner[scheduleDay.Person].ScheduledDay(scheduleDay.DateOnlyAsPeriod.DateOnly.AddDays(-1));
			var endBefore = dayBefore.PersonAssignment(true).Period.EndDateTime;
			var assignmentEnd = scheduleDay.PersonAssignment(true).Period.EndDateTime;
			var end = assignmentEnd < scheduleDay.Period.EndDateTime ? scheduleDay.Period.EndDateTime : assignmentEnd;
			var start = scheduleDay.Period.StartDateTime > endBefore ? scheduleDay.Period.StartDateTime : endBefore;
			return new DateTimePeriod(start, end);
		}

		public void AdjustFullDayAbsenceNextDay(IScheduleDay destination)
		{
			var assignment = destination?.PersonAssignment();
			if (assignment == null) return;

			var dayPeriod = destination.DateOnlyAsPeriod.Period();
			var nextDay = assignment.Period.EndDateTime > dayPeriod.EndDateTime && assignment.Period.StartDateTime < dayPeriod.EndDateTime;

			if (!nextDay) return;

			IList<IPersonAbsence> allAbsences = new List<IPersonAbsence>(destination.PersonAbsenceCollection());
			foreach (IPersonAbsence personAbsence in destination.PersonAbsenceCollection())
			{
				var period = destination.AbsenceSplitPeriod(destination);
				if (personAbsence.Period.EndDateTime <= period.StartDateTime) continue;

				if (personAbsence.Period.StartDateTime < destination.Period.EndDateTime)
					destination.Remove(personAbsence);
			}

			foreach (var personAbsence in allAbsences)
			{
				var period = destination.AbsenceSplitPeriod(destination);
				if (personAbsence.Period.EndDateTime <= period.StartDateTime) continue;

				if (personAbsence.Period.StartDateTime >= destination.Period.EndDateTime) continue;
				var oldLayer = personAbsence.Layer;
				var oldPeriod = oldLayer.Period;

				var diffEnd = oldPeriod.EndDateTime.Subtract(assignment.Period.EndDateTime);
				var diffStart = oldPeriod.StartDateTime.Subtract(assignment.Period.StartDateTime);
				var newPeriod = oldPeriod.ChangeEndTime(-diffEnd);
				newPeriod = newPeriod.ChangeStartTime(-diffStart);

				IAbsenceLayer newLayer = new AbsenceLayer(oldLayer.Payload, newPeriod);
				IPersonAbsence newPersonAbsence = new PersonAbsence(personAbsence.Person, destination.Scenario, newLayer);
				destination.Add(newPersonAbsence);
			}
		}

		private void mergeAbsence(IScheduleDay source)
		{
			MergeAbsences(source, false);
		}

		private void mergeFullDayAbsence(IScheduleDay source)
		{
			MergeAbsences(source, false);
		}

		public void MergeAbsences(IScheduleDay source, bool all)
		{
			var addList = new List<IPersonAbsence>();
			var diff = CalculatePeriodOffset(source.Period);
			var layerCollection = source.ProjectionService().CreateProjection();
			var layerCollectionPeriod = layerCollection.Period();

			foreach (var sourceAbsence in source.PersonAbsenceCollection())
			{
				if (layerCollectionPeriod.HasValue)
				{
					if (sourceAbsence.Layer.Period.Contains(layerCollectionPeriod.Value))
					{
						if (!all)
							addList.Clear();

						addList.Add(sourceAbsence.NoneEntityClone());
					}
				}
				else
				{
					if (!all)
						addList.Clear();

					addList.Add(sourceAbsence.NoneEntityClone());

				}
			}

			foreach (var personAbsence in addList)
			{
				var oldLayer = personAbsence.Layer;
				var newLayer = new AbsenceLayer(oldLayer.Payload, oldLayer.Period.MovePeriod(diff));
				var newAbsence = new PersonAbsence(Person, Scenario, newLayer)
				{
					LastChange = DateTime.UtcNow
				};
				Add(newAbsence);
			}
		}

		public void DeleteAbsence(bool all)
		{
			var layerCollection = ProjectionService().CreateProjection();
			IList<IPersonAbsence> removeList = new List<IPersonAbsence>();

			var layerCollectionPeriod = layerCollection.Period();
			foreach (IPersonAbsence personAbsence in PersonAbsenceCollection(true).Reverse())
			{
				if (layerCollectionPeriod.HasValue)
				{
					if (!personAbsence.Layer.Period.Contains(layerCollectionPeriod.Value))
					{
						if (!all)
							removeList.Clear();

						removeList.Add(personAbsence);
					}
				}
				else
				{
					if (!all)
					{
						removeList.Clear();
					}

					if (Period.Contains(personAbsence.Layer.Period.StartDateTime))
						removeList.Add(personAbsence);

				}
			}

			foreach (IPersonAbsence personAbsence in removeList)
			{
				Remove(personAbsence);
			}
		}

		public void DeleteOvertime()
		{
			var highAss = PersonAssignment();
			highAss?.ClearOvertimeActivities();
		}

		public void DeleteMainShift()
		{
			var highAss = PersonAssignment();
			highAss?.ClearMainActivities();
			highAss?.ClearOvertimeActivities();
		}

		public void DeleteMainShiftSpecial()
		{
			var highAss = PersonAssignment();
			highAss?.ClearMainActivities();
		}

		public TimeSpan CalculatePeriodOffset(DateTimePeriod sourcePeriod)
		{
			var periodOffsetCalculator = new PeriodOffsetCalculator();
			return periodOffsetCalculator.CalculatePeriodOffset(sourcePeriod, Period);

		}

		private void mergeMainShift(IScheduleDay source, bool ignoreTimeZoneChanges, bool splitAbsence)
		{
			var sourceAssignment = source.PersonAssignment();
			if (sourceAssignment == null)
				return;

			var periodOffsetCalculator = new PeriodOffsetCalculator();
			var periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source, this, ignoreTimeZoneChanges, sourceAssignment.Period);

			var workingCopyOfAssignment = sourceAssignment.NoneEntityClone();
			workingCopyOfAssignment.SetActivitiesAndShiftCategoryFromWithOffset(sourceAssignment, periodOffset);

			if (PersonAssignment() == null && SignificantPart() == SchedulePartView.DayOff)
			{
				DeleteDayOff();
			}

			var currentAssignment = PersonAssignment(true);
			currentAssignment.SetActivitiesAndShiftCategoryFrom(workingCopyOfAssignment);
			if (splitAbsence)
			{
				var splitPeriod = AbsenceSplitPeriod(this);
				SplitAbsences(splitPeriod);
			}
		}

		public void SplitAbsences(DateTimePeriod period)
		{
			IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
			IList<IPersonAbsence> deleteList = new List<IPersonAbsence>();
			var layerCollection = ProjectionService().CreateProjection();
			var projectionPeriod = layerCollection.Period();
			if (!projectionPeriod.HasValue) return;
			var dateTimePeriod = projectionPeriod.Value;

			//loop absences in source
			foreach (IPersonAbsence personAbsence in PersonAbsenceCollection())
			{
				if (personAbsence.Layer.Period.Contains(dateTimePeriod))
				{
					//try to split them
					personAbsence.Split(period).ForEach(splitList.Add);

					//remove original
					if (!deleteList.Contains(personAbsence))
						deleteList.Add(personAbsence);
				}
			}

			if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence) && deleteList.Count > 0)
				return;

			//remove absences up for split
			deleteList.ForEach(Remove);

			//add new splitted absences
			foreach (IPersonAbsence newAbsence in splitList)
			{
				Add(newAbsence);
			}
		}

		public void DeletePersonalStuff()
		{
			var ass = PersonAssignment();
			ass?.ClearPersonalActivities();
		}

		private void mergePersonalStuff(IScheduleDay source, bool ignoreTimeZoneChanges)
		{
			var sourceAss = source.PersonAssignment();
			if (sourceAss == null) return;

			var destAss = PersonAssignment(true);
			var periodOffsetCalculator = new PeriodOffsetCalculator();

			foreach (var personalLayer in sourceAss.PersonalActivities())
			{
				var periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source, this, ignoreTimeZoneChanges, sourceAss.Period);
				destAss.AddPersonalActivity(personalLayer.Payload, personalLayer.Period.MovePeriod(periodOffset));
			}
		}

		public IScheduleTag ScheduleTag()
		{
			IList<IAgentDayScheduleTag> retList = new List<IAgentDayScheduleTag>(ScheduleDataInternalCollection().OfType<IAgentDayScheduleTag>());
			if (retList.Count == 0)
				return NullScheduleTag.Instance;

			return retList[0].ScheduleTag;
		}

		public void CreateAndAddDayOff(IDayOffTemplate dayOff)
		{
			var foundPersonAssignment = PersonAssignment(true);
			foundPersonAssignment.SetDayOff(dayOff);
		}

		public void CreateAndAddNote(string text)
		{
			var dateOnly = DateOnlyAsPeriod.DateOnly;
			var note = new Note(Person, dateOnly, Scenario, text);
			DeleteNote();
			Add(note);
		}

		public void CreateAndAddPublicNote(string text)
		{
			var dateOnly = DateOnlyAsPeriod.DateOnly;
			var publicNote = new PublicNote(Person, dateOnly, Scenario, text);
			DeletePublicNote();
			Add(publicNote);
		}

		public IPersonAbsence CreateAndAddAbsence(IAbsenceLayer layer)
		{
			var personAbsence = new PersonAbsence(Person, Scenario, layer) { LastChange = DateTime.UtcNow };
			Add(personAbsence);
			return personAbsence;
		}

		public void CreateAndAddOvertime(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet, bool muteEvent = false, TrackedCommandInfo trackedCommandInfo = null)
		{
			var foundPersonAssignment = PersonAssignment(true);
			foundPersonAssignment.AddOvertimeActivity(activity, period, definitionSet, muteEvent, trackedCommandInfo);
		}

		public void MergeOvertime(IScheduleDay source)
		{
			var dateOnlyPerson = DateOnlyAsPeriod.DateOnly;
			var period = Person.Period(dateOnlyPerson);
			if (period == null) return;
			var personAss = source.PersonAssignment();
			if (personAss == null) return;

			var periodOffsetCalculator = new PeriodOffsetCalculator();
			var diff = periodOffsetCalculator.CalculatePeriodOffsetConsiderDaylightSavings(source, this, personAss.Period);

			foreach (var layer in personAss.OvertimeActivities())
			{
				if (period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
				{
					CreateAndAddOvertime(layer.Payload, layer.Period.MovePeriod(diff), layer.DefinitionSet);
				}
			}
		}

		public IScheduleDay CreateAndAddActivity(IActivity activity, DateTimePeriod period)
		{
			CreateAndAddActivity(activity, period, null);
			return this;
		}

		public void CreateAndAddActivity(IActivity activity, DateTimePeriod period, IShiftCategory shiftCategory)
		{
			if (!_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
				return;

			var ass = PersonAssignment();
			if (ass != null)
			{
				if (!ass.ShiftLayers.Any() || DateOnlyAsPeriod.Period().Contains(period.StartDateTime) || ass.Period.Intersect(period) || ass.Period.AdjacentTo(period))
				{
					ass.AddActivity(activity, period);
					if (ass.ShiftCategory == null)
					{
						ass.SetShiftCategory(shiftCategory);
					}
					return;
				}
			}

			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
			newPersonAssignment.AddActivity(activity, period);
			newPersonAssignment.SetShiftCategory(shiftCategory);
			Add(newPersonAssignment);

			SplitAbsences(Period);
		}

		public void CreateAndAddPersonalActivity(IActivity activity, DateTimePeriod period, bool muteEvent = true, TrackedCommandInfo trackedCommandInfo = null)
		{
			var ass = PersonAssignment(true);
			ass.AddPersonalActivity(activity, period, muteEvent, trackedCommandInfo);
		}

		//will be removed
		public void AddMainShift(IEditableShift mainShift)
		{
			var currentAss = PersonAssignment(true);

			new EditableShiftMapper().SetMainShiftLayers(currentAss, mainShift);
		}

		public void AddMainShift(IPersonAssignment mainShiftSource)
		{
			var currentAss = PersonAssignment(true);
			currentAss.SetActivitiesAndShiftCategoryFrom(mainShiftSource);
		}

		public void Restore(IScheduleDay previousState)
		{
			var callback = UndoRedoState.ScheduleDayChangeCallback ?? new DoNothingScheduleDayChangeCallBack();
			Owner.Modify(ScheduleModifier.UndoRedo, previousState, null, callback, new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		public IMemento CreateMemento()
		{
			return new Memento<IScheduleDay>(this, Owner[Person].ReFetch(this));
		}

		public override string ToString()
		{
			//until we have implemented a cellmodel for the schedulepart this is needed since the grid calls this
			return "";
		}

		protected override void CloneDerived(Schedule clone)
		{
			var thisClone = (ExtractedSchedule)clone;
			thisClone.ServiceForSignificantPart = null;
			thisClone.ServiceForSignificantPartForDisplay = null;
		}

		public void Remove(DeleteOption options)
		{
			if (options.Absence)
			{
				DeleteAbsence(true);
				DeleteFullDayAbsence(this);
			}

			if (options.DayOff)
				DeleteDayOff();

			if (options.MainShift)
				DeleteMainShift();

			if (options.Overtime)
				DeleteOvertime();

			if (options.PersonalShift)
				DeletePersonalStuff();

			if (options.Preference)
				DeletePreferenceRestriction();

			if (options.StudentAvailability)
				DeleteStudentAvailabilityRestriction();

			if (options.OvertimeAvailability)
				DeleteOvertimeAvailability();
		}

		public IPreferenceDay PreferenceDay()
		{
			return PersistableScheduleDataCollection().OfType<IPreferenceDay>().SingleOrDefault();
		}

		private static bool crossNightAbsence(IScheduleDay source, IPersonAbsence personAbsence)
		{
			var timeZone = personAbsence.Person.PermissionInformation.DefaultTimeZone();

			return personAbsence.Period.ToDateOnlyPeriod(timeZone).StartDate != source.Period.ToDateOnlyPeriod(timeZone).StartDate;
		}
	}

	public static class ScheduleDayExtension
	{
		public static bool IsWorkday(this IScheduleDay scheduleDay)
		{
			var schedulePartView = scheduleDay.SignificantPart();
			if (schedulePartView == SchedulePartView.Overtime)
				return true;
			if (schedulePartView == SchedulePartView.MainShift)
				return true;
			return false;
		}

		public static bool IsFullDayAbsence(this IScheduleDay scheduleDay)
		{
			if (scheduleDay == null)
			{
				return false;
			}

			var projection = scheduleDay.ProjectionService().CreateProjection();
			var significantPart = scheduleDay.SignificantPart();
			if (significantPart == SchedulePartView.ContractDayOff || significantPart == SchedulePartView.DayOff)
			{
				return projection.HasLayers && projection.All(l => l.Payload is IAbsence);
			}
			return significantPart == SchedulePartView.FullDayAbsence;
		}

		public static bool HasAbsenceProjection(this IScheduleDay scheduleDay)
		{
			return scheduleDay != null &&
				   scheduleDay.ProjectionService().CreateProjection().Any(x => { return x.Payload is IAbsence; });
		}
	}
}
