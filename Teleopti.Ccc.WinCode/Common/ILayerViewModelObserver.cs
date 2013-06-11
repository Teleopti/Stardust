using System;

namespace Teleopti.Ccc.WinCode.Common
{
    public interface ILayerViewModelObserver 
    {
        void MoveAllLayers(ILayerViewModel sender,TimeSpan timeSpanToMove);

        void LayerMovedVertically(ILayerViewModel sender);

        void RemoveActivity(ILayerViewModel sender);

        void RemoveAbsence(ILayerViewModel sender);

        void SelectLayer(ILayerViewModel model);

    }
}