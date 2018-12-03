using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    /// <summary>
    /// Base user control with unit of work for use in Raptor project.
    /// </summary>
    /// <remarks>
    /// copied from the baseformwuow...
    /// Created by: östenp
    /// Created date: 2007-01-15
    /// </remarks>
    
    public class BaseRibbonFormWithUnitOfWork : BaseRibbonForm 
    {
        private bool _formUowCreatedHere;
        private readonly IDictionary<object, IUnitOfWork> _objectsWithOwnUow
                     = new Dictionary<object, IUnitOfWork>();
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFormWithUnitOfWork"/> class.
        /// </summary>
        /// <remarks>
        /// Remark 1: 
        /// NOTE: Do not write any code that uses or exposes the unit of work after the Close method has been called even if the form is modal.
        /// As a matter of fact, the Close method on an modeless form will call the form's dispose method unlike
        /// on a modal form where you have to call it explicitly after you have read the return values. Here, 
        /// the disposal of unit of work is connected to the Close event, so the uow will be disposed regardless
        /// of whether you opened the form modal or modeless. As a result, if you open the form modal (ShowDialog),
        /// the unit of work will be disposed by the time you get the DialogResult back.
        /// Created by: robink, added to baseribbon by östen
        /// Created date: 2008-01-15
        /// </remarks>
        public BaseRibbonFormWithUnitOfWork()
        {
            if (DesignMode ||
                !StateHolderReader.IsInitialized) return;
            _formUowCreatedHere = true;
            SetFormColor();
        }

        /// <summary>
        /// Sets the color of the form.
        /// </summary>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2/21/2008
        /// </remarks>
        private void SetFormColor()
        {
            ColorScheme = ColorSchemeType.Managed;
            // this.ribbonControlAdv1.OfficeColorScheme = ToolStripEx.ColorScheme.Managed;//not sure if needed - have to make a interface to force everyone to use

            //TODO: Osten! Try to open scheduler twice and you'll get an exception here
            try
            {
                Office12ColorTable.ApplyManagedColors(this, ColorHelper.RibbonFormBaseColor());
                Office2007Colors.ApplyManagedColors(this, ColorHelper.RibbonFormBaseColor());
            }
            catch (NullReferenceException)
            {

            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            DisposeLocalUows();
        }

        /// <summary>
        /// Persists all.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-09
        /// </remarks>
        protected void PersistAll()
        {
            PersistAll(this);
        }

        /// <summary>
        /// Persists all.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-09
        /// </remarks>
        protected void PersistAll(object key)
        {
            IUnitOfWork uow = GetObjectsUnitOfWork(key);

            IEnumerable<IRootChangeInfo> updatesMade;
            using (var runSql = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                updatesMade = uow.PersistAll();
                runSql.PersistAll();
            }

            EntityEventAggregator.TriggerEntitiesNeedRefresh(ParentForm, updatesMade);
        }
        /// <summary>
        /// Disposes the local uows.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        private void DisposeLocalUows()
        {
            foreach (KeyValuePair<object, IUnitOfWork> item in _objectsWithOwnUow)
            {
                if (item.Key.Equals(this) && !_formUowCreatedHere)
                    continue;

                item.Value.Dispose();
            }
            _objectsWithOwnUow.Clear();
        }

        /// <summary>
        /// Gets the unit of work for the form.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Will return a new or reconnected unit of work with open connection status.
        /// NOTE: Disconnect unit of work when you are done with your job!
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        protected IUnitOfWork UnitOfWork
        {
            get
            {
                return GetObjectsUnitOfWork(this);
            }
        }
        /// <summary>
        /// Gets the object's own unit of work.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// Created or reopened unit of work with open connection.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-02
        /// </remarks>
        protected IUnitOfWork GetObjectsUnitOfWork(object value)
        {
            IUnitOfWork theUow;
            lock (_objectsWithOwnUow)
            {
                if (!_objectsWithOwnUow.ContainsKey(value))
                {
                    theUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();
                    _objectsWithOwnUow[value] = theUow;
                }
                else
                {
                    theUow = _objectsWithOwnUow[value];
                }
            }
            return theUow;
        }
    }//ribbonform
}//namespace
