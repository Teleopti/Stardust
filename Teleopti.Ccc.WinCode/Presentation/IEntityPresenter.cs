using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{

    /// <summary>
    /// Base entity presenter interface
    /// </summary>
    public interface IEntityPresenter : IEntity
    {

        /// <summary>
        /// Gets the inner domain object.
        /// </summary>
        /// <value>The domain object.</value>
        object EntityObject
        { 
            get;
        }
    }

}
