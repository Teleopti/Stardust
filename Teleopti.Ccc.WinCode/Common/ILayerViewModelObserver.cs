using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ILayerViewModelObserver 
    {
        void MoveAllLayers(ILayerViewModel sender,TimeSpan timeSpanToMove);

        void LayerMovedVertically(ILayerViewModel sender);

        IRemoveLayerFromScheduleService RemoveService { get; }

        void RemoveActivity(ILayerViewModel sender);

        void RemoveAbsence(ILayerViewModel sender);

        void SelectLayer(ILayerViewModel model);

    }
}