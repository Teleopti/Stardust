using NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public abstract class SettingDataRepository
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IUnitOfWork _unitOfWork;

        protected SettingDataRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected SettingDataRepository(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected ISession Session
        {
            get { return ((NHibernateUnitOfWork) UnitOfWork).Session; }
        }

        protected IUnitOfWork UnitOfWork
        {
            get
            {
                return _unitOfWork ?? _unitOfWorkFactory.CurrentUnitOfWork();
            }
        }

        public abstract ISettingData FindByKey(string key);

        public ISettingData PersistSettingValue(ISettingValue value)
        {
            var owner = value.BelongsTo;
            owner.SetValue(value);
            if (owner.Id.HasValue) //finns i db:n, ej nyskapad
            {
                owner = UnitOfWork.Merge(owner);
            }
            else
            {
                //kolla så att ingen annan tråd har hunnit först, isåfall - mergea ihop!
                var old = FindByKey(owner.Key);
                if (old != null)
                {
                    owner.SetId(old.Id.Value);
                    owner = UnitOfWork.Merge(owner);
                }
                else
                {
                    Session.Save(owner);
                }
            }
            return owner;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public ISettingData PersistSettingValue(string entityName, ISettingValue value)
        {
                var owner = value.BelongsTo;
                owner.SetValue(value);
                if (owner.Id.HasValue) //finns i db:n, ej nyskapad
                {
                    owner = UnitOfWork.Merge(owner);
                }
                else
                {
                    //kolla så att ingen annan tråd har hunnit först, isåfall - mergea ihop!
                    var old = FindByKey(entityName);
                    if (old != null)
                    {
                        owner.SetId(old.Id.Value);
                        owner = UnitOfWork.Merge(owner);
                    }
                    else
                    {
                        Session.Save(entityName,owner);
                    }
                }
                return owner;
            
        }

    }
}
