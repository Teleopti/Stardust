﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(IRunSql runSql, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var affectedInterfaces = from r in modifiedRoots
			                         from i in r.Root.GetType().GetInterfaces()
			                         select i;

			if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
			{
				var persons = (from p in modifiedRoots where p.Root is Person select p.Root).ToList();
				foreach (var personList in persons.Batch(400))
				{
					var idsAsString = (from p in personList select ((IAggregateRoot)p).Id.ToString()).ToArray();
					var ids = string.Join(",", idsAsString);
					runSql.Create(string.Format("exec [ReadModel].[UpdateGroupingReadModel] '{0}'", ids))
						.Execute();
				}
				//runSql.Create("exec ReadModel.UpdateGroupingReadModel").Execute();
			}
		}
	}
}