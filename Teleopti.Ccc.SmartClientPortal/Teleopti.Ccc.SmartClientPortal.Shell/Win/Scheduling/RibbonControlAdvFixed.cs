using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Tools.Win32API;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    // This code exists to fix this bug:
    //
   //at Syncfusion.Windows.Forms.Tools.RibbonControlAdv.UpdateContextMenu(IntPtr hWnd, POINT point)
   //at Syncfusion.Windows.Forms.Tools.RibbonControlAdv.OnWmContextMenu(IntPtr hWnd, IntPtr lParam)
   //at Syncfusion.Windows.Forms.Tools.RibbonControlAdv.CallWndProc(IntPtr hWnd, Int32 nMsg, IntPtr wParam, IntPtr lParam)
   //at Syncfusion.Windows.Forms.Tools.CallWndProcHook.CallWndProcInternal(Int32 nCode, IntPtr wParam, IntPtr lParam)
   //at Syncfusion.Windows.Forms.Tools.WndHook.CallWndProc(Int32 nCode, IntPtr wParam, IntPtr lParam)
   //at MS.Win32.UnsafeNativeMethods.CallWindowProc(IntPtr wndProc, IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   //at MS.Win32.HwndSubclass.DefWndProcWrapper(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   //at MS.Win32.UnsafeNativeMethods.CallWindowProc(IntPtr wndProc, IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   //at MS.Win32.HwndSubclass.SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   //at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG& msg)
   //at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(Int32 dwComponentID, Int32 reason, Int32 pvLoopData)
   //at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)
   //at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)
   //at System.Windows.Forms.Application.Run(Form mainForm)
   //at Microsoft.Practices.CompositeUI.WinForms.FormShellApplication`2.Start()
   //at Microsoft.Practices.CompositeUI.CabApplication`1.Run()
   //at Teleopti.Ccc.SmartClientPortal.Shell.SmartClientShellApplication.LoadShellApplicationDebug() in C:\RaptorScrum\Root\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\ShellApplication.cs:line 104
   //at Teleopti.Ccc.SmartClientPortal.Shell.SmartClientShellApplication.Main() in C:\RaptorScrum\Root\Teleopti.Ccc.SmartClientPortal\Teleopti.Ccc.SmartClientPortal.Shell\ShellApplication.cs:line 65
   //at System.AppDomain._nExecuteAssembly(Assembly assembly, String[] args)
   //at System.AppDomain.ExecuteAssembly(String assemblyFile, Evidence assemblySecurity, String[] args)
   //at Microsoft.VisualStudio.HostingProcess.HostProc.RunUsersAssembly()
   //at System.Threading.ThreadHelper.ThreadStart_Context(Object state)
   //at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
   //at System.Threading.ThreadHelper.ThreadStart()
    //
    // Which occurred when in the schedule view:
    // Select an area in the grid
    // Press delete
    // Click schedule
    // Click ok
    // Right clicking in selected grid area while it is working..
    //

    public class RibbonControlAdvFixed : RibbonControlAdv
    {
        private CallWndProcHook myOwnHook;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
			RemoveSyncfusionHook();
			SetupFixedHook();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
			RemoveFixedHook();
        }

        private void RemoveSyncfusionHook()
        {
            var ribbonType = typeof(RibbonControlAdv);
            var hookField = ribbonType.GetField("m_callWndProcHook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hook = hookField.GetValue(this);
            var hookType = hook.GetType();
            var hookPtrField = hookType.GetField("m_hHook", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hookPtr = (IntPtr) hookPtrField.GetValue(hook);
            hookPtrField.SetValue(hook, IntPtr.Zero);
            WindowsAPI.UnhookWindowsHookEx(hookPtr);
        }

        private void SetupFixedHook()
        {
            myOwnHook = new CallWndProcHook(CallWndProc);
        }

        private void RemoveFixedHook()
        {
            myOwnHook.Dispose();
            myOwnHook = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private IntPtr CallWndProc(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam)
        {
            var type = typeof(RibbonControlAdv);
            if (((Msg)nMsg) == Msg.WM_CONTEXTMENU)
            {
                try
                {
                    var method = type.GetMethod("OnWmContextMenu", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(this, new object[] { hWnd, lParam });
                }
                catch { }
            } 
			else
            {
                var method = type.GetMethod("CallWndProc", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(this, new object[] { hWnd, nMsg, wParam, lParam });
            }
            return IntPtr.Zero;
        }

        internal class CallWndProcHook : WndHook
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="wndProc"></param>
            public CallWndProcHook(WindowsAPI.WindowProc wndProc)
                : base(wndProc)
            {
                if (base.m_wndProc != null)
                {
                    base.m_hHook = WindowsAPI.SetWindowsHookEx(4, base.m_hookProc, IntPtr.Zero, WindowsAPI.GetCurrentThreadId());
                }
            }

            protected override void CallWndProcInternal(int nCode, IntPtr wParam, IntPtr lParam)
            {
                CWPSTRUCT cwpstruct = (CWPSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPSTRUCT));
                base.m_wndProc(cwpstruct.hwnd, cwpstruct.message, cwpstruct.wparam, cwpstruct.lparam);
            }
        }

        internal abstract class WndHook : IDisposable
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
            protected IntPtr m_hHook;
            protected WindowsAPI.HookProc m_hookProc;
            protected WindowsAPI.WindowProc m_wndProc;

            public WndHook(WindowsAPI.WindowProc wndProc)
            {
                this.m_wndProc = wndProc;
                this.m_hookProc = new WindowsAPI.HookProc(this.CallWndProc);
            }

            private IntPtr CallWndProc(int nCode, IntPtr wParam, IntPtr lParam)
            {
                this.CallWndProcInternal(nCode, wParam, lParam);
                return WindowsAPI.CallNextHookEx(this.m_hHook, nCode, wParam, lParam);
            }

            protected abstract void CallWndProcInternal(int nCode, IntPtr wParam, IntPtr lParam);
            /// <summary>
            /// 
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2216:DisposableTypesShouldDeclareFinalizer"), 
            System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
            public void Dispose()
            {
                if (this.m_hHook != IntPtr.Zero)
                {
                    WindowsAPI.UnhookWindowsHookEx(this.m_hHook);
                }
            }
        }

    }
}
