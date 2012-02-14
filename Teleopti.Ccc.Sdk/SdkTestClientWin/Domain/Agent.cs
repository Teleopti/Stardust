using SdkTestClientWin.Sdk;

namespace SdkTestClientWin.Domain
{
    public class Agent
    {
        private readonly PersonDto _personDto;
        private readonly Team _team;

        public Agent(PersonDto personDto, Team team)
        {
            _personDto = personDto;
            _team = team;
        }

        public PersonDto Dto
        {
            get { return _personDto; }
        }

        public Team Team
        {
            get { return _team; }
        }
    }
}