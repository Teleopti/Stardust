using System.Collections.Generic;
using System.Linq;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class OvertimeShift
    {
        private ShiftDto _dto;
        private readonly IList<OvertimeLayer> _layerCollection = new List<OvertimeLayer>();
 
        public OvertimeShift(ShiftDto shiftDto)
        {
            _dto = shiftDto;
            if (shiftDto.LayerCollection==null) return;
            IList<OvertimeLayerDto> activityLayerDtos = new List<OvertimeLayerDto>(shiftDto.LayerCollection.OfType<OvertimeLayerDto>());
            foreach (OvertimeLayerDto activityLayerDto in activityLayerDtos)
            {
                LayerCollection.Add(new OvertimeLayer(activityLayerDto));
            }
        }

        public ShiftDto Dto
        {
            get { return _dto; }
        }

        public IList<OvertimeLayer> LayerCollection
        {
            get { return _layerCollection; }
        }
    }
}
