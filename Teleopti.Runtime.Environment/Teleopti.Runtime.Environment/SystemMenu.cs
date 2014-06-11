using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Teleopti.Runtime.Environment
{
    public class SystemMenu
    {
        [DllImport("USER32", EntryPoint = "GetSystemMenu", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr apiGetSystemMenu(IntPtr windowHandle, int bReset);

        [DllImport("USER32", EntryPoint = "SetMenuItemInfoW", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int setMenuItemInfo(IntPtr menuHandle, int itemId, bool byPosition, ref MenuItemInfo lpmii);

        [DllImport("USER32", EntryPoint = "GetMenuItemInfoW", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern bool getMenuItemInfo(IntPtr menuHandle, int itemId, bool fByPosition, ref MenuItemInfo lpmii);

        [DllImport("USER32", EntryPoint = "InsertMenuW", SetLastError = true,
            CharSet = CharSet.Unicode, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern int apiInsertMenu(IntPtr menuHandle, int position, int flags, int newId, String item);

        private IntPtr _sysMenu = IntPtr.Zero;

        public bool InsertSeparator(int position)
        {
            return (InsertMenu(position, ItemFlags.mfSeparator | ItemFlags.mfByPosition, 0, ""));
        }

        public bool InsertMenu(int position, ItemFlags flags, int newId, String item)
        {
            return (apiInsertMenu(_sysMenu, position, (Int32) flags, newId, item) == 0);
        }

        public static SystemMenu FromForm(Form form)
        {
            SystemMenu cSysMenu = new SystemMenu();
            cSysMenu._sysMenu = apiGetSystemMenu(form.Handle, 0);
            if (cSysMenu._sysMenu == IntPtr.Zero)
            {
                throw new NoSystemMenuException();
            }
            return cSysMenu;
        }

        public void ToggleMenuItem(int menuItem, bool itemChecked)
        {
            var info = new MenuItemInfo();
            info.fMask = 0x00000001;
            info.cbSize = (uint) Marshal.SizeOf(typeof (MenuItemInfo));

            if (!getMenuItemInfo(_sysMenu, menuItem, false, ref info))
            {
                ShowLastError();
            }
            info.fState = (uint) (itemChecked ? ItemFlags.mfChecked : ItemFlags.mfUnchecked);
            setMenuItemInfo(_sysMenu, menuItem, false, ref info);
        }

        private void ShowLastError()
        {
            var ex = new Win32Exception();
            MessageBox.Show(ex.Message, "Last Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}