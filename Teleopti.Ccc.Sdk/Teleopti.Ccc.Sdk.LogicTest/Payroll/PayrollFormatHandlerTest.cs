using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Payroll;

namespace Teleopti.Ccc.Sdk.LogicTest.Payroll
{
    //INTEGRATION TESTS FOR SAVING PAYROLL FORMATS
    [TestFixture]
    public class PayrollFormatHandlerTest
    {
        private PayrollFormatHandler _formatHandler;
        private string path;
        private string completePath;
        private const string esentPath = "one_way.esent";
        private const string fileName = "internal.storage.xml";

        [SetUp]
        public void Setup()
        {
            path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            _formatHandler = new PayrollFormatHandler(path);
            completePath = path + "\\" + esentPath + "\\" + fileName;
        }

        [Test]
        public void ShouldCreateXmlPayrollFormatFile()
        {
            completePath = CleanDefaults(); //Checks that esent dir exists and deletes old file
            var payrollFormatDtos = new List<PayrollFormatDto> { new PayrollFormatDto(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), "Sweet Päjrållformat") };
            
            _formatHandler.Save(payrollFormatDtos);
            var bytes = File.ReadAllBytes(completePath);
            const int byteLength = 156;
            Assert.AreEqual(byteLength, bytes.Length);
        }

        [Test]
        public void ShouldReadPayrollFormatsFromXmlFile()
        {
            var payrollFormatDtos = new List<PayrollFormatDto> { new PayrollFormatDto(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), "Sweet Päjrållformat") };
            //First Save a file
            _formatHandler = new PayrollFormatHandler(path);
            _formatHandler.Save(payrollFormatDtos);

            var formats =  _formatHandler.Load();
            Assert.AreEqual("Sweet Päjrållformat", formats.First().Name);
            Assert.AreEqual(new Guid("1E88583F-284C-4C17-BD07-B7AFA08D0BF4"), formats.First().FormatId);
        }

        [Test]
        public void ShouldNotCrashIfFileNotExist()
        {
            CleanDefaults();
            var formats = _formatHandler.Load();
            Assert.IsNotNull(formats);
        }

        private string CleanDefaults()
        {
            var directoryPath = path + "\\" + esentPath;
            
            if (File.Exists(completePath))
            {
                File.Delete(completePath);
            }
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            return completePath;
        }

    }
}
