using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Win.Scheduling
{
    class TeleoptiProgressChangeMessage
    {
        public string Message { get; set; }

        public TeleoptiProgressChangeMessage(string message)
        {
            Message = message;
        }
    }
}
