using System.Collections.Generic;
using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class MainShift
    {
        private MainShiftDto _dto;
        private readonly IList<ActivityLayer> _layerCollection = new List<ActivityLayer>();
        private readonly PersonAssignmentDto _personAssignmentDto;

        public MainShift(MainShiftDto mainShiftDto, PersonAssignmentDto personAssignmentDto)
        {
            _dto = mainShiftDto;
            _personAssignmentDto = personAssignmentDto;
            IList<ActivityLayerDto> activityLayerDtos = new List<ActivityLayerDto>(mainShiftDto.LayerCollection);
            foreach (ActivityLayerDto activityLayerDto in activityLayerDtos)
            {
                LayerCollection.Add(new ActivityLayer(activityLayerDto));
            }
        }

        public PersonAssignmentDto PersonAssignmentDto
        {
            get { return _personAssignmentDto; }
        }

        public MainShiftDto Dto
        {
            get { return _dto; }
        }

        public IList<ActivityLayer> LayerCollection
        {
            get { return _layerCollection; }
        }
    }
}
