using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Common
{
    public class GuiHelper
    {
        private int stopIndex;

        /// <summary>
        /// Gets the active control (goes inside ContainerControls to find the actual active control).
        /// </summary>
        /// <param name="theControl">The control (typically Form or Forms ActiveControl.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-10
        /// </remarks>
        public Control GetActiveControl(Control theControl)
        {
            ContainerControl containerControl = theControl as ContainerControl;
            if (containerControl != null &&
                stopIndex < 10)
            {
                stopIndex++;
                return containerControl.ActiveControl != null ? GetActiveControl(containerControl.ActiveControl) : containerControl; //returns containercontrol if containerControl.ActiveControl == null //fix for bug:3368 Peter
            }
            stopIndex = 0;
            return theControl;
        }

    }
}
