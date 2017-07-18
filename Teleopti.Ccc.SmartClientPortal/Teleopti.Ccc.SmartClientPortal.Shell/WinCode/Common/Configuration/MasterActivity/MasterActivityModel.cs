using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity
{
    public class MasterActivityModel : IMasterActivityModel
    {
        private readonly IMasterActivity _masterActivityEntity;
        private readonly LocalizedUpdateInfo _localizer;

        public MasterActivityModel(IMasterActivity masterActivityEntity, LocalizedUpdateInfo localizer)
        {
            _masterActivityEntity = masterActivityEntity;
            _localizer = localizer;
        }

        public IList<IActivityModel> Activities
        {
            get
            {
                return _masterActivityEntity.ActivityCollection.Select(activity => (IActivityModel)new ActivityModel(activity)).ToList();
            }
        }

        public Color Color
        {
            get => _masterActivityEntity.DisplayColor;
	        set => _masterActivityEntity.DisplayColor = value;
        }

        public string Name
        {
            get => _masterActivityEntity.Description.Name;
	        set => _masterActivityEntity.Description = new Description(value, _masterActivityEntity.Description.ShortName);
        }

        public string UpdateInfo => _localizer.UpdatedByText(_masterActivityEntity, UserTexts.Resources.UpdatedByColon);

	    public IMasterActivity Entity => _masterActivityEntity;

	    public void UpdateActivities(IList<IActivityModel> activities)
        {
            var lst = activities.Select(activityModel => activityModel.Entity).ToList();
            _masterActivityEntity.UpdateActivityCollection(lst);
        }

        public override bool Equals(object obj)
        {
            var masterObject = obj as MasterActivityModel;
            return masterObject != null && Entity.Equals(masterObject.Entity);
        }

        public bool Equals(MasterActivityModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._masterActivityEntity, _masterActivityEntity);
        }

        public override int GetHashCode()
        {
            return (_masterActivityEntity != null ? _masterActivityEntity.GetHashCode() : 0);
        }
    }
}