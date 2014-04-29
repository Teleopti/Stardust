using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.Views
{
    public partial class UserCredentialConflicts : BaseDialogForm
    {
        public UserCredentialConflicts()
        {
            InitializeComponent();
            Bitmap png = Properties.Resources.ccc_Cancel_16x16;
            Icon =  Icon.FromHandle(png.GetHicon());
            KeyPreview = true;
        }

        public UserCredentialConflicts(IEnumerable<ISameUserCredentialOnOther> sameUserCredentialOnOthers):this()
        {
            SetTexts();
            HelpButton = false;
            
            if(sameUserCredentialOnOthers == null) return;
            
            int count = 0;
            foreach (var sameUserCredentialOnOther in sameUserCredentialOnOthers)
            {
                count ++;
                var group = new ListViewGroup(UserTexts.Resources.ErrorMsgDuplicateUserCredentials);
                listViewConflicts.Groups.Add(group);
                var item = createItem(sameUserCredentialOnOther.Person);
                item.Group = group;
                listViewConflicts.Items.Add(item);

                var conflictItem = createItem(sameUserCredentialOnOther.ConflictingPerson);
                conflictItem.Group = group;
                listViewConflicts.Items.Add(conflictItem);
            }
        }

        private static ListViewItem createItem(IPerson person)
        {
            var item = new ListViewItem(person.Name.ToString());
            item.SubItems.Add(new ListViewItem.ListViewSubItem(item,
                                                               person.AuthenticationInfo == null ? "" :
                                                               person.AuthenticationInfo.Identity));
            item.SubItems.Add(new ListViewItem.ListViewSubItem(item,
                                                               person.ApplicationAuthenticationInfo == null ? "" :
                                                               person.ApplicationAuthenticationInfo.ApplicationLogOnName));
            if(person.TerminalDate.HasValue)
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, person.TerminalDate.Value.ToShortDateString(CultureInfo.CurrentCulture)));

            return item;
        }

        private void userCredentialConflictsKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode.Equals(Keys.Escape))
                Close();
        }

    }
}
