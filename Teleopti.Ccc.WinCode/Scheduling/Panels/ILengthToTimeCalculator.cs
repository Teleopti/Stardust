using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.Panels
{
    public interface ILengthToTimeCalculator
    {
        DateTime DateTimeFromPosition(double position);
        DateTime DateTimeFromPosition(double position, bool rightToLeft);

        double PositionFromDateTime(DateTime dateTime);
    }
}
