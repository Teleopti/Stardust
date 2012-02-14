using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class OvertimeLayer
    {
        private OvertimeLayerDto _dto;

        public OvertimeLayer(OvertimeLayerDto activityLayerDto)
        {
            _dto = activityLayerDto;
        }

        public OvertimeLayerDto Dto
        {
            get { return _dto; }
        }
    }
}
