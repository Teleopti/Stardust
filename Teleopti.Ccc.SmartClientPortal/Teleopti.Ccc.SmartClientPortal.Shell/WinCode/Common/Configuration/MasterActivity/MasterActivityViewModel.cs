using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity
{
    public class MasterActivityViewModel : IMasterActivityViewModel
    {
        private readonly IActivityRepository _activityRepository;
        private readonly IMasterActivityRepository _masterActivityRepository;
        private IList<IMasterActivityModel> _cashedModels;
        private IList<IActivityModel> _cashedActivities;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

        public MasterActivityViewModel(IActivityRepository activityRepository, IMasterActivityRepository masterActivityRepository)
        {
            _activityRepository = activityRepository;
            _masterActivityRepository = masterActivityRepository;
        }

        public IList<IMasterActivityModel> AllNotDeletedMasterActivities
        {
            get
            {
                if (_cashedModels == null)
                {
                    _cashedModels = new List<IMasterActivityModel>();
                    var lst = _masterActivityRepository.LoadAll();
                    var sortedlsts = from a in lst orderby a.Description.Name select a;
                    foreach (var masterActivity in sortedlsts.Where(masterActivity => !masterActivity.IsDeleted))
                    {
                        _cashedModels.Add(new MasterActivityModel(masterActivity, _localizer));
                    }
                }
                return _cashedModels;
            }
        }

        public IList<IActivityModel> AllNotDeletedActivities
        {
            get
            {
                if (_cashedActivities == null)
                {
                    _cashedActivities = new List<IActivityModel>();
                }
                var lst = _activityRepository.LoadAll();
                var sortedlsts = from a in lst orderby a.Name select a;

                foreach (var activity in sortedlsts.Where(activity => !activity.IsDeleted))
                {
                    var newModel = new ActivityModel(activity);
                    if (!_cashedActivities.Contains(newModel))
                        _cashedActivities.Add(newModel);
                }

                return _cashedActivities;
            }
        }

        public IMasterActivityModel CreateNewMasterActivity(string newName)
        {
            var masterActivity = new Domain.Scheduling.MasterActivity { Description = new Description(newName, "") };
            _masterActivityRepository.Add(masterActivity);
            var ret = new MasterActivityModel(masterActivity, _localizer);
            _cashedModels.Add(ret);
            return ret;
        }

        public void DeleteMasterActivity(IMasterActivityModel masterActivityModel)
        {
            if (masterActivityModel == null) throw new ArgumentNullException("masterActivityModel");
            _masterActivityRepository.Remove(masterActivityModel.Entity);
            _cashedModels.Remove(masterActivityModel);
        }

        
    }
}