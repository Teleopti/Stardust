using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class LayerViewModelSelector :ILayerViewModelSelector
    {
        private readonly ILayerViewModel _selectedLayer;

        public LayerViewModelSelector(ILayerViewModel selectedLayer)
        {
            _selectedLayer = selectedLayer;
           
        }

      

        public bool ScheduleAffectsSameDayAndPerson(IScheduleDay schedule)
        {
            return new ScheduleAffectsSameDayAndPerson(SelectedLayer.SchedulePart).IsSatisfiedBy(schedule);
        }

        public bool TryToSelectLayer(IList<ILayerViewModel> layers, ILayerViewModelObserver observer)
        {
            foreach (ILayerViewModel model in layers)
            {
                if (model.Payload.GetType().Equals(_selectedLayer.Payload.GetType()) && model.Period.Equals(_selectedLayer.Period))
                {
                    observer.SelectLayer(model);
                    return true;
                }
            }
            return false;
        }

        public ILayerViewModel SelectedLayer
        {
            get { return _selectedLayer; }
        }

       
    }
}