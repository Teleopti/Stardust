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

    	public bool IsScheduled()
    	{
    		SchedulePartView partView = SignificantPart();
    		return (partView == SchedulePartView.FullDayAbsence || partView == SchedulePartView.DayOff ||
    		        partView == SchedulePartView.ContractDayOff || partView == SchedulePartView.MainShift);
    	}

    	public IPersonAssignment AssignmentHighZOrder()
        {
            IPersonAssignment ret = null;

            IList<IPersonAssignment> assColl = PersonAssignmentCollection();

            if (assColl.Count > 0)
            {
                ret = assColl[0];

                foreach (IPersonAssignment pa in assColl)
                {
                    if (pa.ZOrder > ret.ZOrder)
                    {
                        ret = pa;
                    }
                }
            }

            if(ret != null && ret.ZOrder == DateTime.MinValue)
                ret.ZOrder = DateTime.Now;

            return ret;
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

		public ReadOnlyCollection<IPersonMeeting> PersonMeetingCollection(bool includeOutsideActualDay)
		{
			var sorter = new PersonMeetingByDateSorter();
			var org = new List<IPersonMeeting>(ScheduleDataInternalCollection().OfType<IPersonMeeting>());
			org.Sort(sorter);
			if (includeOutsideActualDay || org.IsEmpty())
				return new ReadOnlyCollection<IPersonMeeting>(org);
			return excludeDataOutsideDayOrShift(org, sorter);
		}

        public ReadOnlyCollection<IPersonAssignment> PersonAssignmentCollection()
        {
            var retList =
                new List<IPersonAssignment>(ScheduleDataInternalCollection().OfType<IPersonAssignment>());
            retList.Sort(new PersonAssignmentByDateSorter());
            return new ReadOnlyCollection<IPersonAssignment>(retList);
        }

        public ReadOnlyCollection<IScheduleData> PersonRestrictionCollection()
        {
            // temporärt så länge båda finns
            var scheduleDataInternalCollection = ScheduleDataInternalCollection();
            //IEnumerable<IPersonRestriction> personRestrictions = scheduleDataInternalCollection.OfType<IPersonRestriction>();
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

        public IList<IPersonAssignment> PersonAssignmentConflictCollection
        {
            get
            {
                return PersonAssignmentConflictInternalCollection;
            }
        }

        public ReadOnlyCollection<IPersonDayOff> PersonDayOffCollection()
        {
            //todo - when only ScheduleDay, no need to sort this one
            var retList = new List<IPersonDayOff>(ScheduleDataInternalCollection().OfType<IPersonDayOff>());
            //retList.Sort(new PersonDayOffByDateSorter());
            return new ReadOnlyCollection<IPersonDayOff>(retList);
        }

        public ReadOnlyCollection<INote> NoteCollection()
        {
            List<INote> retList = new List<INote>(ScheduleDataInternalCollection().OfType<INote>());
            retList.Sort(new NoteByDateSorter());
            return new ReadOnlyCollection<INote>(retList);
        }

        public ReadOnlyCollection<IPublicNote> PublicNoteCollection()
        {
            List<IPublicNote> retList = new List<IPublicNote>(ScheduleDataInternalCollection().OfType<IPublicNote>());
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
                    if (isDelete) DeleteDayOff(); else MergeDayOff(source); break;

                case SchedulePartView.ContractDayOff:
                    if (isDelete) DeleteFullDayAbsence(source); else MergeFullDayAbsence(source);
                    break;

                case SchedulePartView.FullDayAbsence:
                    if (isDelete) DeleteFullDayAbsence(source); else MergeFullDayAbsence(source); break;

                case SchedulePartView.Absence:
                    if (isDelete) DeleteAbsence(false); else MergeAbsence(source); break;

                case SchedulePartView.MainShift:
                    if (isDelete) DeleteMainShift(source); else MergeMainShift(source, ignoreTimeZoneChanges); break;

                case SchedulePartView.PersonalShift:
                    if (isDelete) DeletePersonalStuff(); else MergePersonalStuff(source); break;

                case SchedulePartView.PreferenceRestriction:
                    if (isDelete) DeletePreferenceRestriction(); else MergePreferenceRestriction(source); break;

                case SchedulePartView.StudentAvailabilityRestriction:
                    if (isDelete) DeleteStudentAvailabilityRestriction(); else MergeStudentAvailabilityRestriction(source); break;

                default: break;
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

        public void MergePreferenceRestriction(ISchedulePart source)
        {
            TimeSpan diff = CalculatePeriodOffset(source.Period);
            DateTimePeriod period = source.Period.MovePeriod(diff);
            DateOnly date = new DateOnly(period.StartDateTimeLocal(TimeZone));

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

        public void RemoveEmptyAssignments()
        {
            foreach (var assignment in PersonAssignmentCollection())
            {
                if (assignment.PersonalShiftCollection.Count == 0 && assignment.OvertimeShiftCollection.Count == 0 && assignment.MainShift == null)
                    Remove(assignment);
            }
        }

        public void MergeStudentAvailabilityRestriction(ISchedulePart source)
        {
            TimeSpan diff = CalculatePeriodOffset(source.Period);
            DateTimePeriod period = source.Period.MovePeriod(diff);
            DateOnly date = new DateOnly(period.StartDateTimeLocal(TimeZone));

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

        private void MergeDayOff(IScheduleDay source)
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
            DateOnly date = new DateOnly(Period.StartDateTimeLocal(TimeZone));
            TimeZoneInfo timeZoneInfo = workingCopyOfDayOff.Person.PermissionInformation.DefaultTimeZone();
            if (workingCopyOfDayOff.UsedTimeZone != null)
                timeZoneInfo = workingCopyOfDayOff.UsedTimeZone;
            PersonDayOff personDayOff = new PersonDayOff(Person, Scenario, workingCopyOfDayOff.DayOff, date, timeZoneInfo);

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
                if (assignment != null && assignment.MainShift != null && assignment.MainShift.HasProjection)
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

        private void MergeAbsence(IScheduleDay source)
        {
            MergeAbsences(source, false);
        }

        private void MergeFullDayAbsence(IScheduleDay source)
        {
            MergeAbsences(source, false);
        }

        public void MergeAbsences(IScheduleDay source, bool all)
        {
            List<IPersonAbsence> addList = new List<IPersonAbsence>();
            TimeSpan diff = CalculatePeriodOffset(source.Period);
            IVisualLayerCollection layerCollection = source.ProjectionService().CreateProjection();
            var layerCollectionPeriod = layerCollection.Period();

            foreach (IPersonAbsence sourceAbsence in source.PersonAbsenceCollection())
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

            foreach(IPersonAbsence personAbsence in addList)
            {
                personAbsence.Layer.MoveLayer(diff);
                IPersonAbsence newAbsence = new PersonAbsence(Person,Scenario,personAbsence.Layer);
                newAbsence.LastChange = personAbsence.LastChange;
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
            IList<IOvertimeShift> overtimeShiftsToRemoveList = new List<IOvertimeShift>();
            IList<IPersonAssignment> personAssToRemoveList = new List<IPersonAssignment>();

            foreach (IPersonAssignment assignment in PersonAssignmentCollection())
            {
                foreach (IOvertimeShift overtimeShift in assignment.OvertimeShiftCollection)
                {
                    overtimeShiftsToRemoveList.Add(overtimeShift);
                }

                foreach (IOvertimeShift overTime in overtimeShiftsToRemoveList)
                {
                    assignment.RemoveOvertimeShift(overTime);
                }

                if (assignment.PersonalShiftCollection.Count == 0 && assignment.MainShift == null)
                    personAssToRemoveList.Add(assignment);
            }

            foreach (IPersonAssignment pAss in personAssToRemoveList)
                Remove(pAss);
        }

        public void MergePersonalShiftsToOneAssignment(IMainShift mainShift)
        {
            IPersonAssignment currentAss = AssignmentHighZOrder();
            IList<IPersonAssignment> assignments = new List<IPersonAssignment>();
            IList<IPersonAssignment> assignmentsToDelete = new List<IPersonAssignment>();
            ((List<IPersonAssignment>)assignments).AddRange(PersonAssignmentCollection());

            var mainShiftPeriod = mainShift.LayerCollection.Period();

            foreach (IPersonAssignment assignment in assignments)
            {
                if (currentAss == null || assignment == currentAss)
                    continue;
                IList<IPersonalShift> shiftsToMove = new List<IPersonalShift>();
                foreach (IPersonalShift shift in assignment.PersonalShiftCollection)
                {
                    var currentShiftPeriod = shift.LayerCollection.Period();
                    if (mainShiftPeriod.Value.ContainsPart(currentShiftPeriod.Value) || mainShiftPeriod.Value.Adjacent(currentShiftPeriod.Value))
                        shiftsToMove.Add(shift);
                }
                foreach (IPersonalShift shift in shiftsToMove)
                {
                    assignment.RemovePersonalShift(shift);
                    currentAss.AddPersonalShift((IPersonalShift)shift.NoneEntityClone());
                    if (!assignmentsToDelete.Contains(assignment))
                        assignmentsToDelete.Add(assignment);
                }
            }
            RemoveEmptyAssignments();
        }

		public void DeleteMainShift(IScheduleDay source)
		{
			IPersonAssignment destAss = new PersonAssignment(Person, Scenario);
			IPersonAssignment highAss = AssignmentHighZOrder();

			if (highAss != null)
			{
				foreach (IPersonalShift personalShift in highAss.PersonalShiftCollection)
				{
					IPersonalShift destPersonalShift = (IPersonalShift)personalShift.NoneEntityClone();
					TimeSpan diff = CalculatePeriodOffset(source.Period);
					((LayerCollection<IActivity>)destPersonalShift.LayerCollection).MoveAllLayers(diff);

					destAss.AddPersonalShift(destPersonalShift);
				}

                //foreach (IOvertimeShift overtimeShift in highAss.OvertimeShiftCollection)
                //{
                //    IOvertimeShift destOvertimeShift = (IOvertimeShift)overtimeShift.NoneEntityClone();
                //    TimeSpan diff = CalculatePeriodOffset(source.Period);
                //    ((LayerCollection<IActivity>)destOvertimeShift.LayerCollection).MoveAllLayers(diff);

                //    destAss.AddOvertimeShift(destOvertimeShift);
                //}

				RemovePersonAssignment(AssignmentHighZOrder());

				if (destAss.PersonalShiftCollection.Count > 0)
					Add(destAss);
			}
		}

    	public TimeSpan CalculatePeriodOffset(DateTimePeriod sourcePeriod)
        {
            IPeriodOffsetCalculator periodOffsetCalculator = new PeriodOffsetCalculator();
            return periodOffsetCalculator.CalculatePeriodOffset(sourcePeriod, Period);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void MergeMainShift(IScheduleDay source, bool ignoreTimeZoneChanges)
        {
        	IPersonAssignment personAssignment = source.AssignmentHighZOrder();
			if (personAssignment == null)
				return;

        	IMainShift sourceMainShift = personAssignment.MainShift;
			if (sourceMainShift == null)
				return;

            IMainShift workingCopyOfMainShift = (IMainShift)sourceMainShift.NoneEntityClone();
            var sourceShiftPeriod = source.Period;
            var workingCopyShiftPeriod = workingCopyOfMainShift.LayerCollection.Period();
            if (workingCopyShiftPeriod.HasValue)
                sourceShiftPeriod = workingCopyShiftPeriod.Value;
            IPeriodOffsetCalculator periodOffsetCalculator = new PeriodOffsetCalculator();
            TimeSpan periodOffset = periodOffsetCalculator.CalculatePeriodOffset(source, this, ignoreTimeZoneChanges, sourceShiftPeriod);
            workingCopyOfMainShift.LayerCollection.MoveAllLayers(periodOffset);
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

                IPersonAssignment ass = new PersonAssignment(Person, Scenario);
                ass.SetMainShift(workingCopyOfMainShift);
                Add(ass);
            }
            else
            {
                MergePersonalShiftsToOneAssignment(workingCopyOfMainShift);
                IPersonAssignment destAss = AssignmentHighZOrder();

                if (destAss.MainShift == null)
                {
                    destAss.SetMainShift(workingCopyOfMainShift);
                }
                else
                {
                    destAss.MainShift.Transform(workingCopyOfMainShift);
                    destAss.MainShift.ShiftCategory = workingCopyOfMainShift.ShiftCategory;
                }
            }

            SplitAbsences(period);
            UpdateDateOnlyAsPeriod(workingCopyOfMainShift);
        }

        /// <summary>
        /// Updates the DateOnlyAsPeriod property. After the mainshift move to another timezone (only in copy paste where the UTC time
        /// remains the same) the propery might move to one day ahead or back. We need to update the property.
        /// Remark that most probably it only can happen with copy-paste where the ignoretimechanges param is true but for the sake of robust
        /// we also check the property even if the ignoretimechanges is false. 
        /// </summary>
        /// <param name="mainShift"></param>
        private void UpdateDateOnlyAsPeriod(IMainShift mainShift)
        {
            DateTimePeriod? mainShiftPeriod = mainShift.LayerCollection.Period();
            if(mainShiftPeriod.HasValue)
            {
                DateTime dateTime = mainShiftPeriod.Value.StartDateTime;
                DateTime localDateTime = TimeZoneHelper.ConvertFromUtc(dateTime, Person.PermissionInformation.DefaultTimeZone());
                DateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(localDateTime.Date), this.Person.PermissionInformation.DefaultTimeZone());
            }
        }

        public void SplitAbsences(DateTimePeriod period)
        {
            IList<IPersonAbsence> splitList = new List<IPersonAbsence>();
            IList<IPersonAbsence> deleteList = new List<IPersonAbsence>();
            IVisualLayerCollection layerCollection = ProjectionService().CreateProjection();
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
            IPersonAssignment ass = AssignmentHighZOrder();
            if (ass != null)
            {
                ass.ClearPersonalShift();
                if (!ass.HasProjection)
                    Remove(ass);
            }
        }

        private void MergePersonalStuff(IScheduleDay source)
        {
            IPersonAssignment sourceAss = source.AssignmentHighZOrder();
            IPersonAssignment destAss = AssignmentHighZOrder();

            if (sourceAss != null)
            {
                if (destAss == null)
                {
                    destAss = new PersonAssignment(Person, Scenario);

                    foreach (IPersonalShift personalShift in sourceAss.PersonalShiftCollection)
                    {
                        IPersonalShift destPersonalShift = (IPersonalShift)personalShift.NoneEntityClone();
                        //TimeSpan diff = Period.StartDateTimeLocal(source.TimeZone).Subtract(source.Period.StartDateTimeLocal(source.TimeZone));
                        TimeSpan diff = CalculatePeriodOffset(source.Period);
                        destPersonalShift.LayerCollection.MoveAllLayers(diff);

                        destAss.AddPersonalShift(destPersonalShift);
                    }

                    Add(destAss);
                }
                else
                {
                    foreach (IPersonalShift personalShift in sourceAss.PersonalShiftCollection)
                    {
                        IPersonalShift destPersonalShift = (IPersonalShift)personalShift.NoneEntityClone();
                        //TimeSpan diff = Period.StartDateTimeLocal(source.TimeZone).Date.Subtract(source.Period.StartDateTimeLocal(source.TimeZone).Date);
                        TimeSpan diff = CalculatePeriodOffset(source.Period);
                        destPersonalShift.LayerCollection.MoveAllLayers(diff);

                        var period = destPersonalShift.LayerCollection.Period();
                        if (period.HasValue)
                        {
                            if (destAss.Period.Contains(period.Value))
                                destAss.AddPersonalShift(destPersonalShift);
                        }
                    }
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
            DateOnly dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, TimeZone));
            PersonDayOff personDayOff = new PersonDayOff(Person, Scenario, dayOff, dateOnly);
            // clear if there already is one
            DeleteDayOff();
            Add(personDayOff);           
        }
         public void CreateAndAddNote(string text)
         {
             DateOnly dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, Person.PermissionInformation.DefaultTimeZone() ));
             INote note = new Note(Person, dateOnly, Scenario, text);
             DeleteNote();
             Add(note);
         }

         public void CreateAndAddPublicNote(string text)
         {
             DateOnly dateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, Person.PermissionInformation.DefaultTimeZone()));
             IPublicNote publicNote = new PublicNote(Person, dateOnly, Scenario, text);
             DeletePublicNote();
             Add(publicNote);
         }

        public void CreateAndAddAbsence(IAbsenceLayer layer)
        {
            IPersonAbsence personAbsence = new PersonAbsence(Person, Scenario, layer);
            personAbsence.LastChange = DateTime.UtcNow;
            Add(personAbsence);
        }

        private bool CanCreateAndAddOvertime(IOvertimeShiftActivityLayer overtimeShiftActivityLayer)
        {
            var periodContainsOvertimeTime = Period.Contains(overtimeShiftActivityLayer.Period.StartDateTime);
            var canFindPossiblePersonAssignment = FindPersonAssignmentToConnectOvertimeTo(overtimeShiftActivityLayer) != null;
            return periodContainsOvertimeTime || canFindPossiblePersonAssignment;
        }
       
        public void CreateAndAddOvertime(IOvertimeShiftActivityLayer overtimeShiftActivityLayer)
        {
            if (CanCreateAndAddOvertime(overtimeShiftActivityLayer))
            {
				var overtimeShift = new OvertimeShift();
                var foundPersonAssignment = FindPersonAssignmentToConnectOvertimeTo(overtimeShiftActivityLayer);
                if(foundPersonAssignment!=null)
                {		
					foundPersonAssignment.AddOvertimeShift(overtimeShift);
					overtimeShift.LayerCollection.Add(overtimeShiftActivityLayer);
	                return;
                }

				foundPersonAssignment = new PersonAssignment(Person, Scenario);	
                foundPersonAssignment.AddOvertimeShift(overtimeShift);
				overtimeShift.LayerCollection.Add(overtimeShiftActivityLayer);
                Add(foundPersonAssignment);
            }
        }

        private IPersonAssignment FindPersonAssignmentToConnectOvertimeTo(IOvertimeShiftActivityLayer overtimeShiftActivityLayer)
        {
            var personAssignments = from personAssignment in PersonAssignmentCollection()
                                    where OvertimeCanBeConnectedToPersonAssignment(personAssignment, overtimeShiftActivityLayer)
                                    select personAssignment;
            return personAssignments.FirstOrDefault();
        }

        private static bool OvertimeCanBeConnectedToPersonAssignment(IPersonAssignment personAssignment, IOvertimeShiftActivityLayer overtimeShiftActivityLayer)
        {
            return personAssignment.Period.Adjacent(overtimeShiftActivityLayer.Period) ||
                   personAssignment.Period.Intersect(overtimeShiftActivityLayer.Period);
        }

        public void MergeOvertime(ISchedulePart source)
        {
            TimeZoneInfo timeZoneInfo = Person.PermissionInformation.DefaultTimeZone();
            DateOnly dateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(Period.StartDateTime, timeZoneInfo));
            IPersonPeriod period = Person.Period(dateOnlyPerson);
            TimeSpan diff = CalculatePeriodOffset(source.Period);

            if (period != null)
            {
                foreach (IPersonAssignment personAss in source.PersonAssignmentCollection())
                {
                    foreach (IOvertimeShift overtime in personAss.OvertimeShiftCollection)
                    {
                        foreach (IOvertimeShiftActivityLayer layer in overtime.LayerCollection)
                        {
                            if (period.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Contains(layer.DefinitionSet))
                            {
                                IOvertimeShiftActivityLayer clonedLayer = (IOvertimeShiftActivityLayer)layer.NoneEntityClone();
                                clonedLayer.MoveLayer(diff);
                                CreateAndAddOvertime(clonedLayer);

                            }
                        }
                    }
                }
            }
        }

        public void CreateAndAddActivity(IMainShiftActivityLayer layer, IShiftCategory shiftCategory)
        {
            var authorization = PrincipalAuthorization.Instance();
            if (!authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment))
                return;

            if(SignificantPart() == SchedulePartView.DayOff && !authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff))
                return;

			IMainShift mainShift = new MainShift(shiftCategory);
			mainShift.LayerCollection.Add(layer);
			MergePersonalShiftsToOneAssignment(mainShift);
			foreach (IPersonAssignment personAssignment in PersonAssignmentCollection())
			{
				if (personAssignment.Period.Intersect(layer.Period) || personAssignment.Period.Adjacent(layer.Period))
				{
					if (personAssignment.MainShift == null)
					{
						personAssignment.SetMainShift(mainShift);
					}
					else
					{
						personAssignment.MainShift.LayerCollection.Add(layer);
					}
					return;
				}
			}

			Clear<IPersonDayOff>();

			//TODO create inparameters to check on if to create new personassignment
			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario);
			newPersonAssignment.SetMainShift(new MainShift(shiftCategory));
			newPersonAssignment.MainShift.LayerCollection.Add(layer);
			Add(newPersonAssignment);

			SplitAbsences(Period);
		}

		public void CreateAndAddPersonalActivity(IPersonalShiftActivityLayer layer)
		{
			IPersonalShift personalShift = new PersonalShift();
			personalShift.LayerCollection.Add(layer);

			foreach (IPersonAssignment personAssignment in PersonAssignmentCollection())
			{
				if (personAssignment.Period.Intersect(layer.Period) || personAssignment.Period.Adjacent(layer.Period))
				{
					personAssignment.AddPersonalShift(personalShift);
					return;
				}
			}

			//TODO create inparameters to check on if to create new personassignment
			IPersonAssignment newPersonAssignment = new PersonAssignment(Person, Scenario);
			newPersonAssignment.AddPersonalShift(personalShift);
			Add(newPersonAssignment);
		}

        public void AddMainShift(IMainShift mainShift)
        {
            IPersonAssignment currentAss = AssignmentHighZOrder();
            MergePersonalShiftsToOneAssignment(mainShift);
            if (currentAss == null)
            {
                IPersonAssignment personAssignment = new PersonAssignment(Person, Scenario);
                personAssignment.SetMainShift(mainShift);
                Add(personAssignment);
            }
            else
            {
                currentAss.SetMainShift(mainShift);
            }
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
        }
    }
}
