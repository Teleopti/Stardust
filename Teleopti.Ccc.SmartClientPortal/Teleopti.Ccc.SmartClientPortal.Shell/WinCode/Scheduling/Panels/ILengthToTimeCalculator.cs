using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels
{
    public interface ILengthToTimeCalculator
    {
        DateTime DateTimeFromPosition(double position);
        DateTime DateTimeFromPosition(double position, bool rightToLeft);

        double PositionFromDateTime(DateTime dateTime);
    }
}
