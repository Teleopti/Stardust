using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Teleopti.Ccc.TestCommon
{
    internal class IniFileHelper
    {
        public string _path = string.Empty;

        public IniFileHelper(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string ReadIniValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
        	var file = new FileInfo(_path);
			int retLength = NativeMethods.GetPrivateProfileString(Section, Key, "", temp, 255, file.FullName);
            if (retLength == 0)
                return string.Empty; //skjuta fel här?
            else
                return temp.ToString();
        }

        /// <summary>
        /// Internal class called NativeMetods
        /// </summary>
        internal static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            [return : MarshalAs(UnmanagedType.I4)]
            internal static extern int GetPrivateProfileString(string section,
                                                               string key, string def, StringBuilder retVal,
                                                               int size, string filePath);
        }
    }
}