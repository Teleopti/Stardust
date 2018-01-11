using System;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common
{
	public static class InvokeIfRequiredExtension
	{
		public static void InvokeIfRequired(this Control c, Action action)
		{
			if (c.InvokeRequired)
			{
				c.Invoke(new Action(action));
			}
			else
			{
				action();
			}
		}
	}
}