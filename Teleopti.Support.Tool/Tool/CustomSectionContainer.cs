using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Teleopti.Support.Code.Tool
{
	public class CustomSectionContainer
	{
		private int currentSection;
		private readonly ICollection<string> content = new Collection<string>();

		public void Add(string row)
		{
			content.Add(row);
		}

		public void AddSection()
		{
			currentSection++;
			content.Add(currentSection.ToString(CultureInfo.InvariantCulture));
		}

		public void WriteToFile(string path)
		{
			File.WriteAllLines(path,content);
		}
	}
}