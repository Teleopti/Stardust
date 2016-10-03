(function() {
﻿    'use strict';
﻿
﻿    angular
﻿        .module('currentUserInfoService')
﻿        .factory('Settings', Settings);
﻿
﻿    Settings.$inject = ['$http'];
﻿
﻿    function Settings($http) {
﻿        var service = {
﻿            init: init,
﻿        };
﻿
﻿        return service;
﻿
﻿        function init() {
          return $http.get('../api/Settings/SupportEmail').success(function (data) {
              service.supportEmailSetting = data ? data : 'ServiceDesk@teleopti.com';
          });
﻿        }
﻿    }
﻿})();
