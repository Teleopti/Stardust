using System;
using System.Linq;
using System.Windows.Forms;
using SdkTestClientWin.Infrastructure;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
{
    public partial class UpdatePersonAccountForm : Form
    {
        private readonly ServiceApplication _service;
        private readonly PersonAccountDto _personAccount;
        public event EventHandler<PersonAccountSavedEventArgs> PersonAccountSaved;

        public UpdatePersonAccountForm(ServiceApplication service)
        {
            _service = service;
            InitializeComponent();
        }

        public UpdatePersonAccountForm(ServiceApplication service, PersonAccountDto personAccount)
        {
            _service = service;
            _personAccount = personAccount;
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            loadAbsences();
            loadPersonAccount();
        }

        private void loadPersonAccount()
        {
            if (_personAccount == null) return;
            textBoxAccrued.Text = _personAccount.Accrued.ToString();
            textBoxBalanceIn.Text = _personAccount.BalanceIn.ToString();
            textBoxExtra.Text = _personAccount.Extra.ToString();
            dateTimePickerDateFrom.Value = _personAccount.Period.StartDate.DateTime;
            comboBoxTracker.SelectedIndex = comboBoxTracker.FindString(_personAccount.TrackingDescription);
            comboBoxTracker.Enabled = false;
            dateTimePickerDateFrom.Enabled = false;
        }

        private void loadAbsences()
        {
            var absencesDtos = _service.SchedulingService.GetAbsences(new AbsenceLoadOptionDto() { LoadDeleted = false, LoadDeletedSpecified = true });
            var datasource = absencesDtos.Where(a => a.IsTrackable).ToList();
            comboBoxTracker.DataSource = datasource;
            comboBoxTracker.DisplayMember = "Name";
            comboBoxTracker.ValueMember = "Id";
            if (comboBoxTracker.Items.Count != 0)
                comboBoxTracker.SelectedIndex = 0;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var eventArgs = new PersonAccountSavedEventArgs();
            eventArgs.AbsenceId = new Guid(comboBoxTracker.SelectedValue.ToString());
            eventArgs.DateFrom = new DateOnlyDto { DateTime = dateTimePickerDateFrom.Value.Date, DateTimeSpecified = true };
            if (!string.IsNullOrEmpty(textBoxAccrued.Text))
                eventArgs.Accrued = long.Parse(textBoxAccrued.Text);
            if (!string.IsNullOrEmpty(textBoxBalanceIn.Text))
                eventArgs.BalanceIn = long.Parse(textBoxBalanceIn.Text);
            if (!string.IsNullOrEmpty(textBoxExtra.Text))
                eventArgs.Extra = long.Parse(textBoxExtra.Text);
            invokePersonAccountSaved(eventArgs);

            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void invokePersonAccountSaved(PersonAccountSavedEventArgs e)
        {
            EventHandler<PersonAccountSavedEventArgs> handler = PersonAccountSaved;
            if (handler != null) handler(this, e);
        }
    }

    public class PersonAccountSavedEventArgs : EventArgs
    {
        public Guid AbsenceId { get; set; }
        public DateOnlyDto DateFrom { get; set; }
        public long? BalanceIn { get; set; }
        public long? Extra { get; set; }
        public long? Accrued { get; set; }
    }
}
