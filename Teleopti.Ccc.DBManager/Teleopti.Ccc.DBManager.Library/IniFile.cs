using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Teleopti.Ccc.DBManager.Library
{
    public class IniFile
    {
        private uint MAX_BUFFER = 32767;
		private string _path;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer,
           uint nSize, string lpFileName);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern uint GetPrivateProfileSection(string lpAppName,
           IntPtr lpReturnedString, uint nSize, string lpFileName);



        /// <summary>
        /// Gets the section values.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-11-12
        /// </remarks>
        public Dictionary<string, string> GetSectionValues(string section)
        {
            Dictionary<string, string> stringDictionary = new Dictionary<string, string>();
            string[] values;
            string[] separator = new string[]{"="};
            if (GetPrivateProfileSection(section, out values))
            {
                foreach (string s in values)
                {
                    string[] value = s.Split(separator, StringSplitOptions.None);
                    if (value.Length == 2)
                    {
                        stringDictionary.Add(value[0], value[1]);
                    }
                    else
                    {
                        // Throw some error.
                    }
                }
                return stringDictionary;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFile"/> class.
        /// </summary>
        /// <param name="path">Path to the ini file.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-19
        /// </remarks>
        public IniFile(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Sections the names.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-11-12
        /// </remarks>
        public string[] SectionNames()
        {
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);
            uint bytesReturned = GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, _path);
            if (bytesReturned == 0)
                return null;
            string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
            Marshal.FreeCoTaskMem(pReturnedString);
            //use of Substring below removes terminating null for split
            return local.Substring(0, local.Length - 1).Split('\0');
        }

    
        /// <summary>
        /// Gets the private profile section.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="section">The section.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-11-12
        /// </remarks>
        public bool GetPrivateProfileSection(string sectionName, out string[] section)
        {
            section = null;
            if (!System.IO.File.Exists(_path))
                return false;
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER);
            uint bytesReturned = GetPrivateProfileSection(sectionName, pReturnedString, MAX_BUFFER, _path);
            if ((bytesReturned == MAX_BUFFER - 2) ||(bytesReturned == 0))
            {
                Marshal.FreeCoTaskMem(pReturnedString);
                return false;
            }

            string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int) (bytesReturned - 1));
            section = returnedString.Split('\0');
            Marshal.FreeCoTaskMem(pReturnedString);
            return true;
        }
    
    }
}
