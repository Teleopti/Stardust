using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ReplaceString
{
    class Program
    {                          
        const string teleopti1 = "load.php@debug=false&amp;lang=en&amp;modules=mediawiki.legacy.commonPrint,shared%257Cskins.vector&amp;only=styles&amp;printable=1&amp;skin=vector&amp;%252A";
        const string teleopti2 = "load.php@debug=false&amp;lang=en&amp;modules=mediawiki.legacy.commonPrint,shared%257Cskins.vector&amp;only=styles&amp;skin=vector&amp;%252A";
        const string teleopti3 = "load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";
        const string teleopti4 = "load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=scripts&amp;skin=vector&amp;%252A";
        const string teleopti5 = "load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=styles&amp;printable=1&amp;skin=vector&amp;%252A";
        const string teleopti6 = "load.php@debug=false&amp;lang=en&amp;modules=site&amp;only=styles&amp;skin=vector&amp;%252A";
        const string teleopti7 = "load.php@debug=false&amp;lang=en&amp;modules=skins.vector&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";
        const string teleopti8 = "load.php@debug=false&amp;lang=en&amp;modules=skins.vector&amp;only=scripts&amp;skin=vector&amp;%252A";
        const string teleopti9 = "load.php@debug=false&amp;lang=en&amp;modules=startup&amp;only=scripts&amp;printable=1&amp;skin=vector&amp;%252A";
        const string teleopti10 = "load.php@debug=false&amp;lang=en&amp;modules=startup&amp;only=scripts&amp;skin=vector&amp;%252A";

        static void Main(string[] args)
        {
            // get list of files
            string path = args[0];
            string[] regExps = { "F01%253A[A-Za-z0-9]*\\+[A-Za-z0-9]*", "F02%253A[A-Za-z0-9]*\\+[A-Za-z0-9]*"};
            string[] files = Directory.GetFiles(path);
            string[] directories = Directory.GetDirectories(path);

            //iterate each regular exp in each file
            foreach (var text in regExps)
            {
                foreach (var file in files)
                {
                    ReplaceInFile(file, text);
                }
            }

            // replace file name if it contains + sign
            foreach (var file in files)
            {
                if (file.Contains("+"))
                {
                    var newfile = file.Replace("+", "_");
                    File.Move(file, newfile);
                }

                //need to do replace load script file with teleopti names.
                if (file.Contains("load.php"))
                {
                    var index = file.LastIndexOf("\\");

                    if (index != -1)
                    {
                        var fileName = file.Substring(index + 1, file.Length - index - 1);

                        if (fileName =="load.php@debug=false&lang=en&modules=mediawiki.legacy.commonPrint,shared%7Cskins.vector&only=styles&printable=1&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti1.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=mediawiki.legacy.commonPrint,shared%7Cskins.vector&only=styles&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti2.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=site&only=scripts&printable=1&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti3.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=site&only=scripts&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti4.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=site&only=styles&printable=1&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti5.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=site&only=styles&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti6.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=skins.vector&only=scripts&printable=1&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti7.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=skins.vector&only=scripts&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti8.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=startup&only=scripts&printable=1&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti9.css");
                        else if (fileName == "load.php@debug=false&lang=en&modules=startup&only=scripts&skin=vector&%2A")
                            File.Move(file, path + "\\" + "teleopti10.css");
                    }
                }
            }

            // replace + with _ and load php script in translation files also
            foreach (var text in regExps)
            {
                foreach (var directory in directories)
                {
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        ReplaceInFile(file, text);
                    }
                }
            }

            // rename directory name if it contains + sign
            foreach (var directory in directories)
            {
                if (directory.Contains("+"))
                {
                    var newDir = directory.Replace("+", "_");
                    Directory.Move(directory, newDir);
                }
            }

        }

        public static void ReplaceInFile(string filePath, string searchText)
        {

            var content = string.Empty;

            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
                reader.Close();
            }

            string replaceStr = string.Empty;

            if (content.Contains("load.php"))
            {
                content = content.Replace(teleopti1, "teleopti1.css");
                content = content.Replace(teleopti2, "teleopti2.css");
                content = content.Replace(teleopti3, "teleopti3.css");
                content = content.Replace(teleopti4, "teleopti4.css");
                content = content.Replace(teleopti5, "teleopti5.css");
                content = content.Replace(teleopti6, "teleopti6.css");
                content = content.Replace(teleopti7, "teleopti7.css");
                content = content.Replace(teleopti8, "teleopti8.css");
                content = content.Replace(teleopti9, "teleopti9.css");
                content = content.Replace(teleopti10, "teleopti10.css");
            }


            Regex regExp = new Regex(searchText);
            foreach (Match match in regExp.Matches(content))
            {
                string matchStr = match.ToString();
                replaceStr = matchStr.Replace("+", "_");
                content = Regex.Replace(content, matchStr.Replace("+", "\\+"), replaceStr);    
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(content);
                writer.Close();
            }
        }
    }
}
