using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public static class WorkShiftMasterActivityChecker
    {
        public static bool DoesContainMasterActivity(ILayerCollectionOwner<IActivity> workShift)
        {
            if (workShift == null)
                return false;

            foreach (var layer in workShift.LayerCollection)
            {
                var payload = layer.Payload as IMasterActivity;
                if (payload != null)
                    return true;
            }
            return false;
        }
    }
}