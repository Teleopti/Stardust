﻿using System;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
	public class DelegateCommand : DelegateCommandBase
	{

		public DelegateCommand(Action executeMethod)
			: this(executeMethod, () => true)
		{
		}

		public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
			: base(o => executeMethod(), o => canExecuteMethod())
		{
			if (executeMethod == null || canExecuteMethod == null)
				throw new ArgumentNullException("executeMethod");
		}

		public void Execute()
		{
			Execute(null);
		}

		public bool CanExecute()
		{
			return CanExecute(null);
		}
	}

	public class DelegateCommand<T> : DelegateCommandBase
	{

		public DelegateCommand(Action<T> executeMethod)
			: this(executeMethod, (o) => true)
		{
		}

		public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
			: base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
		{
			if (executeMethod == null || canExecuteMethod == null)
				throw new ArgumentNullException("executeMethod");
		}

		public bool CanExecute(T parameter)
		{
			return base.CanExecute(parameter);
		}

		public void Execute(T parameter)
		{
			base.Execute(parameter);
		}
	}
}