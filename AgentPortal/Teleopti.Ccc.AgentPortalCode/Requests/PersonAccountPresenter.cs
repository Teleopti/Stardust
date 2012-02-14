using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public class PersonAccountPresenter
    {
        private readonly IPersonAccountView _view;
        private readonly ITeleoptiOrganizationService _service;
        private readonly PersonDto _personDto;
        private readonly IList<PersonAccountModel> _personAccountModels = new List<PersonAccountModel>();

        public PersonAccountPresenter(IPersonAccountView view, ITeleoptiOrganizationService service, PersonDto personDto)
        {
            _view = view;
            _service = service;
            _personDto = personDto;
        }

        public void Initialize()
        {
            _view.AccruedHeader = UserTexts.Resources.Accrued;
            _view.DescriptionHeader = UserTexts.Resources.Absence;
            _view.PeriodFromHeader = UserTexts.Resources.FromDate;
            _view.PeriodToHeader = UserTexts.Resources.To;
            _view.RemainingHeader = UserTexts.Resources.Remaining;
            _view.TypeOfValueHeader = UserTexts.Resources.Type;
            _view.UsedHeader = UserTexts.Resources.Used;
            _view.SetDate(DateTime.Today);
            _view.SetDateText(UserTexts.Resources.SelectedDateColon);
        }

        public void ChangeDate(DateTime dateTime)
        {
            CreatePersonAccountModels(_service.GetPersonAccounts(_personDto, new DateOnlyDto {DateTime = dateTime, DateTimeSpecified = true}));
            _view.DataLoaded();
        }

        public int ItemCount
        {
            get { return _personAccountModels.Count; }
        }

        public IList<PersonAccountModel> Items
        {
            get { return _personAccountModels; }
        }

        private void CreatePersonAccountModels(IEnumerable<PersonAccountDto> personAccountDtos)
        {
            _personAccountModels.Clear();
            foreach (PersonAccountDto personAccountDto in personAccountDtos)
            {
                _personAccountModels.Add(new PersonAccountModel(personAccountDto));
            }
        }
    }
}