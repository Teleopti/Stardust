using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture, Ignore("WIP")]
	public class PayrollDllCopyTest
	{
		private string _source;
		private string _destination;
		private IList<string> _createdTestFiles;
		private IList<string> _createdFolders;

		[SetUp]
		public void Setup()
		{
			_source = Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll.DeployNew\\");
			if (!Directory.Exists(_source))
			{
				Directory.CreateDirectory(_source);
			}

			_destination = Path.GetFullPath(Environment.CurrentDirectory + "\\Payroll\\");
			if (!Directory.Exists(_destination))
			{
				Directory.CreateDirectory(_destination);
			}

			createTestFilesToCopy();
		}

		private void createTestFilesToCopy()
		{
			_createdFolders = new[]
			{
				Path.GetFullPath(_source + Guid.NewGuid()),
				Path.GetFullPath(_source + Guid.NewGuid())
			};
			_createdFolders.ForEach(f => Directory.CreateDirectory(f));

			_createdTestFiles = new[]
			{
				Path.GetFullPath(_createdFolders[0] + "\\" + Guid.NewGuid() + ".dll"),
				Path.GetFullPath(_createdFolders[0] + "\\" + Guid.NewGuid() + ".dll"),
				Path.GetFullPath(_createdFolders[0] + "\\" + Guid.NewGuid() + ".dll"),
				Path.GetFullPath(_createdFolders[1] + "\\" + Guid.NewGuid() + ".xml"),
				Path.GetFullPath(_createdFolders[1] + "\\" + Guid.NewGuid() + ".dll"),
				Path.GetFullPath(_createdFolders[1] + "\\" + Guid.NewGuid() + ".settings")
			};

			_createdTestFiles.ForEach(f => File.Create(f).Dispose());
		}

		private ISearchPath createStubbedSearchPath()
		{
			var searchPath = MockRepository.GenerateStub<ISearchPath>();
			searchPath.Stub(x => x.PayrollDeployNewPath).Return(_source);
			searchPath.Stub(x => x.Path).Return(_destination);
			return searchPath;
		}

		[Test]
		public void ShouldCopyXmlAndSettingsFileToPayroll()
		{
			var xmlFileInfo = getSourceFileEndingWith(".xml");
			var settingsFileInfo = getSourceFileEndingWith(".settings");

			var target = new PayrollDllCopy(createStubbedSearchPath());

			target.CopyPayrollDll();
			
			Assert.IsTrue(File.Exists(Path.GetFullPath(_destination + xmlFileInfo)));
			Assert.IsTrue(File.Exists(Path.GetFullPath(_destination + settingsFileInfo)));
		}

		private string getSourceFileEndingWith(string fileEnding)
		{
			return new FileInfo(_createdTestFiles.Single(x => x.EndsWith(fileEnding, StringComparison.OrdinalIgnoreCase))).Name;
		}

		[Test]
		public void ShouldHaveSameDllFileStructure()
		{
			var createdDllFiles = getDllFilesWithFolderStructure();
			var target = new PayrollDllCopy(createStubbedSearchPath());

			target.CopyPayrollDll();
			
			Assert.IsNotEmpty(createdDllFiles);
			foreach (var path in createdDllFiles)
			{
				Assert.IsTrue(File.Exists(Path.GetFullPath(_destination + path)));
			}
		}

		private IList<string> getDllFilesWithFolderStructure()
		{
			return _createdTestFiles
				.Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				.Select(file => new FileInfo(file))
				.Select(fileInfo => fileInfo.Directory.Name + "\\" + fileInfo.Name)
				.ToList();
		}

		[Test]
		public void ShouldSkipLockedFile()
		{
			var fileName = Guid.NewGuid() + ".dll";
			var filePath = _source + fileName;
			var file = File.Create(filePath);
			var target = new PayrollDllCopy(createStubbedSearchPath());

			target.CopyPayrollDll();
			
			Assert.That(!File.Exists(_destination + fileName));

			file.Dispose();
			File.Delete(filePath);
		}

		[Test]
		[Ignore("Not used right now. WIP Container")]
		public void CopyDllsFromAzureStorage()
		{
			var fakeSettingValues = new Dictionary<string, string>
			{
				{"AzureStorageContainer", "installations"},
				{"AzureStoragePayrollPath", "i-devtest/payroll"}
			};

			var fakeConfigReader = new FakeConfigReader(fakeSettingValues);

			fakeConfigReader.FakeConnectionString("AzureStorage",
				"DefaultEndpointsProtocol=https;AccountName=payrolltests;AccountKey=uR8uzi2XR6EEjZePYC/W0qYAcafovsVfHFvJrk6a2TjFMPCbmsiOsN0LiysoSAAcZWcQyf7MXfg0s4ThMOk3Mw==;EndpointSuffix=core.windows.net");
			
			IConfigReader configReader = new ConfigOverrider(fakeConfigReader, new Dictionary<string, string>());

			var target = new PayrollDllCopy(createStubbedSearchPath(), configReader);
			target.CopyPayrollDllFromAzureStorage("Teleopti WFM");

			Assert.IsTrue(File.Exists(Path.GetFullPath($"{_destination}Teleopti.Ccc.Payroll.dll")));
		}

		[TearDown]
		public void Teardown()
		{
			foreach (var createdFile in _createdTestFiles)
			{
				File.Delete(createdFile);
			}
			foreach (var createdFolder in _createdFolders)
			{
				Directory.Delete(createdFolder);
			}

			deleteCopiedFiles();
		}

		private void deleteCopiedFiles()
		{
			foreach (var folder in Directory.GetDirectories(_destination))
			{
				foreach (var file in Directory.GetFiles(folder))
				{
					File.Delete(file);
				}
				if (!Directory.GetFiles(folder).Any())
				{
					Directory.Delete(folder);
				}
			}
			foreach (var file in Directory.GetFiles(_destination))
			{
				File.Delete(file);
			}
		}
	}
}
