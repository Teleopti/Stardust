using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity
{
    public class ActivityModel : IActivityModel
    {
        private readonly IActivity _activityEntity;

        public ActivityModel(IActivity activity)
        {
            _activityEntity = activity;
        }

        public bool IsDeleted
        {
            get
            {
                if (!_activityEntity.InContractTime)
                    return true;
                if (!_activityEntity.RequiresSkill)
                    return true;
                return ((Activity)_activityEntity).IsDeleted;
            }
        }

        public void SetDeleted()
        {
            //
        }

        public IActivity Entity
        {
            get { return _activityEntity; }
        }

        public string Name
        {
            get { return _activityEntity.Name; }
        }

        public override int GetHashCode()
        {
            return _activityEntity.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            IActivityModel ent = obj as IActivityModel;
            if (ent == null)
                return false;
            return ent.Entity.Equals(_activityEntity);
        }
    }
}