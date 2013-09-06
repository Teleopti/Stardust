using System;

namespace Teleopti.Support.Code.Tool
{
    public class RefreshConfigFile
    {
        public void ReplaceFile(string oldFilePath, string newFilePath)
        {
            //System.IO.File.Delete(oldFilePath);
            System.IO.File.Copy(newFilePath, oldFilePath,true);
        }

        public void SplitAndReplace(string oldAndNewFile)
        {
            var files = oldAndNewFile.Split(',');
            if (files.GetUpperBound(0).Equals(1))
                ReplaceFile(files[0], files[1]);
        }

        public void ReadLinesFromString(string allFilesToRefresh)
        {
            foreach (var myString in allFilesToRefresh.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                SplitAndReplace(myString);
        }
    }
}