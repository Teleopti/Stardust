using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Teleopti.Ccc.WpfControls.Common.Interop;

namespace Teleopti.Ccc.Win.Common.Interop
{
    /// <summary>
    /// ElementHost that takes multiple objects (wpf,winform,clr-objects) and displays them with headers
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-01-13
    /// </remarks>
    public class MultipleElementHost:ElementHost
    {
        MultipleHostControl control = new MultipleHostControl();
        public MultipleElementHost()
        {
            Child = control;
            base.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Adds A child to the control
        /// By default, the control will display the item in tabs
        /// </summary>
        /// <param name="childHeader">The child header.</param>
        /// <param name="childContent">Content of the child.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-13
        /// </remarks>
        public void AddChild(object childHeader, object childContent)
        {
            control.AddItem(childHeader,childContent);
        }


    }
}
