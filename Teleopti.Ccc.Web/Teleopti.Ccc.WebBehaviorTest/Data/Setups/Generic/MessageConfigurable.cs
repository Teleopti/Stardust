using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class MessageConfigurable : IUserDataSetup
	{
		public string Title { get; set; }
        public string Message { get; set; }

	    public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
		    var message = new PushMessage()
		                      {
		                          Title = Title,
		                          Message = "Hello",
                                  AllowDialogueReply = false
		                      };

			var repository = new PushMessageRepository(uow);
			repository.Add(message, new List<IPerson> {user});
		}
	}
}