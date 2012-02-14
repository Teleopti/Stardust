using System;
using System.IdentityModel.Tokens;

namespace Teleopti.Ccc.Sdk.Common.WcfExtensions
{
    public class CustomUserNameSecurityToken : UserNameSecurityToken, ITokenWithBusinessUnitAndDataSource
    {
        private readonly string _dataSource;
        private readonly Guid _businessUnit;

        public CustomUserNameSecurityToken(string userName, string password, string dataSource, Guid businessUnit) : base(userName,password)
        {
            _dataSource = dataSource;
            _businessUnit = businessUnit;
        }

        public Guid BusinessUnit
        {
            get { return _businessUnit; }
        }

        public bool HasBusinessUnit
        {
            get { return _businessUnit != Guid.Empty; }
        }

        public string DataSource
        {
            get { return _dataSource; }
        }
    }

    public interface ITokenWithBusinessUnitAndDataSource
    {
        Guid BusinessUnit { get; }
        bool HasBusinessUnit { get; }
        string DataSource { get; }
    }

    /*
    *- Ta hand om header information
    *- Om ingen datasource/header är angiven -> AnonymousIdentity
    *- Kontrollera cache för användare
    *    ingen träff -> Hämta upp användare för användarnamn / windowsanvändare + datasource = logga in + lägg till i cache
    *    träff       -> Hämta upp användare från cachen
    *- Kontrollera cache för varje roll => ClaimSet = roll
    *    ingen träff -> Hämta upp data för roll, konvertera till claimset + lägg till i cache
    *    träff       -> Hämta upp claimset från cache
    - Kontrollera cache för business unit
        ingen träff -> Hämta upp business unit
        träff       -> Hämta business unit från cache
    *- Koppla ihop allt med authorization context + principal?
    
    
    Frågor:
    - Licens, hur ska det hanteras?
    - Hur ska man bygga upp principal som funkar med webben?
    - Hur ska man bygga upp principal som funkar med fetklienten?
    - Ska vi wrappa AuthorizationContext?
     */
}