using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Filter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging.Filters;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging
{
    /// <summary>
    /// Viewmodel for replyoptions. 
    /// </summary>
    /// <remarks>
    /// Holds a filter that can calculate the number of answers in a collection of IFollowUpMessageDialogueViewModel
    /// Created by: henrika
    /// Created date: 2009-05-29
    /// </remarks>
    public class ReplyOptionViewModel:DependencyObject
    {
        #region fields & props

        public SpecificationFilter<IFollowUpMessageDialogueViewModel> Filter { get; private set; }
        public ListCollectionView FilteredView { get; private set; }
        public ListCollectionView DefaultView { get; private set; }
        public CommandModel FilterCommand { get; private set; }

        public bool FilterIsActive
        {
            get { return (bool)GetValue(FilterIsActiveProperty); }
            set { SetValue(FilterIsActiveProperty, value); }
        }

        public string Reply
        {
            get { return (string)GetValue(ReplyProperty); }
            set { SetValue(ReplyProperty, value); }
        }

        public int Total
        {
            get { return (int)GetValue(TotalProperty); }
            set { SetValue(TotalProperty, value); }
        }

        public bool IsNotRepliedOption { get; private set; }
    
        public IFilterTarget FilterTarget { get; set; }

        public static readonly DependencyProperty TotalProperty =
           DependencyProperty.Register("Total", typeof(int), typeof(ReplyOptionViewModel), new UIPropertyMetadata(0));

        public static readonly DependencyProperty ReplyProperty =
          DependencyProperty.Register("Reply", typeof(string), typeof(ReplyOptionViewModel), new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty FilterIsActiveProperty =
         DependencyProperty.Register("FilterIsActive", typeof(bool), typeof(ReplyOptionViewModel), new UIPropertyMetadata(false));


        #endregion
        
        #region ctor
        public ReplyOptionViewModel(string reply, ObservableCollection<IFollowUpMessageDialogueViewModel> collection)
        {
           
            FilteredView = new ListCollectionView(collection);
            DefaultView = new ListCollectionView(collection);
            Reply = reply;
            Filter = new SpecificationFilter<IFollowUpMessageDialogueViewModel>();
            Filter.Filter = new DialogueReplySpecification(reply);
            FilteredView.Filter = Filter.FilterAllButSpecification;
            FilterCommand = CommandModelFactory.CreateCommandModel(ApplyFilter,ApplyFilterCanExecute, UserTexts.Resources.Filter);
        }

        public ReplyOptionViewModel(string reply)
            : this(reply, new ObservableCollection<IFollowUpMessageDialogueViewModel>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyOptionViewModel"/> class.
        /// Creates a IsNotReplied Specification
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-08-24
        /// </remarks>
        public ReplyOptionViewModel(ObservableCollection<IFollowUpMessageDialogueViewModel> collection)
        {
            FilteredView = new ListCollectionView(collection);
            DefaultView = new ListCollectionView(collection);
            Reply = UserTexts.Resources.NotReplied;
            Filter = new SpecificationFilter<IFollowUpMessageDialogueViewModel>();
            Filter.Filter = new DialogueIsRepliedSpecification();
            FilteredView.Filter = Filter.FilterAllButSpecification;
            FilterCommand = CommandModelFactory.CreateCommandModel(ApplyFilter, ApplyFilterCanExecute, UserTexts.Resources.Filter);
            IsNotRepliedOption = true;
        }
       
        #endregion


        private bool ApplyFilterCanExecute()
        {
            return FilterTarget != null;
        }

       
        

        private void ApplyFilter()
        {
           if (FilterIsActive)
               FilterTarget.RemoveFilter(this);
           else
               FilterTarget.AddFilter(this);
        }
        
    }
}
