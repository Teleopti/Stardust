using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture, Ignore]
	public class PayrollDllCopyTest
	{
		private string _source;
		private string _destination;
		private List<string> _originalFiles;
		
		[SetUp]
		public void Setup()
		{
			_source = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\"
			                           + "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll.DeployNew\\");
			_destination = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\"
			                                + "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll\\");
			_originalFiles = new List<string>();
			_originalFiles.AddRange(Directory.GetFiles(_source, "*.*", SearchOption.AllDirectories));
			_originalFiles.AddRange(Directory.GetFiles(_destination, "*.*", SearchOption.AllDirectories));
			
			Teardown();
			
			for (var i = 0; i <= 1; i++)
			{
				var folder = Guid.NewGuid();
				var folderPath = Path.GetFullPath(_source + folder);
				Directory.CreateDirectory(folderPath);

				for (var j = 0; j <= 2; j++)
				{
					var file = Guid.NewGuid();
					var filePath = Path.GetFullPath(folderPath + "\\" + file);
					if (j == 1 && i == 1)
						File.Create(filePath + ".xml").Dispose();
					else if (j == 2 && i == 1)
						File.Create(filePath + ".settings").Dispose();
					else
						File.Create(filePath + ".dll").Dispose();
				}
			}
		}

		[TearDown]
		public void Teardown()
		{
			RemoveAllFiles(_source);
			RemoveAllFiles(_destination);
		}

		private void RemoveAllFiles(string path)
		{
			foreach (var folder in Directory.GetDirectories(path).Where(f => !_originalFiles.Contains(f)))
			{
				foreach (
					var file in
						Directory.GetFiles(folder).Where(f => !_originalFiles.Contains(f)))
					File.Delete(file);
				if (!Directory.GetFiles(folder).Any())
					Directory.Delete(folder);
			}
			foreach (var file in Directory.GetFiles(path).Where(f => !_originalFiles.Contains(f)))
				File.Delete(file);
		}

		private IEnumerable<string> CopiedFiles()
		{
			var copiedFiles = new List<string>();
			var filesInTopFolder = Directory.GetFiles(_destination)
				.Where(f => !_originalFiles.Contains(f) && !f.EndsWith("Teleopti.Ccc.Payroll.dll", StringComparison.OrdinalIgnoreCase))
				.ToList();
			if (filesInTopFolder.Any())
				copiedFiles.AddRange(filesInTopFolder);

			copiedFiles.AddRange(
				Directory.GetDirectories(_destination)
				.SelectMany(folder => Directory.GetFiles(folder)
					.Where(f => !_originalFiles.Contains(f))));

			return copiedFiles;
		}

		[Test]
		public void ShouldCopyXmlAndSettingsFileToPayroll()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			var xmlFile = CopiedFiles().Single(x => x.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
			var xmlFileInfo = new FileInfo(xmlFile);
			var settingsFile = CopiedFiles().Single(x => x.EndsWith(".settings", StringComparison.OrdinalIgnoreCase));
			var settingsFileInfo = new FileInfo(settingsFile);
			Assert.That(File.Exists(Path.GetFullPath(_destination + xmlFileInfo.Name)), Is.True);
			Assert.That(File.Exists(Path.GetFullPath(_destination + settingsFileInfo.Name)), Is.True);
		}

		[Test]
		public void ShouldHaveSameDllFileStructure()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			foreach (var path in CopiedFiles()
				.Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				.Select(file => new FileInfo(file))
				.Select(fileInfo => fileInfo.Directory != null ? fileInfo.Directory.Name + "\\" + fileInfo.Name : null))
				Assert.That(File.Exists(Path.GetFullPath(_destination + path)), Is.True);
		}

		[Test, ExpectedException(typeof(DirectoryNotFoundException))]
		public void ShouldThrowException()
		{
			// path gets messed up when running from testproj
			PayrollDllCopy.CopyPayrollDll();
		}

		[Test, ExpectedException(typeof(DirectoryNotFoundException))]
		public void ShouldThrowExceptionWithWrongPath()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source + Guid.NewGuid(), _destination + Guid.NewGuid());
		}

		[Test]
		public void ShouldSkipLockedFile()
		{
			var guid = Guid.NewGuid() + ".dll";
			var file = File.Create(_source + guid);
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			file.Dispose();
			Assert.That(!File.Exists(_destination + guid));
		}
	}
}
