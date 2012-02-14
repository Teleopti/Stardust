using System;
using System.Globalization;
using System.Web;

namespace Teleopti.Analytics.Portal.Utils
{
    /// <summary>
    /// Holds information about the logged in User
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-05-10    
    /// /// </remarks>
    public class UserInfo
    {
        private readonly string _userName;
        private readonly int _langId;
        private readonly Guid _userId;
        private readonly int _cultureId;
        private readonly string _personName;


         


        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfo"/> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="langId">The lang ID.</param>
        /// <param name="cultureId">The culture ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="personName"></param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-10    
        /// /// </remarks>
        public UserInfo(string userName, int langId, int cultureId, Guid userId, string personName)
        {
            _userId = userId;
            _langId = langId;
            _userName = userName;
            _cultureId = cultureId;
            _personName = personName;
        }

        /// <summary>
        /// Gets the user ID.
        /// </summary>
        /// <value>The user ID.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-10    
        /// /// </remarks>
        public Guid UserId
        {
            get { return _userId; }
        }

        /// <summary>
        /// Gets the culture id of the user. If it is computer default (-1) then the client´s (browser) language is used.
        /// </summary>
        /// <value>The culture id.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-10    
        /// /// </remarks>
        public int CultureId
        {
            get
            {
                return _cultureId.Equals(-1) ? ClientLanguageId : _cultureId;
            }
        }

        /// <summary>
        /// Gets the language id of the user. If it is computer default (-1) then the client´s (browser) language is used.
        /// </summary>
        /// <value>The lang id.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-10    
        /// /// </remarks>
        public int LangId
        {
            get
            {
                return _langId.Equals(-1) ? ClientLanguageId : _langId;
            }
        }


        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-10    
        /// /// </remarks>
        public string UserName
        {
            get { return _userName; }
        }

        /// <summary>
        /// Gets the name of the person.
        /// </summary>
        /// <value>The name of the person.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-12    
        /// /// </remarks>
        public string PersonName
        {
            get { return _personName; }
        }

        private static int ClientLanguageId
        {
            get
            {
                if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length > 0)
                    return CultureInfo.CreateSpecificCulture(HttpContext.Current.Request.UserLanguages[0]).LCID;

                return CultureInfo.GetCultureInfo("en-US").LCID;
            }
        }
    }
}
