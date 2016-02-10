using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
    [DatabaseTest]
    public class NonceInfoRepositoryTest
    {
        public ITenantUnitOfWork TenantUnitOfWork;
        public INonceInfoRepository Target;

        [Test]
        public void ShouldFindNonce()
        {
            using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
            {
                const string context = "context";
                const string nonce = "nonce";
                var expiration = DateTime.Today;

                Target.Add(new NonceInfo
                {
                    Context = context,
                    Nonce = nonce,
                    Timestamp = expiration
                });
                var result = Target.Find(context, nonce, expiration);
                result.Should().Not.Be.Null();
                result.Context.Should().Be.EqualTo(context);
                result.Nonce.Should().Be.EqualTo(nonce);
                result.Timestamp.Should().Be.EqualTo(expiration);
            }
        }

        [Test]
        public void ShouldReturnNullIfNonceNotExists()
        {
            using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
            {
                const string context = "context";
                const string nonce = "nonce";
                var expiration = DateTime.Today;

                var result = Target.Find(context, nonce, expiration);
                result.Should().Be.Null();
            }
        }

        [Test]
        public void ShouldClearExpiredNonces()
        {
            using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
            {
                const string context = "context";
                const string nonce = "nonce";
                var timestamp = DateTime.Today;

                var expired = timestamp.Subtract(TimeSpan.FromMinutes(3));
                Target.Add(new NonceInfo
                {
                    Context = context,
                    Nonce = nonce,
                    Timestamp = expired
                });

                var nonExpired = timestamp.Add(TimeSpan.FromMinutes(3));
                Target.Add(new NonceInfo
                {
                    Context = context,
                    Nonce = nonce,
                    Timestamp = nonExpired
                });

                Target.ClearExpired(timestamp);

                Target.Find(context, nonce, expired).Should().Be.Null();
                Target.Find(context, nonce, nonExpired).Should().Not.Be.Null();
            }
        }

        [Test]
        public async void ShouldNotDeadlockWhenInsertingAndClearingExpired()
        {
            var timestamp = DateTime.Today;
            const string context = "context";
            const string nonce = "nonce";
            DateTime expired = timestamp.Subtract(TimeSpan.FromMinutes(3));
            DateTime nonExpired = timestamp.Add(TimeSpan.FromMinutes(3));

            using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
            {
                Target.Add(new NonceInfo
                {
                    Context = context,
                    Nonce = nonce,
                    Timestamp = nonExpired
                });
                Target.Add(new NonceInfo
                {
                    Context = context,
                    Nonce = nonce,
                    Timestamp = expired
                });
                for (var b = 0; b < 10000; b++)
                {
                    Target.Add(new NonceInfo
                    {
                        Context = context,
                        Nonce = Guid.NewGuid().ToString(),
                        Timestamp = expired
                    });
                }
            }
            var tasks = new List<Task>();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(new Task(() =>
                {
                    var n = Guid.NewGuid().ToString();
                    using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
                    {
                        
                        var nonceInfo = Target.Find(context, n, nonExpired);
                        if (nonceInfo == null)
                        {
                            
                            Target.Add(new NonceInfo
                            {
                                Context = context,
                                Nonce = n,
                                Timestamp = nonExpired
                            });

                            Target.ClearExpired(timestamp);
                        }                       
                    }
                }));
            }

            foreach (var task in tasks)
                task.Start();

            await Task.WhenAll(tasks.ToArray());

            using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
            {
                Target.Find(context, nonce, expired).Should().Be.Null();
                Target.Find(context, nonce, nonExpired).Should().Not.Be.Null();
            }
        }
    }
}