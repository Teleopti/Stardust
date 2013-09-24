﻿using System;
using System.IO;
using NUnit.Framework;
using Teleopti.Support.Code.Tool;
using System.Xml.Linq;

namespace Teleopti.Support.CodeTest.Tool
{
    [TestFixture]
    public class MachineKeyCheckerTests
    {
        [Test]
        public void ShouldOnlyProcessWebConfigFile()
        {
            var target = new MachineKeyChecker();
            Assert.That(target.CheckForMachineKey("c:\app.config"), Is.False);
        }

        [Test]
        public void ShouldInsertMachineKey()
        {
            var xDocument = new XDocument();
            var xElement = new XElement("system.web");
            xDocument.Add(xElement);
            xDocument.Save("web.config");
            var target = new MachineKeyChecker();
            Assert.That(target.CheckForMachineKey("web.config"), Is.True);
        }

        [Test]
        public void ShouldNotInsertIfNoSystemWeb()
        {
            var xDocument = new XDocument();
            var xElement = new XElement("ingensystem.web");
            xDocument.Add(xElement);
            xDocument.Save("web.config");
            var target = new MachineKeyChecker();
            Assert.That(target.CheckForMachineKey("web.config"), Is.False);
        }

        [Test]
        public void ShouldNotInsertIfInAreas()
        {
            var xDocument = new XDocument();
            var xElement = new XElement("system.web");
            xDocument.Add(xElement);
            if (!Directory.Exists("Areas"))
                Directory.CreateDirectory("Areas");
            xDocument.Save(@"Areas\web.config");
            var target = new MachineKeyChecker();
            var path = AppDomain.CurrentDomain.BaseDirectory + @"\Areas\web.config";
            Assert.That(target.CheckForMachineKey(path), Is.False);
        }
    }
}
