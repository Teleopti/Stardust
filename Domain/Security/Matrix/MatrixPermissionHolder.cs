using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    public class MatrixPermissionHolder
    {
        private readonly IPerson _person;
        private readonly ITeam _team;
        private readonly bool _isMy;
        private readonly IApplicationFunction _applicationFunction;

        public MatrixPermissionHolder
            (IPerson person,
             ITeam team,
             bool isMy)
        {
            _person = person;
            _team = team;
            _isMy = isMy;
        }

        public MatrixPermissionHolder
            (IPerson person,
             ITeam team,
             bool isMy, 
             IApplicationFunction applicationFunction)
            : this(person, team, isMy)
        {
            _applicationFunction = applicationFunction;
        }

        public IApplicationFunction ApplicationFunction
        {
            get { return _applicationFunction; }
        }

        public bool IsMy
        {
            get { return _isMy; }
        }

        public ITeam Team
        {
            get { return _team; }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public override int GetHashCode()
        {
            if(ApplicationFunction == null)
                return Person.GetHashCode() ^ Team.GetHashCode() ^ IsMy.GetHashCode();
            return Person.GetHashCode() ^ ApplicationFunction.GetHashCode() ^ Team.GetHashCode() ^ IsMy.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var casted = obj as MatrixPermissionHolder;
            if (obj == null || casted == null)
            {
                return false;
            }
            return (casted.GetHashCode() == this.GetHashCode());
        }

    }
}