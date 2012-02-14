using System;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public interface IPersonAccountView
    {
        void SetDate(DateTime dateTime);
        void DataLoaded();
        string DescriptionHeader { get; set; }
        string PeriodFromHeader { get; set; }
        string PeriodToHeader { get; set; }
        string TypeOfValueHeader { get; set; }
        string AccruedHeader { get; set; }
        string UsedHeader { get; set; }
        string RemainingHeader { get; set; }
        void SetDateText(string text);
    }
}