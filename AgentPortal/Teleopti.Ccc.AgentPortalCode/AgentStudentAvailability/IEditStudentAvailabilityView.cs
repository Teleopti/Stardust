using System;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public interface IEditStudentAvailabilityView
    {
        bool SaveButtonEnabled { set; get; }

        TimeSpan? StartTimeLimitation { set; get; }
        TimeSpan? EndTimeLimitation { set; get; }
        bool EndTimeLimitationNextDay { set; get; }

        TimeSpan? SecondEndTimeLimitation { set; get; }
        TimeSpan? SecondStartTimeLimitation { set; get; }
        bool SecondEndTimeLimitationNextDay { set; get; }

        string StartTimeLimitationErrorMessage { get; set; }
        string EndTimeLimitationErrorMessage { get; set; }
        string SecondStartTimeLimitationErrorMessage { get; set; }
        string SecondEndTimeLimitationErrorMessage { get; set; }
        void HideView();
        void AllowInput(bool enable);
    }
}