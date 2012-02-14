using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Teleopti.Ccc.WinCodeTest.Common.CustomPanelsTest
{
    /// <summary>
    /// Helper for calling protected methods on a Panel
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-07-19
    /// </remarks>
    public static class TesterForPanels
    {
        public static Size CallMeasure(Panel target, Size parameter)
        {

            Panel instance = target;
            MethodInfo method = instance.GetType().GetMethod("MeasureOverride", BindingFlags.Instance | BindingFlags.NonPublic);
            return CallMethod(method, target, parameter);
            

        }

        public static Size CallArrange(Panel target, Size parameter)
        {
            Panel instance = target;
            MethodInfo method = instance.GetType().GetMethod("ArrangeOverride", BindingFlags.Instance | BindingFlags.NonPublic );
            return CallMethod(method, target, parameter);
        }

        private static Size CallMethod(MethodInfo method, Panel target, Size parameter)
        {
            if (method != null)
            {
                object ret = method.Invoke(target, new object[] { parameter });
                return (Size)ret;
            }
            throw new ArgumentException("Target must be a panel");
        }
    }
}
