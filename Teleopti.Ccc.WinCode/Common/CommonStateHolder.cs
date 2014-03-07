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
    public class CommonStateHolder:ICommonStateHolder
    {
        private readonly List<IAbsence> _absences = new List<IAbsence>();
        private readonly List<IActivity> _activities = new List<IActivity>();
        private readonly List<IShiftCategory> _shiftCategories = new List<IShiftCategory>();
        private readonly List<IDayOffTemplate> _dayOffs = new List<IDayOffTemplate>();
        private readonly List<IScheduleTag> _scheduleTags = new List<IScheduleTag>();

        public void LoadCommonStateHolder(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork)
        {
			_absences.Clear();
			_activities.Clear();
			_dayOffs.Clear();
			_scheduleTags.Clear();
			_shiftCategories.Clear();

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
            _scheduleTags.AddRange(scheduleTagRepository.LoadAll().OrderBy(t => t.Description));
            _scheduleTags.Insert(0, NullScheduleTag.Instance);
        }
        
        public IEnumerable<IScheduleTag> ScheduleTags
        {
            get { return _scheduleTags; }
        }

        public IEnumerable<IScheduleTag> ActiveScheduleTags
        {
            get
            {
				return _scheduleTags.Where(scheduleTag => !scheduleTag.IsDeleted).ToArray();
            }
        }

		public IEnumerable<IAbsence> ActiveAbsences
		{
			get
			{
				return _absences.Where(a => !((IDeleteTag)a).IsDeleted).ToArray();
			}
		}

        public IEnumerable<IAbsence> Absences
        {
            get { return _absences; }
        }

        public IEnumerable<IDayOffTemplate> DayOffs
        {
            get { return _dayOffs; }
        }

        public IEnumerable<IDayOffTemplate> ActiveDayOffs
        {
			get { return _dayOffs.Where(d => !((IDeleteTag)d).IsDeleted).ToArray(); }
        }

	    public IDayOffTemplate DefaultDayOffTemplate
	    {
		    get
		    {
				var displayList = ActiveDayOffs.ToList();
				displayList.Sort(new DayOffTemplateSorter());

				return displayList[0];
		    }
	    }

        public IEnumerable<IActivity> Activities
        {
            get { return _activities; }
        }

		public IEnumerable<IShiftCategory> ShiftCategories
        {
            get { return _shiftCategories; }
        }

		public IEnumerable<IShiftCategory> ActiveShiftCategories
		{
			get { return _shiftCategories.Where(s => !((IDeleteTag)s).IsDeleted).ToArray(); }
		}

		public IEnumerable<IActivity> ActiveActivities
        {
			get { return _activities.Where(a => !a.IsDeleted).ToArray(); }
        }

	    private void LoadAbsences(IRepository<IAbsence> absRep)
	    {
		    _absences.AddRange(absRep.LoadAll());
		    _absences.Sort(new AbsenceSorter());
	    }

        private void LoadDayOffs(IRepository<IDayOffTemplate> dayOffRep)
        {
	        _dayOffs.AddRange(dayOffRep.LoadAll());
         	_dayOffs.Sort(new DayOffTemplateSorter());
        }

	    private void LoadActivity(IRepository<IActivity> activityRep)
	    {
		    _activities.AddRange(activityRep.LoadAll());
		    _activities.Sort(new ActivitySorter());
	    }

	    private void LoadShiftCategory(IShiftCategoryRepository shiftCategoryRep)
        {
            _shiftCategories.AddRange(shiftCategoryRep.FindAll());
            _shiftCategories.Sort(new ShiftCategorySorter());
        }

        private void LoadContracts(IContractRepository contractRepository)
        {
            contractRepository.FindAllContractByDescription();
        }
        private void LoadContractSchedules(IContractScheduleRepository contractScheduleRepository)
        {
            contractScheduleRepository.LoadAllAggregate();
        }
    }
}
