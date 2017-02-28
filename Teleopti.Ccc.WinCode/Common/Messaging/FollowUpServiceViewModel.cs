using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Common.Filter;
using Teleopti.Ccc.WinCode.Common.Messaging.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
	/// <summary>
	/// Responsible for loading and presenting Conversations (Headers)
	/// </summary>
	/// <remarks>
	/// Created by: henrika
	/// Created date: 2009-05-19
	/// </remarks>
	public class FollowUpServiceViewModel : IObservable<FollowUpPushMessageViewModel>
	{
		private readonly IPerson _sender;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly PagingDetail _pagingDetail = new PagingDetail {Take = 20};

		public FollowUpServiceViewModel(IPerson sender, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_sender = sender;
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			Load = new LoadCommandModel(this, repositoryFactory, unitOfWorkFactory);
			LoadNextPage = new LoadNextCommandModel(this, _pagingDetail);
			LoadPreviousPage = new LoadPreviousCommandModel(this, _pagingDetail);
			PushMessages = new ObservableCollection<IFollowUpPushMessageViewModel>();
			PushMessageFilter = new SpecificationFilterViewModel<IFollowUpPushMessageViewModel>(PushMessages, new PushMessageIsRepliedSpecification());
		}

		public ICommand LoadNextPage { get; private set; }
		public ICommand LoadPreviousPage { get; private set; }
		public CommandModel Load { get; private set; }
		public SpecificationFilterViewModel<IFollowUpPushMessageViewModel> PushMessageFilter { get; private set; }
		public ObservableCollection<IFollowUpPushMessageViewModel> PushMessages
		{
			get;
			private set;
		}

		public PagingDetail PagingDetail { get { return _pagingDetail; } }

		private void loadPagedConversations(IPushMessageRepository repository)
		{
			PushMessages.Clear();

			foreach (IPushMessage message in repository.Find(_sender,_pagingDetail))
			{
				IFollowUpPushMessageViewModel modelToAdd = new FollowUpPushMessageViewModel(message,_repositoryFactory,_unitOfWorkFactory);
				modelToAdd.Observables.Add(this);
				PushMessages.Add(modelToAdd);
			}
		}

		private class LoadNextCommandModel : ICommand
		{
			private readonly FollowUpServiceViewModel _model;
			private readonly PagingDetail _pagingDetail;

			public LoadNextCommandModel(FollowUpServiceViewModel model,PagingDetail pagingDetail)
			{
				_model = model;
				_pagingDetail = pagingDetail;
				_pagingDetail.PropertyChanged += (sender, e) =>
				                                 	{
				                                 		var handler = CanExecuteChanged;
				                                 		if (handler != null)
				                                 		{
				                                 			handler.Invoke(this, EventArgs.Empty);
				                                 		}
				                                 	};
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public string Text
			{
				get { return UserTexts.Resources.DoubleArrowAdd; }
			}

			public void Execute(object parameter)
			{
				_pagingDetail.Skip += _pagingDetail.Take;
				_model.Load.OnExecute(this,null);
			}

			public bool CanExecute(object parameter)
			{
				return (_pagingDetail.Skip+_pagingDetail.Take) <= _pagingDetail.TotalNumberOfResults;
			}

			public event EventHandler CanExecuteChanged;
		}

		private class LoadPreviousCommandModel : ICommand
		{
			private readonly FollowUpServiceViewModel _model;
			private readonly PagingDetail _pagingDetail;

			public LoadPreviousCommandModel(FollowUpServiceViewModel model, PagingDetail pagingDetail)
			{
				_model = model;
				_pagingDetail = pagingDetail;
				_pagingDetail.PropertyChanged += (sender, e) =>
				{
					var handler = CanExecuteChanged;
					if (handler != null)
					{
						handler.Invoke(this, EventArgs.Empty);
					}
				};
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
			public string Text
			{
				get { return UserTexts.Resources.DoubleArrowRemove; }
			}

			public void Execute(object parameter)
			{
				_pagingDetail.Skip -= _pagingDetail.Take;
				_model.Load.OnExecute(this, null);
			}

			public bool CanExecute(object parameter)
			{
				return _pagingDetail.Skip > 0;
			}

			public event EventHandler CanExecuteChanged;
		}

		private class LoadCommandModel : RepositoryCommandModel
		{
			private readonly FollowUpServiceViewModel _model;
			private readonly IRepositoryFactory _repFactory;

			public LoadCommandModel(FollowUpServiceViewModel model, IRepositoryFactory repFactory, IUnitOfWorkFactory unitOfWorkFactory)
				: base(unitOfWorkFactory)
			{
				_model = model;
				_repFactory = repFactory;
			}
			public override string Text
			{
				get { return UserTexts.Resources.Load; }
			}

			public override void OnExecute(IUnitOfWork uow, object sender, ExecutedRoutedEventArgs e)
			{
				_model.loadPagedConversations(_repFactory.CreatePushMessageRepository(uow));
			}

			public override void OnQueryEnabled(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = true;
				e.Handled = true;
			}
		}

		public void Notify(FollowUpPushMessageViewModel item)
		{
			PushMessages.Remove(item);
		}
	}
}
