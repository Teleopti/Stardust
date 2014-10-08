using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Support.Code.Tool
{
    public class SetToggleModeCommand : ISupportCommand
    {
        private string toggleMode;

        public SetToggleModeCommand(string toggleMode)
        {
            this.toggleMode = toggleMode;
        }

        public void Execute(ModeFile mode)
        {
        }
    }
}
