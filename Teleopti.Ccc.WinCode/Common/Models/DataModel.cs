using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Threading;

namespace Teleopti.Ccc.WinCode.Common.Models
{
    public class DataModel : INotifyPropertyChanged
    {
        private Dispatcher _dispatcher;
        private ModelState _state;
        private PropertyChangedEventHandler _propertyChangedEvent;
     
        public Dispatcher Dispatcher
        {
            get { return _dispatcher; }
        }

        public ModelState State
        {
            get
            {
                VerifyCalledOnUIThread();
                return _state;
            }
            set
            {
                VerifyCalledOnUIThread();
                if (value != _state)
                {
                    _state = value;
                    SendPropertyChanged("State");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                VerifyCalledOnUIThread();
                _propertyChangedEvent += value;
            }
            remove
            {
                VerifyCalledOnUIThread();
                _propertyChangedEvent -= value;
            }
        }
        
        public DataModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected void SendPropertyChanged(string property)
        {
            if (_propertyChangedEvent != null) _propertyChangedEvent(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Fires PropertyChanged with the propertyname
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="property">The property.</param>
        /// <remarks>
        /// Use: ()=>Property
        /// Typesafe, but slower than the SendPropertyChanged
        /// Created by: henrika
        /// Created date: 2010-06-02
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected void NotifyProperty<TProperty>(Expression<Func<TProperty>> property)
        {
            var lambda = (LambdaExpression)property; MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
            }
            else memberExpression = (MemberExpression)lambda.Body;
            SendPropertyChanged(memberExpression.Member.Name);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Conditional("Debug")]
        protected void VerifyCalledOnUIThread()
        {
            Debug.Assert(Dispatcher.CurrentDispatcher == this.Dispatcher, "Call must be made on current Dispatcher");
        }
    }
}
