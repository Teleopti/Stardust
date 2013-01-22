using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class PayrollDllCopyTest
	{
		private string _source;
		private string _destination;
		private string[] _originalDeployFiles;
		private string[] _originalPayrollFiles;
		
		[SetUp]
		public void Setup()
		{
			_source = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\"
			                           + "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll.DeployNew\\");
			_destination = Path.GetFullPath(Environment.CurrentDirectory + "\\..\\" + "\\..\\" + "\\..\\"
			                                + "\\Teleopti.Ccc.Sdk.ServiceBus.Host\\bin\\Debug\\Payroll\\");

			_originalDeployFiles = Directory.GetFiles(_source, "*.*", SearchOption.AllDirectories);
			_originalPayrollFiles = Directory.GetFiles(_destination, "*.*", SearchOption.AllDirectories);

			RemoveAllFiles(_source);
			RemoveAllFiles(_destination);
			
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
			PayrollDllCopy.CopiedFiles.Clear();
		}

		private void RemoveAllFiles(string path)
		{

			foreach (var folder in Directory.GetDirectories(path).Where(f => !f.EndsWith("Teleopti CCC", StringComparison.OrdinalIgnoreCase)))
			{
				foreach (
					var file in
						Directory.GetFiles(folder).Where(file => !_originalDeployFiles.Contains(file)).Where(
							file => !_originalPayrollFiles.Contains(file)))
					File.Delete(file);
				if (!Directory.GetFiles(folder).Any())
					Directory.Delete(folder);
			}
			foreach (var file in Directory.GetFiles(path).Where(file => !_originalDeployFiles.Contains(file)).Where(
				file => !_originalPayrollFiles.Contains(file)))
				File.Delete(file);
		}


		[Test]
		public void ShouldReturnList()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			Assert.That(PayrollDllCopy.CopiedFiles.Count, Is.EqualTo(6 + _originalDeployFiles.Count()));
		}

		[Test]
		public void ShouldCopyXmlAndSettingsFileToPayroll()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			var copiedFilesWithoutOriginal =
				PayrollDllCopy.CopiedFiles.Where(
					file => !_originalPayrollFiles.Contains(file.Key) && !_originalDeployFiles.Contains(file.Key)).ToList();
			var xmlFile = copiedFilesWithoutOriginal.Single(x => x.Key.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).Key;
			var xmlFileInfo = new FileInfo(xmlFile);
			var settingsFile = copiedFilesWithoutOriginal.Single(x => x.Key.EndsWith(".settings", StringComparison.OrdinalIgnoreCase)).Key;
			var settingsFileInfo = new FileInfo(settingsFile);
			Assert.That(File.Exists(Path.GetFullPath(_destination + xmlFileInfo.Name)), Is.True);
			Assert.That(File.Exists(Path.GetFullPath(_destination + settingsFileInfo.Name)), Is.True);
		}

		[Test]
		public void ShouldHaveSameDllFileStructure()
		{
			PayrollDllCopy.CopyPayrollDllTest(_source, _destination);
			foreach (var path in PayrollDllCopy.CopiedFiles
				.Where(x => x.Key.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				.Select(k => k.Key)
				.Select(file => new FileInfo(file))
				.Select(fileInfo => fileInfo.Directory.Name + "\\" + fileInfo.Name))
				Assert.That(File.Exists(Path.GetFullPath(_destination + path)), Is.True);
		}
	}
}
