:: Runs from [repo]\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM

call ..\.node\npm run continuous

:: Clear all dev dependencies and only install what we need for production
call ..\.node\npm install rimraf
call ..\.node\node node_modules\bin.js node_modules
call ..\.node\npm install --production
