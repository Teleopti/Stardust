using System;
using Infrastructure;
using log4net;
using IState=Infrastructure.IState;
using StateHolder=Infrastructure.StateHolder;
using User=Domain.User;

namespace Teleopti.Ccc.DBConverter
{
    public class Initialize
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Initialize));

        public void CreateOldDomainAndLogOn(IState state, string sourceServer, string sourceDatabase, int componentId)
        {
            Setup.Initialize(state, sourceServer, sourceDatabase);
            User aUser = get6xUser();
            StateHolder.Instance.SetLoginValue(aUser);
            StateHolder.Instance.Component = componentId;
        }

        private static User get6xUser()
        {
            UserReader aReader = new UserReader();
            User aUser;
            try
            {
                aUser = aReader.GetUserFromDB(Environment.UserDomainName, Environment.UserName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error: ", ex);
                throw;
            }
            return aUser;
        }
    }
}
