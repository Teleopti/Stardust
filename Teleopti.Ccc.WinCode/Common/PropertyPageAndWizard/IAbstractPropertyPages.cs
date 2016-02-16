using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Interface for handling of several pages within a dialogue
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public interface IAbstractPropertyPages
    {
        /// <summary>
        /// Gets the first page.
        /// </summary>
        /// <value>The first page.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IPropertyPage FirstPage { get; }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>The current page.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IPropertyPage CurrentPage { get; set; }

        /// <summary>
        /// Gets the previous page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IPropertyPage PreviousPage();

        /// <summary>
        /// Gets the next page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IPropertyPage NextPage();

        /// <summary>
        /// Shows the page.
        /// </summary>
        /// <param name="propertyPage">The property page.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IPropertyPage ShowPage(IPropertyPage propertyPage);

        /// <summary>
        /// Determines whether [is on first].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is on first]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        bool IsOnFirst();

        /// <summary>
        /// Determines whether [is on last].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is on last]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        bool IsOnLast();

        /// <summary>
        /// Adds to repository.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        void AddToRepository();

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        IEnumerable<IRootChangeInfo> Save();

        /// <summary>
        /// Gets the page names.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        string[] GetPageNames();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <value>The window text.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-06-02
        /// </remarks>
        string WindowText { get; }

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <value>The pages.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        ReadOnlyCollection <IPropertyPage> Pages { get; }

        /// <summary>
        /// Occurs when [name changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        event EventHandler<WizardNameChangedEventArgs> NameChanged;

        /// <summary>
        /// Triggers the [name changed] event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard.WizardNameChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        void TriggerNameChanged(WizardNameChangedEventArgs eventArgs);

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        Form Owner { get; set; }

        /// <summary>
        /// Gets the minimum size.
        /// </summary>
        /// <value>The minimum size.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-01-28
        /// </remarks>
        Size MinimumSize { get; }

        /// <summary>
        /// Gets a value indicating whether [mode is "create new" false if in "edit existing item"].
        /// </summary>
        /// <value><c>true</c> if [mode create new]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        bool ModeCreateNew { get; }


        /// <summary>
        /// Loads a working copy of AggregateRoot.
        /// Typically used for Properties dialogs and such.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-23
        /// </remarks>
        void LoadAggregateRootWorkingCopy();
    }
}
