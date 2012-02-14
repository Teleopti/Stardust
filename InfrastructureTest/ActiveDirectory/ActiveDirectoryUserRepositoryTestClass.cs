using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;
using Teleopti.Ccc.Infrastructure.ActiveDirectory;

namespace Teleopti.Ccc.InfrastructureTest.ActiveDirectory
{
    /// <summary>
    /// Testable ActiveDirectoryUserRepository Class
    /// </summary>
    public class ActiveDirectoryUserRepositoryTestClass : ActiveDirectoryUserRepository
    {
        private IDirectorySearcherChannel _searcher;

        /// <summary>
        /// Sets the local directory searcher.
        /// </summary>
        /// <param name="searcher">The searcher.</param>
        public void SetDirectorySearcher(IDirectorySearcherChannel searcher)
        {
            _searcher = searcher;
        }

        /// <summary>
        /// Creates the new directory searcher.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Method needed for making the method testable.
        /// </remarks>
        protected override IDirectorySearcherChannel CreateNewDirectorySearcher()
        {
            return _searcher ?? new DirectorySearcherChannel();
        }


    }
}
