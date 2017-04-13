using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity
{
    public class MasterActivityModel : IMasterActivityModel
    {
        private readonly IMasterActivity _masterActivityEntity;
        private readonly ILocalizedUpdateInfo _localizer;

        public MasterActivityModel(IMasterActivity masterActivityEntity, ILocalizedUpdateInfo localizer)
        {
            _masterActivityEntity = masterActivityEntity;
            _localizer = localizer;
        }

        public IList<IActivityModel> Activities
        {
            get
            {
                return _masterActivityEntity.ActivityCollection.Select(activity => new ActivityModel(activity)).Cast<IActivityModel>().ToList();
            }
        }

        public Color Color
        {
            get { return _masterActivityEntity.DisplayColor; }
            set { _masterActivityEntity.DisplayColor = value; }
        }

        public string Name
        {
            get { return _masterActivityEntity.Description.Name; }
            set { _masterActivityEntity.Description = new Description(value, _masterActivityEntity.Description.ShortName); }
        }

        public string UpdateInfo
        {
            get
            {
                return _localizer.UpdatedByText(_masterActivityEntity, UserTexts.Resources.UpdatedByColon);
            }
        }

        public IMasterActivity Entity
        {
            get { return _masterActivityEntity; }
        }

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