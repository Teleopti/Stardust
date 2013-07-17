using System;
using System.IO;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Common.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Security
{
    public class PasswordPolicyServiceViewModel : DataModel
    {
        private IPasswordPolicy _model;
        private readonly ILoadPasswordPolicyService _loadPasswordService  =new LoadPasswordPolicyService(string.Empty);
        private string _path;
        private bool _filePathIsOk;
        private string _password;
        private bool _passwordIsStrongEnough;

        #region properties
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                NotifyProperty(() => Path);
                FilePathIsOk = File.Exists(Path);

            }
        }
        public bool FilePathIsOk
        {
            get { return _filePathIsOk; }
            private set
            {
                _filePathIsOk = value;
                NotifyProperty(() => FilePathIsOk);
            }
        }
        public CommandModel LoadFileCommand
        {
            get;
            private set;
        }
        public bool PasswordIsStrongEnough
        {
            get
            {
                return _passwordIsStrongEnough;
            }
            private set
            {
                _passwordIsStrongEnough = value;
                NotifyProperty(() => PasswordIsStrongEnough);
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                NotifyProperty(() => Password);
                if (_model != null) PasswordIsStrongEnough = _model.CheckPasswordStrength(Password);
            }
        }

        #region exposed
        public TimeSpan InvalidAttemptWindow 
        {
            get { return _model.InvalidAttemptWindow; }
        }
        
        public int MaxAttemptCount
        {
            get { return _model.MaxAttemptCount; }
        }
        
        public int PasswordValidForDayCount
        {
            get { return _model.PasswordValidForDayCount; }
        }
        public int PasswordExpireWarningDayCount
        {
            get { return _model.PasswordExpireWarningDayCount; }
        }
        #endregion
        #endregion //properties

        public PasswordPolicyServiceViewModel()
        {
            LoadFileCommand = CommandModelFactory.CreateCommandModel(loadFile, canLoadFile, CommonRoutedCommands.LoadPasswordPolicy);
        }

        public PasswordPolicyServiceViewModel(ILoadPasswordPolicyService loadPasswordService):this()
        {
            _loadPasswordService = loadPasswordService;

        }

        public PasswordPolicyServiceViewModel(IPasswordPolicy passwordPolicy)
            : this()
        {
            _model = passwordPolicy;
        }


        private bool canLoadFile()
        {
            return true;
        }

        private void loadFile()
        {
            _loadPasswordService.Path = Path;
            _model  = new PasswordPolicy(_loadPasswordService);
        
            NotifyProperty(() => InvalidAttemptWindow);
            NotifyProperty(() => MaxAttemptCount);
            NotifyProperty(() => PasswordValidForDayCount);
            NotifyProperty(() => PasswordExpireWarningDayCount);
        
        }
    }






}
