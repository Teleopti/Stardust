using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ILayerViewModelObserver 
    {
        void MoveAllLayers(ILayerViewModel sender,TimeSpan timeSpanToMove);

        void LayerMovedVertically(ILayerViewModel sender);

        void RemoveActivity(ILayerViewModel sender, ILayer<IActivity> activityLayer,IScheduleDay scheduleDay);

		void RemoveAbsence(ILayerViewModel sender, ILayer<IAbsence> absenceLayer, IScheduleDay scheduleDay);

        void SelectLayer(ILayerViewModel model);

		void ReplaceActivity(ILayerViewModel sender, ILayer<IActivity> layer, IScheduleDay scheduleDay);

		void ReplaceAbsence(ILayerViewModel sender, ILayer<IAbsence> layer, IScheduleDay scheduleDay);

	    void UpdateAllMovedLayers();
    }
}