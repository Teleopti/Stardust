using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WpfControls;
using System.Windows.Input;
using System.Windows;
using Rhino.Mocks;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class CommandRouterTest
    {
        private MockRepository mocks;


        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }


        [Test]
        public void CanSetTarget()
        {
            IInputElement element = mocks.CreateMock<IInputElement>();
            CommandRouter.Target = element;
            Assert.AreSame(element, CommandRouter.Target);
            
        }


        [Test]
        public void CanAddCommandToToolStripItem()
        {
            ToolStripButton btn = mocks.PartialMock<ToolStripButton>();
            Assert.IsNull(CommandRouter.GetCommand(btn));
            CommandRouter.SetCommand(btn, ApplicationCommands.Close);
            Assert.AreSame(CommandRouter.GetCommand(btn), ApplicationCommands.Close);
        }


       
	
    }

   
}
