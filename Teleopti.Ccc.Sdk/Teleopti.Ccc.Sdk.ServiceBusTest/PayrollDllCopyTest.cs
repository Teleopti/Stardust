using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class PayrollDllCopyTest
	{
		private Dictionary<string, List<string>> _tempFoldersAndFiles;
		private MockRepository _mock;
		private ISearchPath _searchPath;
		
		[SetUp]
		public void Setup()
		{
			 _mock = new MockRepository();
			_searchPath = _mock.StrictMock<ISearchPath>();
			_tempFoldersAndFiles = new Dictionary<string, List<string>>();
			var source = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\" 
				+ "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll.DeployNew\\");

			for (var i = 0; i <= 2; i++)
			{
				var folder = Guid.NewGuid();
				var folderPath = Path.GetFullPath(source + folder);
				Directory.CreateDirectory(folderPath);
				_tempFoldersAndFiles.Add(folderPath, new List<string>());

				for (var j = 0; j <= 3; j++)
				{
					var file = Guid.NewGuid();
					var filePath = Path.GetFullPath(folderPath + "\\" + file);
					if (j == 3 && i == 2)
					{
						File.Create(filePath + ".xml");
						File.Create(filePath + ".settings");
					}
					else
						File.Create(filePath + ".dll");
					_tempFoldersAndFiles[folderPath].Add(filePath);
				}
			}
		}
		
		public void Teardown()
		{
			foreach (var folder in _tempFoldersAndFiles)
			{
				foreach (var files in folder.Value)
				{
					File.Delete(files);
				}
				Directory.Delete(folder.Key);
			}
			_tempFoldersAndFiles.Clear();
		}

		[Test]
		public void ShouldReturnList()
		{
			_searchPath.Expect(x => x.Path).IgnoreArguments().Return(Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\"
				                 + "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll\\"));

			PayrollDllCopy.CopyPayrollDll();
			Assert.That(PayrollDllCopy.CopiedFiles.Count, Is.EqualTo(7));

			Teardown();
		}

	}
}
