using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    /// <summary>
    /// Class for easier testing of commands and commandmodels
    /// </summary>
    /// <remarks>
    /// Made so the developer doesnt have to create Eventargs etc..
    /// Created by: henrika
    /// Created date: 2009-05-20
    /// </remarks>
    /// <example>
    /// Testing a commandmodel:
    /// For testing if a commandmodel can execute:
    /// Assert.IsTrue(new TesterForCommandModels.CanExecute(mycommandmodel);
    /// 
    /// And for easy executing it:
    /// new TesterForCommandModels.Execute(mycommandmodel);
    /// 
    /// For checking Handled, setting parameters etc.. create the arguments with the helper and call the OnQueryEnabled/OnExecute methods on the model
    /// </example>
    public class TesterForCommandModels
    {

        private readonly Type _typeOfCommand = typeof (ICommand);
        private readonly Type _typeOfObject  = typeof (object);

        private static readonly RoutedEvent CommandHelperFakeEvent = EventManager.RegisterRoutedEvent(
        "CommandHelperFake", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TesterForCommandModels));


        public CanExecuteRoutedEventArgs CreateCanExecuteRoutedEventArgs(RoutedCommand command)
        {
            Type[] types = { _typeOfCommand, _typeOfObject };
            object[] param = { command, null };
            //Create the CreateCanExecuteRoutedEventArgs
            ConstructorInfo ctorInfoArg = typeof(CanExecuteRoutedEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null);
            CanExecuteRoutedEventArgs arg = (CanExecuteRoutedEventArgs)ctorInfoArg.Invoke(param);

            arg.RoutedEvent = TesterForCommandModels.CommandHelperFakeEvent;
            return arg;
        }

        public CanExecuteRoutedEventArgs CreateCanExecuteRoutedEventArgs()
        {
            return CreateCanExecuteRoutedEventArgs(new RoutedCommand());
        }

        public ExecutedRoutedEventArgs CreateExecutedRoutedEventArgs()
        {
            return CreateExecutedRoutedEventArgs(new RoutedCommand());
        }

        public ExecutedRoutedEventArgs CreateExecutedRoutedEventArgs(ICommand command)
        {
            Type[] types = { _typeOfCommand, _typeOfObject };
            object[] param = { command, null };
            //Create the ExecutedRoutedEventArgs
            ConstructorInfo ctorInfoArg = typeof(ExecutedRoutedEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null);
            return (ExecutedRoutedEventArgs)ctorInfoArg.Invoke(param);
        }

        #region testing CommandModels

        /// <summary>
        /// Runs the CommandModels CanExecute
        /// </summary>
        /// <param name="modelToTest">The commandmodel</param>
        /// <returns>
        /// 	<c>true</c> if this instance can execute with the specified sender; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Runs the actual method of the command, creates fake events
        /// Sets the sender to null
        /// </remarks>
        public bool CanExecute(CommandModel modelToTest)
        {
            return CanExecute(null, modelToTest);
        }

        /// <summary>
        /// Runs the CommandModels CanExecute
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="modelToTest">The commandmodel</param>
        /// <returns>
        /// 	<c>true</c> if this instance can execute with the specified sender; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Runs the actual method of the command, creates fake events
        /// </remarks>
        public bool CanExecute(object sender,CommandModel modelToTest)
        {
            var canExecArgs = CreateCanExecuteRoutedEventArgs();
            modelToTest.OnQueryEnabled(sender, canExecArgs);
            return canExecArgs.CanExecute;
        }

        /// <summary>
        /// Executes the command model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="modelToTest">The model to test.</param>
        public void ExecuteCommandModel(object sender,CommandModel modelToTest)
        {
            modelToTest.OnExecute(sender, CreateExecutedRoutedEventArgs());
        }

        /// <summary>
        /// Executes the command model.
        /// </summary>
        /// <param name="modelToTest">The model to test.</param>
        /// <remarks>
        /// Sets the sender to null
        /// </remarks>
        public void ExecuteCommandModel(CommandModel modelToTest)
        {
            ExecuteCommandModel(null, modelToTest);
        }

        /// <summary>
        /// Executes the repository commandmodel and provides a unitofwork
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="repositoryCommandModel">The repository command model.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <remarks>
        /// For testing repositorycommands without setting up unitofworkfactory in tests
        /// </remarks>
        public void ExecuteRepositoryCommandModel(object sender,RepositoryCommandModel repositoryCommandModel, IUnitOfWork unitOfWork)
        {
            repositoryCommandModel.OnExecute(unitOfWork,sender,CreateExecutedRoutedEventArgs());
        }
        #endregion    
    }
}
