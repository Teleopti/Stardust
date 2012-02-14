using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Class for holding winclient common state
    /// </summary>
    /// <remarks>
    /// should be named "repository wrapper" or "read only data" or something later...
    /// </remarks>
    public class CommonStateHolder:ICommonStateHolder
    {
        private readonly IList<IAbsence> _absences = new List<IAbsence>();
        private readonly IList<IActivity> _activities = new List<IActivity>();
        private readonly IList<IActivity> _activeActivities = new List<IActivity>();
        private readonly IList<IShiftCategory> _shiftCategories = new List<IShiftCategory>();
        private readonly IList<IDayOffTemplate> _dayOffs = new List<IDayOffTemplate>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private  IList<IContract> _contracts = new List<IContract>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private ICollection<IContractSchedule> _contractSchedules = new List<IContractSchedule>();

        private IList<IScheduleTag> _scheduleTags;

        /// <summary>
        /// Loads the common state holder.
        /// </summary>
        /// <param name="repositoryFactory">The repository factory.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-29
        /// </remarks>
        public void LoadCommonStateHolder(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
        {
            using(unitOfWork.DisableFilter(QueryFilter.Deleted))
            {
                LoadActivity(repositoryFactory.CreateActivityRepository(unitOfWork));
                LoadAbsences(repositoryFactory.CreateAbsenceRepository(unitOfWork));
                LoadDayOffs(repositoryFactory.CreateDayOffRepository(unitOfWork));
                LoadShiftCategory(repositoryFactory.CreateShiftCategoryRepository(unitOfWork));
                LoadContracts(repositoryFactory.CreateContractRepository(unitOfWork));
                LoadContractSchedules(repositoryFactory.CreateContractScheduleRepository(unitOfWork));
                loadScheduleTags(repositoryFactory.CreateScheduleTagRepository(unitOfWork));
            }
        }

        private void loadScheduleTags(IScheduleTagRepository scheduleTagRepository)
        {
            _scheduleTags = (from t in scheduleTagRepository.LoadAll()
                              orderby t.Description
                              select t).ToList();

            //add nullen först
            _scheduleTags.Insert(0, NullScheduleTag.Instance);
        }
        
        public IList<IScheduleTag> ScheduleTags
        {
            get { return _scheduleTags; }
        }

        public IList<IScheduleTag> ScheduleTagsNotDeleted
        {
            get
            {
                return _scheduleTags.Where(scheduleTag => !scheduleTag.IsDeleted).ToList();
            }
        }
        
        /// <summary>
        /// Gets the absences.
        /// </summary>
        /// <value>The absences.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        public IList<IAbsence> Absences
        {
            get { return _absences; }
        }

        /// <summary>
        /// Gets the dayOffs
        /// </summary>
        public IList<IDayOffTemplate> DayOffs
        {
            get { return _dayOffs; }
        }

        /// <summary>
        /// Gets the activities.
        /// </summary>
        /// <value>The activities.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public IList<IActivity> Activities
        {
            get { return _activities; }
        }

        /// <summary>
        /// Gets the shift categories.
        /// </summary>
        /// <value>The shift categories.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-16
        /// </remarks>
        public IList<IShiftCategory> ShiftCategories
        {
            get { return _shiftCategories; }
        }

        public IList<IActivity> ActiveActivities
        {
            get { return _activeActivities; }
        }

        /// <summary>
        /// Loads the absences.
        /// </summary>
        /// <param name="absRep">The abs rep.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2007-11-08
        /// </remarks>
        private void LoadAbsences(IRepository<IAbsence> absRep)
        {
#pragma warning disable 0618
            ICollection<IAbsence> absList = absRep.LoadAll();
#pragma warning restore 0618
            foreach (IAbsence absence in absList)
            {
                Absences.Add(absence);
            }

            ((List<IAbsence>)Absences).Sort(new AbsenceSorter());
        }

        /// <summary>
        /// Loads the dayOffs
        /// </summary>
        /// <param name="dayOffRep"></param>
        private void LoadDayOffs(IRepository<IDayOffTemplate> dayOffRep)
        {
            ICollection<IDayOffTemplate> dayOffList = dayOffRep.LoadAll();

            foreach (IDayOffTemplate dayOff in from d in dayOffList orderby d.Description.Name select d)
            {
                DayOffs.Add(dayOff);
            }
        }

        /// <summary>
        /// Loads the activities.
        /// </summary>
        /// <param name="activityRep">The activity rep.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        private void LoadActivity(IRepository<IActivity> activityRep)
        {
#pragma warning disable 0618
            ICollection<IActivity> activityList = activityRep.LoadAll();
#pragma warning restore 0618
            foreach (IActivity activity in activityList)
            {
                if (!((IDeleteTag)activity).IsDeleted)
                    ActiveActivities.Add(activity);

                Activities.Add(activity);
            }
            ((List<IActivity>)Activities).Sort(new ActivitySorter());
            ((List<IActivity>)ActiveActivities).Sort(new ActivitySorter());
        }

        /// <summary>
        /// Loads the shift category.
        /// </summary>
        /// <param name="shiftCategoryRep">The shift category rep.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-16
        /// </remarks>
        private void LoadShiftCategory(IShiftCategoryRepository shiftCategoryRep)
        {
            ICollection<IShiftCategory> shiftCategoryList = shiftCategoryRep.FindAll();

            foreach (IShiftCategory shiftCategory in shiftCategoryList)
            {
                    ShiftCategories.Add(shiftCategory);
            }

            ((List<IShiftCategory>)ShiftCategories).Sort(new ShiftCategorySorter());
        }

        private void LoadContracts(IContractRepository contractRepository)
        {
            _contracts = new List<IContract>(contractRepository.FindAllContractByDescription());
        }
        private void LoadContractSchedules(IContractScheduleRepository contractScheduleRepository)
        {
            _contractSchedules = contractScheduleRepository.LoadAllAggregate();
        }
    }
}
