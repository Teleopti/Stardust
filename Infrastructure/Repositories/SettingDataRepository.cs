using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public abstract class SettingDataRepository
    {
	    protected SettingDataRepository(ICurrentUnitOfWork currentUnitOfWork)
	    {
			CurrentUnitOfWork = currentUnitOfWork;
	    }

		protected ICurrentUnitOfWork CurrentUnitOfWork { get; private set; }

	    public abstract ISettingData FindByKey(string key);

        public ISettingData PersistSettingValue(ISettingValue value)
        {
	        var uow = CurrentUnitOfWork.Current();
            var owner = value.BelongsTo;
            owner.SetValue(value);
            if (owner.Id.HasValue) //finns i db:n, ej nyskapad
            {
                owner = uow.Merge(owner);
            }
            else
            {
                //kolla så att ingen annan tråd har hunnit först, isåfall - mergea ihop!
                var old = FindByKey(owner.Key);
                if (old != null)
                {
                    owner.SetId(old.Id.Value);
                    owner = uow.Merge(owner);
                }
                else
                {
                    uow.Session().Save(owner);
                }
            }
            return owner;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public ISettingData PersistSettingValue(string entityName, ISettingValue value)
        {
			var uow = CurrentUnitOfWork.Current();
			var owner = value.BelongsTo;
                owner.SetValue(value);
                if (owner.Id.HasValue) //finns i db:n, ej nyskapad
                {
                    owner = uow.Merge(owner);
                }
                else
                {
                    //kolla så att ingen annan tråd har hunnit först, isåfall - mergea ihop!
                    var old = FindByKey(entityName);
                    if (old != null)
                    {
                        owner.SetId(old.Id.Value);
                        owner = uow.Merge(owner);
                    }
                    else
                    {
					uow.Session().Save(entityName,owner);
                    }
                }
                return owner;
            
        }

    }
}
