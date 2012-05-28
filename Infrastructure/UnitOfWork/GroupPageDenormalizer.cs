using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class GroupPageDenormalizer : IDenormalizer
	{
		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IPerson),
		                                                        		typeof (IGroupPage),
		                                                        		typeof (ITeam),
		                                                        		typeof (ISite),
		                                                        		typeof (IContract),
		                                                        		typeof (IContractSchedule),
		                                                        		typeof (IPartTimePercentage),
		                                                        		typeof (IRuleSetBag),
		                                                        		typeof (ISkill)
		                                                        	};

		private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
		private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

		public GroupPageDenormalizer(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(IRunSql runSql, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var atLeastOneMessage = false;
			var affectedInterfaces = from r in modifiedRoots
			                         from i in r.Root.GetType().GetInterfaces()
			                         select i;
	        if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
			{
                //get the person ids
				var persons = modifiedRoots.Select(r => r.Root).OfType<IPerson>();
                foreach (var personList in persons.Batch(400))
                {
                    var idsAsString = (from p in personList select p.Id.ToString()).ToArray();
                    var ids = string.Join(",", idsAsString);
                    var message = new DenormalizeGroupingMessage
                    {
                        Ids = ids,
                        GroupingType = 1,
                    };
                    _saveToDenormalizationQueue.Execute(message, runSql);
                	atLeastOneMessage = true;
                }
				
                //get the group page ids
				var groupPage = modifiedRoots.Select(r => r.Root).OfType<IGroupPage>();
                foreach (var groupPageList in groupPage.Batch(400))
                {
                    var idsAsString = (from p in groupPageList select p.Id.ToString()).ToArray();
                    var ids = string.Join(",", idsAsString);
                    var message = new DenormalizeGroupingMessage
                    {
                        Ids = ids,
                        GroupingType = 2,
                    };
                    _saveToDenormalizationQueue.Execute(message, runSql);
					atLeastOneMessage = true;
                  }

                //get the ids which are not in person or in grouppage
                var notPerson = (from p in modifiedRoots where !((p.Root is Person)||(p.Root is GroupPage ) ) select p.Root).ToList();
                foreach (var notpersonList in notPerson.Batch(400))
                {
                    var idsAsString = (from p in notpersonList select ((IAggregateRoot)p).Id.ToString()).ToArray();
                    var ids = string.Join(",", idsAsString);
                    var message = new DenormalizeGroupingMessage
                    {
                        Ids = ids,
                        GroupingType = 3,
                    };
                    _saveToDenormalizationQueue.Execute(message, runSql);
					atLeastOneMessage = true;
                 }
				if (atLeastOneMessage)
				{
					_sendDenormalizeNotification.Notify();
				}
			}
		}
	}
}