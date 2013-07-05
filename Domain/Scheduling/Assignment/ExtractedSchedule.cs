using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;


namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
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
		    var personAssignment = AssignmentHighZOrder();
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

    	public IPersonAssignment AssignmentHighZOrder()
    	{
				return ScheduleDataInternalCollection().OfType<IPersonAssignment>().SingleOrDefault();
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

			//will be deleted!
        public ReadOnlyCollection<IPersonAssignment> PersonAssignmentCollection()
        {
	        var currentAssignment = AssignmentHighZOrder();
	        return currentAssignment == null ? 
						new List<IPersonAssignment>().AsReadOnly() : 
						new List<IPersonAssignment> {currentAssignment}.AsReadOnly();
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


        public ReadOnlyCollection<IPersonDayOff> PersonDayOffCollection()
        {
            //todo - when only ScheduleDay, no need to sort this one
            var retList = new List<IPersonDayOff>(ScheduleDataInternalCollection().OfType<IPersonDayOff>());
            return new ReadOnlyCollection<IPersonDayOff>(retList);
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

        public bool HasProjection
        {
            get
            {
                return (ScheduleDataInternalCollection().OfType<IPersonAssignment>().Any() ||
                        ScheduleDataInternalCollection().OfType<IPersonAbsence>().Any());
            }
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
                    if (isDelete) DeleteDayOff(); else mergeDayOff(source); break;

                case SchedulePartView.ContractDayOff:
                    if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source);
                    break;

                case SchedulePartView.FullDayAbsence:
                    if (isDelete) DeleteFullDayAbsence(source); else mergeFullDayAbsence(source); break;

                case SchedulePartView.Absence:
                    if (isDelete) DeleteAbsence(false); else mergeAbsence(source); break;

                case SchedulePartView.MainShift:
                    if (isDelete) DeleteMainShift(source); else mergeMainShift(source, ignoreTimeZoneChanges); break;

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

        public void RemoveEmptyAssignments()
        {
	        foreach (var assignment in PersonAssignmentCollection())
	        {
		        if (!assignment.PersonalLayers().Any() && !assignment.OvertimeLayers().Any() && assignment.ShiftCategory == null)
			        Remove(assignment);
	        }
        }

	    private void MergeStudentAvailabilityRestriction(IScheduleDay source)
        {
            TimeSpan diff = CalculatePeriodOffset(source.Period);
            DateTimePeriod period = source.Period.MovePeriod(diff);
            var date = new DateOnly(period.StartDateTimeLocal(TimeZone));

			foreach (IStudentAvailabilityRestriction studentAvailabilityRestriction in source.RestrictionCollection().OfType<IStudentAvailabilityRestriction>())
            {
				Clear<IStudentAvailabilityDay>();
				Add(new StudentAvailabilityDay(Person, date, new List<IStudentAvailabilityRestriction>{studentAvailabilityRestriction}));
            }
        }

        public void DeleteDayOff()
        {
            Clear<IPersonDayOff>();
        }

        private void mergeDayOff(IScheduleDay source)
        {
            var authorization = PrincipalAuthorization.Instance();
            if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff))
                return;

            if (!PersonAssignmentCollection().IsEmpty() && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
                return;

            if (!PersonAbsenceCollection().IsEmpty() && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence))
                return;

            IPersonDayOff workingCopyOfDayOff = source.PersonDayOffCollection()[0].NoneEntityClone();
            //TimeSpan diff = Period.StartDateTime.Subtract(source.Period.StartDateTime);
            TimeSpan diff = CalculatePeriodOffset(source.Period);
            IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
            DateTimePeriod period = source.Period.MovePeriod(diff);

            //loop absences
            foreach (IPersonAbsence personAbsence in PersonAbsenceCollection())
            {
                personAbsence.Split(period).ForEach(splitList.Add);
            }

            Clear<IPersonDayOff>();
            //Clear<IPersonAbsence>();

            IList<IPersonAbsence> filterList = new List<IPersonAbsence>(ScheduleDataInternalCollection().OfType<IPersonAbsence>());
            foreach (IPersonAbsence data in filterList)
            {
                if (data.Period.Intersect(Period))
                    Remove(data);
            }
            
            Clear<IPersonAssignment>();

            splitList.ForEach(Add);
            var date = new DateOnly(Period.StartDateTimeLocal(TimeZone));
            TimeZoneInfo timeZoneInfo = workingCopyOfDayOff.Person.PermissionInformation.DefaultTimeZone();
            if (workingCopyOfDayOff.UsedTimeZone != null)
                timeZoneInfo = workingCopyOfDayOff.UsedTimeZone;
            var personDayOff = new PersonDayOff(Person, Scenario, workingCopyOfDayOff.DayOff, date, timeZoneInfo);

            Add(personDayOff);     
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
                var assignment = AssignmentHighZOrder();
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

            foreach (var sourceAbsence in source.PersonAbsenceCollection())
            {
                if (layerCollection.HasLayers)
                {
                    if (sourceAbsence.Layer.Period.Contains(layerCollection.Period().Value))
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
		                LastChange = personAbsence.LastChange
	                };
	            Add(newAbsence);
            }   
        }

        public void DeleteAbsence(bool all)
        {
            //IPersonAbsence personAbsenceUpForDelete = null;
            IVisualLayerCollection layerCollection = ProjectionService().CreateProjection();
            IList<IPersonAbsence> removeList = new List<IPersonAbsence>();

            foreach (IPersonAbsence personAbsence in PersonAbsenceCollection(true).Reverse())
            {
                if (layerCollection.HasLayers)
                {
                    if (!personAbsence.Layer.Period.Contains(layerCollection.Period().Value))
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
            IList<IPersonAssignment> personAssToRemoveList = new List<IPersonAssignment>();

            foreach (IPersonAssignment assignment in PersonAssignmentCollection())
            {
							assignment.ClearOvertimeLayers();

								if (!assignment.PersonalLayers().Any() && assignment.ShiftCategory == null)
                    personAssToRemoveList.Add(assignment);
            }

            foreach (IPersonAssignment pAss in personAssToRemoveList)
                Remove(pAss);
        }

		public void DeleteMainShift(IScheduleDay source)
		{
			IPersonAssignment highAss = AssignmentHighZOrder();

			if (highAss != null)
				highAss.ClearMainLayers();

			RemoveEmptyAssignments();
		}

	    public TimeSpan CalculatePeriodOffset(DateTimePeriod sourcePeriod)
        {
            var periodOffsetCalculator = new PeriodOffsetCalculator();
            return periodOffsetCalculator.CalculatePeriodOffset(sourcePeriod, Period);

        }

        private void mergeMainShift(IScheduleDay source, bool ignoreTimeZoneChanges)
        {
			var sourceMainShift = source.GetEditorShift();
			if (sourceMainShift == null)
				return;

            var workingCopyOfMainShift = sourceMainShift.NoneEntityClone();
			workingCopyOfMainShift.LayerCollection.Clear();

            var sourceShiftPeriod = source.Period;
            if (workingCopyOfMainShift.LayerCollection.Period().HasValue)
                sourceShiftPeriod = workingCopyOfMainShift.LayerCollection.Period().Value;
            IPeriodOffsetCalculator periodOffsetCalculator = new PeriodOffsetCalculator();
            TimeSpan periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source, this, ignoreTimeZoneChanges, sourceShiftPeriod);
			foreach (var layer in sourceMainShift.LayerCollection)
			{
				var newLayer = new EditorActivityLayer(layer.Payload, layer.Period.MovePeriod(periodOffset));
				workingCopyOfMainShift.LayerCollection.Add(newLayer);
			}
	
            DateTimePeriod period = source.Period.MovePeriod(periodOffset);

            if (PersonAssignmentCollection().Count == 0)
            {
                if (SignificantPart() == SchedulePartView.DayOff)
                {
                    if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff))
                        return;

                    IPersonDayOff dayOff = PersonDayOffCollection()[0];
                    Remove(dayOff);
                }

                IPersonAssignment ass = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
                new EditableShiftMapper().SetMainShiftLayers(ass, workingCopyOfMainShift);
                Add(ass);
            }
            else
            {
                IPersonAssignment destAss = AssignmentHighZOrder();
				new EditableShiftMapper().SetMainShiftLayers(destAss, workingCopyOfMainShift);
            }

            SplitAbsences(period);
            updateDateOnlyAsPeriod(workingCopyOfMainShift);
        }

        /// <summary>
        /// Updates the DateOnlyAsPeriod property. After the mainshift move to another timezone (only in copy paste where the UTC time
        /// remains the same) the propery might move to one day ahead or back. We need to update the property.
        /// Remark that most probably it only can happen with copy-paste where the ignoretimechanges param is true but for the sake of robust
        /// we also check the property even if the ignoretimechanges is false. 
        /// </summary>
        /// <param name="mainShift"></param>
        private void updateDateOnlyAsPeriod(IEditableShift mainShift)
        {
            DateTimePeriod? mainShiftPeriod = mainShift.LayerCollection.Period();
            if(mainShiftPeriod.HasValue)
            {
                DateTime dateTime = mainShiftPeriod.Value.StartDateTime;
                DateTime localDateTime = TimeZoneHelper.ConvertFromUtc(dateTime, Person.PermissionInformation.DefaultTimeZone());
                DateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(localDateTime.Date), Person.PermissionInformation.DefaultTimeZone());
            }
        }

        public void SplitAbsences(DateTimePeriod period)
        {
            IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
            IList<IPersonAbsence> deleteList = new List<IPersonAbsence>();
            IVisualLayerCollection layerCollection = ProjectionService().CreateProjection();

            //loop absences in source
            foreach (IPersonAbsence personAbsence in PersonAbsenceCollection())
            {
                if (personAbsence.Layer.Period.Contains(layerCollection.Period().Value))
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
            IPersonAssignment ass = AssignmentHighZOrder();
            if (ass != null)
            {
                ass.ClearPersonalLayers();
                RemoveEmptyAssignments();
            }
        }

        private void mergePersonalStuff(IScheduleDay source)
        {
            IPersonAssignment sourceAss = source.AssignmentHighZOrder();
            IPersonAssignment destAss = AssignmentHighZOrder();

	        if (sourceAss != null)
	        {
		        if (destAss == null)
		        {
			        destAss = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
			        Add(destAss);
		        }

				IPeriodOffsetCalculator periodOffsetCalculator = new PeriodOffsetCalculator();
				
		        foreach (var personalLayer in sourceAss.PersonalLayers())
		        {
					TimeSpan periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source.Period, Period);
			        destAss.AddPersonalLayer(personalLayer.Payload, personalLayer.Period.MovePeriod(periodOffset));
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
            //TimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
            var dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, TimeZone));
            var personDayOff = new PersonDayOff(Person, Scenario, dayOff, dateOnly);
            // clear if there already is one
            DeleteDayOff();
            Add(personDayOff);           
        }
         public void CreateAndAddNote(string text)
         {
             var dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, Person.PermissionInformation.DefaultTimeZone() ));
             var note = new Note(Person, dateOnly, Scenario, text);
             DeleteNote();
             Add(note);
         }

         public void CreateAndAddPublicNote(string text)
         {
             var dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, Person.PermissionInformation.DefaultTimeZone()));
             var publicNote = new PublicNote(Person, dateOnly, Scenario, text);
             DeletePublicNote();
             Add(publicNote);
         }

        public void CreateAndAddAbsence(IAbsenceLayer layer)
        {
            var personAbsence = new PersonAbsence(Person, Scenario, layer) {LastChange = DateTime.UtcNow};
	        Add(personAbsence);
        }
       
        public void CreateAndAddOvertime(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet definitionSet)
        {
						//todo: rk - not sure about this.. 
						var foundPersonAssignment = AssignmentHighZOrder();
						if (foundPersonAssignment == null)
						{
							var newAss = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
							newAss.AddOvertimeLayer(activity, period, definitionSet);
							Add(newAss);
						}
						else
						{
							foundPersonAssignment.AddOvertimeLayer(activity, period, definitionSet);
						}
        }

				public void MergeOvertime(IScheduleDay source)
        {
            var timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
            var dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, timeZoneInfo));
            var period = Person.Period(dateOnlyPerson);
            var diff = CalculatePeriodOffset(source.Period);

            if (period != null)
            {
                foreach (var personAss in source.PersonAssignmentCollection())
                {
	                foreach (var layer in personAss.OvertimeLayers())
	                {
										if (period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
										{
											CreateAndAddOvertime(layer.Payload, layer.Period.MovePeriod(diff), layer.DefinitionSet);
										}
	                }
                }
            }
        }

				public void CreateAndAddActivity(IActivity activity, DateTimePeriod period, IShiftCategory shiftCategory)
        {
            var authorization = PrincipalAuthorization.Instance();
            if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
                return;

            if(SignificantPart() == SchedulePartView.DayOff && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff))
                return;

					var newLayer = new MainShiftLayer(activity, period);
			foreach (IPersonAssignment personAssignment in PersonAssignmentCollection())
			{
				if (personAssignment.Period.Intersect(period) || personAssignment.Period.AdjacentTo(period))
				{
					if (personAssignment.ShiftCategory == null)
					{
						personAssignment.SetMainShiftLayers(new[] {newLayer}, shiftCategory);
					}
					else
					{
						//introduce AddLayer on PersonAssignment instead?
						//rk: Micke and I have talked about this... 
						// Maybe remove SetMainShiftLayers and use Add/RemoveLayer instead.
						var oldLayers = personAssignment.MainLayers().ToList();
						oldLayers.Add(newLayer);
						personAssignment.SetMainShiftLayers(oldLayers, shiftCategory);
					}
					return;
				}
			}

			Clear<IPersonDayOff>();

			//TODO create inparameters to check on if to create new personassignment
			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
			newPersonAssignment.SetMainShiftLayers(new[] { newLayer }, shiftCategory);
			Add(newPersonAssignment);

			SplitAbsences(Period);
		}

		public void CreateAndAddPersonalActivity(IActivity activity, DateTimePeriod period)
		{
			var closest = PersonAssignmentCollection().FirstOrDefault();

			foreach (IPersonAssignment personAssignment in PersonAssignmentCollection())
			{
				if (personAssignment.Period.Intersect(period) || personAssignment.Period.AdjacentTo(period))
				{
					personAssignment.AddPersonalLayer(activity, period);
					return;
				}

				if (closest != null)
				{
					var diff = personAssignment.Period.StartDateTime.Subtract(period.StartDateTime);
					var closestDiff = closest.Period.StartDateTime.Subtract(period.StartDateTime);

					if (Math.Abs(diff.TotalSeconds) < Math.Abs(closestDiff.TotalSeconds))
						closest = personAssignment;
				}

			}

			if (closest != null)
			{
				closest.AddPersonalLayer(activity, period);
				return;
			}

			//TODO create inparameters to check on if to create new personassignment
			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
			newPersonAssignment.AddPersonalLayer(activity, period);
			Add(newPersonAssignment);
		}

        public void AddMainShift(IEditableShift mainShift)
        {
            IPersonAssignment currentAss = AssignmentHighZOrder();
            if (currentAss == null)
            {
                currentAss = new PersonAssignment(Person, Scenario, DateOnlyAsPeriod.DateOnly);
				Add(currentAss);
            }

			new EditableShiftMapper().SetMainShiftLayers(currentAss, mainShift);
        }

        #endregion Methods

        public void Restore(IScheduleDay previousState)
        {
            Owner.Modify(ScheduleModifier.UndoRedo, previousState, null, new EmptyScheduleDayChangeCallback(), new ScheduleTagSetter(NullScheduleTag.Instance));
        }

        public IMemento CreateMemento()
        {
            return new Memento<IScheduleDay>(this, 
                                    Owner[Person].ReFetch(this),
                                    string.Format(CultureInfo.CurrentUICulture, Resources.UndoRedoModifySchedule, Person.Name, Period.StartDateTime.ToShortDateString()));
        }

        public override string ToString()
        {
            //until we have implemented a cellmodel for the schedulepart this is needed since the grid calls this
            return "";
        }

        protected override void CloneDerived(Schedule clone)
        {
            ((ExtractedSchedule)clone).ServiceForSignificantPart = null;
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
