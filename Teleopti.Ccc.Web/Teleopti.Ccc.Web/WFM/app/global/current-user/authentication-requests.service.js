(function() {
﻿    'use strict';
﻿
﻿    angular
﻿        .module('currentUserInfoService')
﻿        .factory('AuthenticationRequests', AuthenticationRequests);
﻿
﻿    AuthenticationRequests.$inject = ['$http'];
﻿
﻿    function AuthenticationRequests($http) {
﻿        var service = {
﻿            getCurrentUser: getCurrentUser
﻿        };
﻿
﻿        return service;
﻿
﻿        function getCurrentUser() {
﻿           return $http.get('../api/Global/User/CurrentUser');
﻿        }
﻿    }
﻿})();
