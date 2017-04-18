using System.Windows;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    //Tests must run on STA because the commandbindings are implemented on FrameworkElement (and higher, wich is a GUI-element)
    [TestFixture]
    public class CreateCommandBindingTest
    {
        private CommandModel _commandModel1;
        private CommandModel _commandModel2;
        private CommandModel _commandModel3;
        private List<CommandModel> _commands;
        private int _executed;

        [SetUp]
        public void Setup()
        {
            _commandModel1 = CommandModelFactory.CreateCommandModel(executeCommand, "for test");
            _commandModel2 = CommandModelFactory.CreateCommandModel(delegate() { }, "for test");
            _commandModel3 = CommandModelFactory.CreateCommandModel(delegate() { }, "for test");
            _commands = new List<CommandModel>() { _commandModel1, _commandModel2, _commandModel3 };
        }

        private void executeCommand()
        {
            _executed++;
        }


        [Test, Apartment(ApartmentState.STA)]
        public void VerifyCanAddMultipleCommandModelsToAnElement()
        {
            var element = new FrameworkElement(); //just something that can hold commandbindings
            CreateCommandBinding.SetCommands(element, _commands);
            Assert.AreEqual(element.CommandBindings.Count, _commands.Count);
            Assert.AreEqual(CreateCommandBinding.GetCommands(element), _commands);

        }

		[Test, Apartment(ApartmentState.STA)]
		public void VerifyOnlyAddsUniqueCommands()
        {
            var element = new FrameworkElement(); //just something that can hold commandbindings

            //Because its a dependencyproperty, we have to set it to another object, otherwise it will not invalidate 
            var anotherCommandList = new List<CommandModel>() { _commandModel1, _commandModel2 };

            //Setting the commandbindings with a list containing two commandmodels
            CreateCommandBinding.SetCommands(element, anotherCommandList);
            Assert.AreEqual(element.CommandBindings.Count, anotherCommandList.Count);

            //Setting with a list containing the same two and another one
            CreateCommandBinding.SetCommands(element, _commands);
            Assert.AreEqual(element.CommandBindings.Count, 3, "The two original ones will still be there, and the new one");


            anotherCommandList.Add(CommandModelFactory.CreateCommandModel(delegate() { }, "for test"));
            CreateCommandBinding.SetCommands(element, anotherCommandList);
            Assert.AreEqual(element.CommandBindings.Count, 4, "the new command should have been added to the commandbindings");



        }

        #region bindabletarget
       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test, Apartment(ApartmentState.STA)]
        public void VerifyThatAllExposedCommandModelsGetsRegistered()
        {
            ViewModelExposingCommands viewModelExposingCommands = new ViewModelExposingCommands();
            var element = new FrameworkElement();
            CreateCommandBinding.SetTarget(element, viewModelExposingCommands);
            Assert.AreEqual(2, element.CommandBindings.Count);

            CreateCommandBinding.SetTarget(element, viewModelExposingCommands);
            Assert.AreEqual(2, element.CommandBindings.Count,"Should not have added any new commandbindings");

            //Henrik 20100603 Check that we can add multiple models. I'm not really sure that this is the right thing to do
            string viewModelNotExposingAnyCommands = "this is just a string";
            CreateCommandBinding.SetTarget(element,viewModelNotExposingAnyCommands);
            Assert.AreEqual(2,element.CommandBindings.Count, "Keep the existing commandbindings");

        }


        internal class ViewModelExposingCommands
        {
            
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public CommandModel Command1 { get; set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public CommandModel Command2 { get; set; }

            public ViewModelExposingCommands()
            {
                Command1 = CommandModelFactory.CreateCommandModel(() => { }, "doStuff");
                Command2 = CommandModelFactory.CreateCommandModel(() => { }, "doStuff");
            }
        }
        #endregion

    }
}
