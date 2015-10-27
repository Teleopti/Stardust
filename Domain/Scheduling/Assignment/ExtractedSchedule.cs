using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;
using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;


namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ExtractedSchedule : Schedule, IScheduleDay
	{
		public ISignificantPartService ServiceForSignificantPart  { get; set; }
		public ISignificantPartService ServiceForSignificantPartForDisplay { get; set; }

		private ExtractedSchedule(IScheduleDictionary owner, IScheduleParameters parameters)
			: base(owner, parameters)
		{
			TimeZone = Person.PermissionInformation.DefaultTimeZone();
		}
		
		public static IScheduleDay CreateScheduleDay(IScheduleDictionary owner, IPerson person, DateOnly dateOnly)
		{
			var dateAndPeriod = new DateOnlyAsDateTimePeriod(dateOnly, person.PermissionInformation.DefaultTimeZone());
			return CreateScheduleDay(owner, person, dateAndPeriod);
		}

		public static IScheduleDay CreateScheduleDay(IScheduleDictionary owner, IPerson person, IDateOnlyAsDateTimePeriod dateAndPeriod)
		{
			var param = new ScheduleParameters(owner.Scenario, person, dateAndPeriod.Period());
			var ret = new ExtractedSchedule(owner, param) { DateOnlyAsPeriod = dateAndPeriod };
			return ret;
		}

		public TimeZoneInfo TimeZone { get; private set; }

		public IDateOnlyAsDateTimePeriod DateOnlyAsPeriod { get; private set; }

		public bool IsFullyPublished { get; set; }

		public IEnumerable<IPersistableScheduleData> PersistableScheduleDataCollection()
		{
			return PersistableScheduleDataInternalCollection();
		}

		public bool FullAccess { get; set; }

		public IList<IBusinessRuleResponse> BusinessRuleResponseCollection
		{
			get { return BusinessRuleResponseInternalCollection; }
		}

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
			SchedulePartView partView = SignificantPart();
			return (partView == SchedulePartView.FullDayAbsence || partView == SchedulePartView.DayOff ||
					partView == SchedulePartView.ContractDayOff || partView == SchedulePartView.MainShift);
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

		public ReadOnlyCollection<IPersonAbsence> PersonAbsenceCollection()
		{
			return PersonAbsenceCollection(false);
		}

		public ReadOnlyCollection<IPersonAbsence> PersonAbsenceCollection(bool includeOutsideActualDay)
		{
			var sorter = new PersonAbsenceSorter();
			var org = new List<IPersonAbsence>(ScheduleDataInternalCollection().OfType<IPersonAbsence>());
			org.Sort(sorter);
			if (includeOutsideActualDay || org.IsEmpty())
				return new ReadOnlyCollection<IPersonAbsence>(org);
			return excludeDataOutsideDayOrShift(org, sorter);
		}

		private ReadOnlyCollection<T> excludeDataOutsideDayOrShift<T>(IEnumerable<T> org, IComparer<T> sorter) where T : IScheduleData
		{
			var ret = new List<T>();
			var period = DateOnlyAsPeriod.Period();
			var projPeriod = ProjectionService().CreateProjection().Period();

			foreach (var personAbsence in org)
			{
				if (period.Intersect(personAbsence.Period) || (projPeriod.HasValue && personAbsence.Period.Intersect(projPeriod.Value)))
					ret.Add(personAbsence);
			}
			ret.Sort(sorter);
			return new ReadOnlyCollection<T>(ret);
		}

		public ReadOnlyCollection<IPersonMeeting> PersonMeetingCollection()
		{
			return PersonMeetingCollection(false);
		}

		public ReadOnlyCollection<IOvertimeAvailability> OvertimeAvailablityCollection()
		{
			var scheduleDataInternalCollection = ScheduleDataInternalCollection().ToList();

			var overtimeRestrictions = scheduleDataInternalCollection.OfType<IOvertimeAvailability>();
			var ret = new List<IOvertimeAvailability>();

			foreach (var overtimeRestriction in overtimeRestrictions)
			{
				ret.Add(overtimeRestriction);
			}
			return ret.AsReadOnly();
		}

		public ReadOnlyCollection<IPersonMeeting> PersonMeetingCollection(bool includeOutsideActualDay)
		{
			var sorter = new PersonMeetingByDateSorter();
			var org = new List<IPersonMeeting>(ScheduleDataInternalCollection().OfType<IPersonMeeting>());
			org.Sort(sorter);
			if (includeOutsideActualDay || org.IsEmpty())
				return new ReadOnlyCollection<IPersonMeeting>(org);
			return excludeDataOutsideDayOrShift(org, sorter);
		}

		public ReadOnlyCollection<IScheduleData> PersonRestrictionCollection()
		{
			// temporärt så länge båda finns
			var scheduleDataInternalCollection = ScheduleDataInternalCollection().ToList();
			IEnumerable<IScheduleDataRestriction> dataRestrictions = scheduleDataInternalCollection.OfType<IScheduleDataRestriction>();

			IEnumerable<PreferenceDay> persistRestrictions = scheduleDataInternalCollection.OfType<PreferenceDay>();
			IEnumerable<StudentAvailabilityDay> studentRestrictions = scheduleDataInternalCollection.OfType<StudentAvailabilityDay>();
			var ret = new List<IScheduleData>();

			foreach (var dataRestriction in dataRestrictions)
			{
				ret.Add(dataRestriction);
			}
			foreach (var dataRestriction in persistRestrictions)
			{
				ret.Add(dataRestriction);
			}
			foreach (var studRestriction in studentRestrictions)
			{
				ret.Add(studRestriction);
			}
			return ret.AsReadOnly();
		}

		public IEnumerable<IRestrictionBase> RestrictionCollection()
		{
			var dataRestrictions = ScheduleDataInternalCollection().OfType<IRestrictionOwner>();
			var retList = new List<IRestrictionBase>();
			dataRestrictions.ForEach(dataRestriction => retList.AddRange(dataRestriction.RestrictionBaseCollection));
			return retList;
		}

		public ReadOnlyCollection<INote> NoteCollection()
		{
			var retList = new List<INote>(ScheduleDataInternalCollection().OfType<INote>());
			retList.Sort(new NoteByDateSorter());
			return new ReadOnlyCollection<INote>(retList);
		}

		public ReadOnlyCollection<IPublicNote> PublicNoteCollection()
		{
			var retList = new List<IPublicNote>(ScheduleDataInternalCollection().OfType<IPublicNote>());
			retList.Sort(new PublicNoteByDateSorter());
			return new ReadOnlyCollection<IPublicNote>(retList);
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
			return (internalCollection.OfType<IPersonAssignment>().Any() ||
					internalCollection.OfType<IPersonAbsence>().Any());
		}

		#region Methods (7)

		public void Merge(IScheduleDay source, bool isDelete)
		{
			Merge(source, isDelete, false);
		}

		public void Merge(IScheduleDay source, bool isDelete, bool ignoreTimeZoneChanges)
		{
			SchedulePartView view = source.SignificantPartForDisplay();

			switch (view)
			{
				case SchedulePartView.DayOff:
					if (isDelete) DeleteDayOff(); else mergeDayOff(source, true); break;

				case SchedulePartView.ContractDayOff:
					if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source);
					break;

				case SchedulePartView.FullDayAbsence:
					if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source); break;

				case SchedulePartView.Absence:
					if (isDelete) DeleteAbsence(false); else mergeAbsence(source); break;

				case SchedulePartView.MainShift:
					if (isDelete) DeleteMainShift(source); else mergeMainShift(source, ignoreTimeZoneChanges, true); break;

				case SchedulePartView.PersonalShift:
					if (isDelete) DeletePersonalStuff(); else mergePersonalStuff(source); break;

				case SchedulePartView.PreferenceRestriction:
					if (isDelete) DeletePreferenceRestriction(); else MergePreferenceRestriction(source); break;

				case SchedulePartView.StudentAvailabilityRestriction:
					if (isDelete) DeleteStudentAvailabilityRestriction(); else MergeStudentAvailabilityRestriction(source); break;
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
			return ass != null && ass.DayOff() != null;
		}

		private void MergePreferenceRestriction(IScheduleDay source)
		{
			TimeSpan diff = CalculatePeriodOffset(source.Period);
			DateTimePeriod period = source.Period.MovePeriod(diff);
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

		private void MergeStudentAvailabilityRestriction(IScheduleDay source)
		{
			TimeSpan diff = CalculatePeriodOffset(source.Period);
			DateTimePeriod period = source.Period.MovePeriod(diff);
			var date = new DateOnly(period.StartDateTimeLocal(TimeZone));

			foreach (IStudentAvailabilityRestriction studentAvailabilityRestriction in source.RestrictionCollection().OfType<IStudentAvailabilityRestriction>())
			{
				Clear<IStudentAvailabilityDay>();
				Add(new StudentAvailabilityDay(Person, date, new List<IStudentAvailabilityRestriction>{studentAvailabilityRestriction.NoneEntityClone()}));
			}
		}

			//borde tas bort!
		public void DeleteDayOff()
		{
			var ass = PersonAssignment();
					if (ass != null)
					{
						ass.SetDayOff(null);
					}
		}

		private void mergeDayOff(IScheduleDay source, bool deleteAbsence)
		{
			var authorization = PrincipalAuthorization.Instance();
			if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
				return;

			if (!PersonAbsenceCollection().IsEmpty() && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
				return;

			TimeSpan diff = CalculatePeriodOffset(source.Period);
			IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
			DateTimePeriod period = source.Period.MovePeriod(diff);

			if (deleteAbsence)
			{
			//loop absences
			foreach (IPersonAbsence personAbsence in PersonAbsenceCollection())
			{
				personAbsence.Split(period).ForEach(splitList.Add);
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
			IPersonAbsence personAbsenceUpForDelete = null;
		  
			foreach (var personAbsence in PersonAbsenceCollection())
			{
				if (Period.Intersect(personAbsence.Layer.Period))
					personAbsenceUpForDelete = personAbsence;

				if (personAbsenceUpForDelete == null) continue;

				IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
				var assignment = PersonAssignment();
				if (assignment != null && assignment.ShiftCategory != null)
				{
					if (assignment.Period != personAbsenceUpForDelete.Period)
					{
						personAbsenceUpForDelete.Split(source.Period).ForEach(splitList.Add);
					}
				}
				else
				{
					personAbsenceUpForDelete.Split(source.Period).ForEach(splitList.Add);
				}

				Remove(personAbsenceUpForDelete);
				splitList.ForEach(Add);
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
				if (layerCollection.HasLayers)
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

			foreach(var personAbsence in addList)
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
			//IPersonAbsence personAbsenceUpForDelete = null;
			IVisualLayerCollection layerCollection = ProjectionService().CreateProjection();
			IList<IPersonAbsence> removeList = new List<IPersonAbsence>();

			var layerCollectionPeriod = layerCollection.Period();
			foreach (IPersonAbsence personAbsence in PersonAbsenceCollection(true).Reverse())
			{
				if (layerCollection.HasLayers)
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

					if (Period.Intersect(personAbsence.Layer.Period))
					removeList.Add(personAbsence);
				}
			}


			foreach(IPersonAbsence personAbsence in removeList)
			{
				Remove(personAbsence);
			}
		}

		public void DeleteOvertime()
		{
			IPersonAssignment highAss = PersonAssignment();

			if (highAss != null)
				highAss.ClearOvertimeActivities();
		}

		public void DeleteMainShift(IScheduleDay source)
		{
			IPersonAssignment highAss = PersonAssignment();

			if (highAss != null)
			{
				highAss.ClearMainActivities();
				highAss.ClearOvertimeActivities();
			}
				
		}

		public void DeleteMainShiftSpecial(IScheduleDay source)
		{
			var highAss = PersonAssignment();

			if (highAss != null)
			{
				highAss.ClearMainActivities();
			}    
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
					workingCopyOfAssignment.Clear();
					workingCopyOfAssignment.SetShiftCategory(sourceAssignment.ShiftCategory);
					foreach (var layer in sourceAssignment.MainActivities())
					{
						workingCopyOfAssignment.AddActivity(layer.Payload, layer.Period.MovePeriod(periodOffset));
					}

					var period = source.Period.MovePeriod(periodOffset);
					if (PersonAssignment()==null && SignificantPart() == SchedulePartView.DayOff)
					{
						DeleteDayOff();
					}

			var currentAssignment = PersonAssignment(true);
					currentAssignment.SetActivitiesAndShiftCategoryFrom(workingCopyOfAssignment);
					if(splitAbsence) SplitAbsences(period);
					updateDateOnlyAsPeriod(workingCopyOfAssignment);
		}

		/// <summary>
		/// Updates the DateOnlyAsPeriod property. After the mainshift move to another timezone (only in copy paste where the UTC time
		/// remains the same) the propery might move to one day ahead or back. We need to update the property.
		/// Remark that most probably it only can happen with copy-paste where the ignoretimechanges param is true but for the sake of robust
		/// we also check the property even if the ignoretimechanges is false. 
		/// </summary>
		/// <param name="mainShift"></param>
				private void updateDateOnlyAsPeriod(IPersonAssignment mainShift)
				{
					if (mainShift.MainActivities().Any())
					{
						DateTimePeriod mainShiftPeriod = mainShift.Period;
						DateTime dateTime = mainShiftPeriod.StartDateTime;
						DateTime localDateTime = TimeZoneHelper.ConvertFromUtc(dateTime, Person.PermissionInformation.DefaultTimeZone());
						DateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(localDateTime.Date), Person.PermissionInformation.DefaultTimeZone());
					}
				}


		public void SplitAbsences(DateTimePeriod period)
		{
			IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
			IList<IPersonAbsence> deleteList = new List<IPersonAbsence>();
			IVisualLayerCollection layerCollection = ProjectionService().CreateProjection();
			if (!layerCollection.Period().HasValue) return;
			var dateTimePeriod = layerCollection.Period().Value;

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

			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence) && deleteList.Count > 0)
				return;

			//remove absences up for split
			deleteList.ForEach(Remove);

			//add new splitted absences
			foreach (IPersonAbsence newAbsence in splitList)
			{
				//newAbsence.LastChange = DateTime.UtcNow;
				Add(newAbsence);
			}      
		}

		public void DeletePersonalStuff()
		{
			IPersonAssignment ass = PersonAssignment();
			if (ass != null)
				ass.ClearPersonalActivities();
		}

		private void mergePersonalStuff(IScheduleDay source)
		{
			IPersonAssignment sourceAss = source.PersonAssignment();

			if (sourceAss != null)
			{
				var destAss = PersonAssignment(true);

				IPeriodOffsetCalculator periodOffsetCalculator = new PeriodOffsetCalculator();
				
				foreach (var personalLayer in sourceAss.PersonalActivities())
				{
					TimeSpan periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source.Period, Period);
					destAss.AddPersonalActivity(personalLayer.Payload, personalLayer.Period.MovePeriod(periodOffset));
				}

			}
		}

		public IScheduleTag ScheduleTag()
		{
			IList<IAgentDayScheduleTag> retList = new List<IAgentDayScheduleTag>(ScheduleDataInternalCollection().OfType<IAgentDayScheduleTag>());
			if(retList.Count == 0)
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
			var personAbsence = new PersonAbsence(Person, Scenario, layer) {LastChange = DateTime.UtcNow};
			Add(personAbsence);
	        return personAbsence;
		}
	   
		public void CreateAndAddOvertime(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet)
		{
					var foundPersonAssignment = PersonAssignment(true);
					foundPersonAssignment.AddOvertimeActivity(activity, period, definitionSet);
		}

		public void MergeOvertime(IScheduleDay source)
		{
			var dateOnlyPerson = DateOnlyAsPeriod.DateOnly;
			var period = Person.Period(dateOnlyPerson);
			var diff = CalculatePeriodOffset(source.Period);

			if (period != null)
			{
				var personAss = source.PersonAssignment();
				if (personAss != null)
				{
					foreach (var layer in personAss.OvertimeActivities())
					{
						if (period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
						{
							CreateAndAddOvertime(layer.Payload, layer.Period.MovePeriod(diff), layer.DefinitionSet);
						}
					}
				}
			}
		}

		public IScheduleDay CreateAndAddActivity(IActivity activity, DateTimePeriod period)
		{
			CreateAndAddActivity(activity, period, null);
			return this;
		}

		public void ModifyDictionary()
		{
			Owner.Modify(this);
		}

		public void CreateAndAddActivity(IActivity activity, DateTimePeriod period, IShiftCategory shiftCategory)
		{
			var authorization = PrincipalAuthorization.Instance();
			if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
				return;

			var ass = PersonAssignment();

			if ( ass != null)
			{
				if ((!ass.ShiftLayers.Any() || DateOnlyAsPeriod.Period().Contains(period.StartDateTime) || ass.Period.Intersect(period) || ass.Period.AdjacentTo(period)))
				{
					ass.AddActivity(activity, period);
					if (ass.ShiftCategory == null)
					{
						ass.SetShiftCategory(shiftCategory);
					}
					return;
				}
			}

			//TODO create inparameters to check on if to create new personassignment
			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
			newPersonAssignment.AddActivity(activity, period);
			newPersonAssignment.SetShiftCategory(shiftCategory);
			Add(newPersonAssignment);

			SplitAbsences(Period);
		}

		public void CreateAndAddPersonalActivity(IActivity activity, DateTimePeriod period)
		{
			var ass = PersonAssignment(true);
			ass.AddPersonalActivity(activity, period);
		}

			//will be removed
		public void AddMainShift(IEditableShift mainShift)
		{
			IPersonAssignment currentAss = PersonAssignment(true);

			new EditableShiftMapper().SetMainShiftLayers(currentAss, mainShift);
		}

			public void AddMainShift(IPersonAssignment mainShiftSource)
			{
				var currentAss = PersonAssignment(true);
				currentAss.SetActivitiesAndShiftCategoryFrom(mainShiftSource);
			}

		#endregion Methods

		public void Restore(IScheduleDay previousState)
		{
			Owner.Modify(ScheduleModifier.UndoRedo, previousState, null, new ResourceCalculationOnlyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
		}

		public IMemento CreateMemento()
		{
			return new Memento<IScheduleDay>(this, 
									Owner[Person].ReFetch(this));
		}

		public override string ToString()
		{
			//until we have implemented a cellmodel for the schedulepart this is needed since the grid calls this
			return "";
		}

		protected override void CloneDerived(Schedule clone)
		{
			var thisClone = ((ExtractedSchedule)clone);
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
				DeleteMainShift(this);

			if (options.Overtime)
				DeleteOvertime();

			if (options.PersonalShift)
				DeletePersonalStuff();

			if (options.Preference)
				DeletePreferenceRestriction();

			if (options.StudentAvailability)
				DeleteStudentAvailabilityRestriction();
			
			if(options.OvertimeAvailability)
				DeleteOvertimeAvailability();
		}
	}
}
