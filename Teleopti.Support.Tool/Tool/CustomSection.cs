using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Code.Tool
{
	public class CustomSection
	{
		public void Read(string filePath, CustomSectionContainer customSectionContainer)
		{
			var config = File.ReadAllLines(filePath);
			var write = false;
			foreach (var line in config)
			{
				if (line.Contains("<!-- custom section ends here -->"))
				{
					write = false;
					continue;
				}
				if (write)
					customSectionContainer.Add(line);
				if (line.Contains("<!-- custom section starts here -->"))
				{
					customSectionContainer.AddSection();
					write = true;
				}
			}
		}

		public void WriteCustomSection(int section, string filePath, IEnumerable<string> towrite)
		{
			var config = File.ReadAllLines(filePath).ToList();
			var sectionRowStart = config.Select((x, i) => new { x, i }).Where(c => c.x.Contains("<!-- custom section starts here -->")).ElementAt(section - 1);
			var sectionRowEnd = config.Select((x, i) => new { x, i }).Where(c => c.x.Contains("<!-- custom section ends here -->")).ElementAt(section - 1);
			var indexRowEnd = sectionRowEnd.i;
			var start = sectionRowStart.i + 1;
			var rowsToRemove = indexRowEnd - start;

			for (int i = indexRowEnd - 1; i >= indexRowEnd - rowsToRemove; i--)
			{
				config.RemoveAt(i);
			}

			config.InsertRange(start, towrite);
			File.WriteAllLines(filePath, config);
		}
	}
}