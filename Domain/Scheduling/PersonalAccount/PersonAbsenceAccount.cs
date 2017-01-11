using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.PersonalAccount
{
	public class PersonAbsenceAccount : VersionedAggregateRoot, IPersonAbsenceAccount
	{
		private readonly IPerson _person;
		private readonly IAbsence _absence;
		private IList<IAccount> accountCollection;

		public PersonAbsenceAccount(IPerson person, IAbsence absence)
		{
			InParameter.NotNull(nameof(absence), absence);
			_person = person;
			_absence = absence;
			accountCollection = new List<IAccount>();
		}

		protected PersonAbsenceAccount()
		{
		}

        public virtual IEnumerable<IAccount> AccountCollection()
        {
            var ret = new List<IAccount>(accountCollection);
            ret.Sort(new AccountDateDescendingComparer());

            return new ReadOnlyCollection<IAccount>(ret);
        }

		public virtual IPerson Person => _person;

		public virtual IAbsence Absence => _absence;

		public virtual void Add(IAccount account)
		{
			account.SetParent(this);
			accountCollection.Add(account);
		}

		public virtual void Remove(IAccount account)
		{
			accountCollection.Remove(account);
		}

		public virtual IAccount Find(DateOnly dateOnly)
		{
			return Find(dateOnly.ToDateOnlyPeriod()).FirstOrDefault();
		}

		public virtual IEnumerable<IAccount> Find(DateOnlyPeriod dateOnlyPeriod)
		{
			var retList = new SortedSet<IAccount>(new AccountDateDescendingComparer());
			foreach (var account in accountCollection)
			{
				if (account.Period().Intersection(dateOnlyPeriod).HasValue)
					retList.Add(account);
			}

			return retList;
		}

		public virtual void Restore(IPersonAbsenceAccount previousState)
		{
			//beh�vs inte person & absence h�r?
			accountCollection.Clear();
			previousState.AccountCollection().ForEach(accountCollection.Add);
		}

		public virtual IMemento CreateMemento()
		{
			return new Memento<IPersonAbsenceAccount>(this,
												EntityClone());
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual IPersonAbsenceAccount NoneEntityClone()
		{
			var clonedEntity = (PersonAbsenceAccount)MemberwiseClone();
			clonedEntity.SetId(null);
			clonedEntity.accountCollection = new List<IAccount>(accountCollection.Count);
			foreach (IAccount account in accountCollection)
			{
				var clonedAccount = account.NoneEntityClone();
				clonedAccount.SetParent(clonedEntity);
				clonedEntity.accountCollection.Add(clonedAccount);
			}
			return clonedEntity;
		}

		public virtual IPersonAbsenceAccount EntityClone()
		{
			var clonedEntity = (PersonAbsenceAccount)MemberwiseClone();
			clonedEntity.accountCollection = new List<IAccount>(accountCollection.Count);
			foreach (IAccount account in accountCollection)
			{
				var clonedAccount = account.EntityClone();
				clonedAccount.SetParent(clonedEntity);
				clonedEntity.accountCollection.Add(clonedAccount);
			}
			return clonedEntity;
		}
	}
}
