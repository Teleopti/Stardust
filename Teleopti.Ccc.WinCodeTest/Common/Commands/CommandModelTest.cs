using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    //Tests CreateComamndBinding and CommandModel
    [TestFixture]
    public class CommandModelTest
    {
        private CommandModelForTest _target;
        private TestClass _commandable;
        private string _text = "test";
        private CrossThreadTestRunner testRunner;
   

        [SetUp]
        public void Setup()
        {
            //Moved erything down to methods for STA, fix
            testRunner = new CrossThreadTestRunner();
        }

      

        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            testRunner.RunInSTA(
                delegate
                {
                    _target = new CommandModelForTest(_text);
                        _commandable = new TestClass();
                        CreateCommandBinding.SetCommand(_commandable, _target);
                        _commandable.TriggerLoad();
                        Assert.IsNotNull(_target.Command);
                        Assert.AreEqual(_text, _target.Text);
                    });
        }


        [Test]
        public void VerifyCanAttachCommandModel()
        {
            testRunner.RunInSTA(
                delegate
                {
                    _target = new CommandModelForTest(_text);
                         _commandable = new TestClass();
                         CreateCommandBinding.SetCommand(_commandable, _target);
                        _commandable.TriggerLoad();
                        CommandModel anotherModel = new CommandModelForTest("a");
                        _commandable.TriggerLoad();
                        Assert.AreEqual(1, _commandable.CommandBindings.Count);
                        Assert.AreSame(_target.Command, _commandable.CommandBindings[0].Command);
                        CreateCommandBinding.SetCommand(_commandable, anotherModel);
                        _commandable.TriggerLoad();
                        Assert.AreEqual(2, _commandable.CommandBindings.Count);
                        Assert.AreEqual(anotherModel, CreateCommandBinding.GetCommand(_commandable)); //Verify get
                    });
        }

        [Test]
        public void VerifyCanAddTheSameCommandTwice()
        {
            testRunner.RunInSTA(
                delegate
                {
                   
                    _target = new CommandModelForTest(_text);
                    var target2 = new CommandModelForTest(_target.Command);
                    _commandable = new TestClass();
                    CreateCommandBinding.SetCommand(_commandable, _target);
                    CreateCommandBinding.SetCommand(_commandable, target2); //Adding the same command twice should remove the first instance
                    _commandable.TriggerLoad();
                        int numberOfBindings = _commandable.CommandBindings.Count;
                        CommandModelForTest modelWithApplicationCommand =
                            new CommandModelForTest(ApplicationCommands.Paste);
                        CreateCommandBinding.SetCommand(_commandable, modelWithApplicationCommand);
                        CreateCommandBinding.SetCommand(_commandable, modelWithApplicationCommand);
                        //Adding a singelton command twice should add one binding
                        _commandable.TriggerLoad();
                        Assert.AreEqual(numberOfBindings + 1, _commandable.CommandBindings.Count);
                    });
        }

        [Test]
        public void VerifyExecuteCanExecuteIsRoutedToModel()
        {
            testRunner.RunInSTA(
                delegate
                {
                    _target = new CommandModelForTest(_text);
                    _commandable = new TestClass();
                    CreateCommandBinding.SetCommand(_commandable, _target);
                    _commandable.TriggerLoad();
                        Assert.IsTrue(_target.Command.CanExecute(null, _commandable),"default is true"); // default is true;
                        _target.Command.Execute(null, _commandable);
                        Assert.AreEqual(1, _target.HasExecuted);
                        _target.ModelCanExecute = false;
                        Assert.IsFalse(_target.Command.CanExecute(null, _commandable));
                    });
        }

        [Test]
        public void VerifyDescriptionTextReturnsTextByDefault()
        {
            string targetText = "targetText";
            _target = new CommandModelForTest(targetText);
            Assert.AreEqual(targetText, _target.Text);
            Assert.AreEqual(targetText, _target.DescriptionText);
        }


        [Test]
        public void VerifySetCommandModel()
        {
            //Should set the content/tooltip/command
            testRunner.RunInSTA(
                delegate
                {
                    _target = new CommandModelForTestWithDescription(ApplicationCommands.Close);
                    _commandable = new TestClass();
                    CreateCommandBinding.SetCommandModel(_commandable, _target);
                    Assert.AreEqual(_commandable.ToolTip,_target.DescriptionText);
                    Assert.AreEqual(_commandable.Content,_target.Text);
                    Assert.AreSame(_target.Command, _commandable.CommandBindings[0].Command);
                });
        }

        [Test]
        public void VerifyExecuteCommandModelOnLoad()
        {
            testRunner.RunInSTA(
                delegate
                {
                    _target = new CommandModelForTestWithDescription(ApplicationCommands.Close);
                    _commandable = new TestClass();
                    CreateCommandBinding.SetExecuteCommandModel(_commandable, _target);
                    
                    Assert.AreEqual(0,_target.HasExecuted,"The command should not have beeen fired");
                   
                    _commandable.TriggerLoad();
                    Assert.AreEqual(1, _target.HasExecuted, "The command should not beeen fired once (we have simulated load-event)");
                  
                    _commandable.TriggerLoad();
                    Assert.AreEqual(1, _target.HasExecuted, "The command should only have been fired once, (it should have been removed from the loadevent)");

                    CreateCommandBinding.SetLoadOnlyOnce(_commandable, false);
                    _commandable.TriggerLoad();
                    Assert.AreEqual(2, _target.HasExecuted, "The command should now  have been fired again");
                    CreateCommandBinding.SetLoadOnlyOnce(_commandable, true);
                    _commandable.TriggerLoad(); //Should fire and remove
                    _commandable.TriggerLoad();
                    _commandable.TriggerLoad();
                   
                    Assert.AreEqual(4, _target.HasExecuted, "The command should now  have been fired again, and then removed from loadevent"); //Todo fix this, I think it should be 3

                });
        }

     
        

        //need a derived commandable to be able to call load and trigger the createCommand
        private class TestClass : Button
        {
            internal void TriggerLoad()
            {
                RaiseEvent(new RoutedEventArgs(LoadedEvent));
            }
        }

        internal class CommandModelForTest : CommandModel
        {
            internal string CommandText;
            internal int HasExecuted;
            internal bool ModelCanExecute = true;
            

            internal CommandModelForTest(RoutedUICommand command)
                : base(command)
            {

            }
            
            internal CommandModelForTest(string arg)
            {
                CommandText = arg;
            }

            public override string Text 
            {
                get { return CommandText; }
               
            }

            public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
            {
                HasExecuted++;
            }

            public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
            {
                if (ModelCanExecute) base.OnQueryEnabled(sender, e);
                else e.CanExecute = false;
            }
        }

        internal class CommandModelForTestWithDescription : CommandModelForTest
        {
           
            internal CommandModelForTestWithDescription(RoutedUICommand command)
                : base(command)
            {

            }

            public override string DescriptionText
            {
                get
                {
                    return "description";
                }
            }

            public override string Text
            {
                get
                {
                    return "text";
                }
            }

            
        }
        
    }
}

  
