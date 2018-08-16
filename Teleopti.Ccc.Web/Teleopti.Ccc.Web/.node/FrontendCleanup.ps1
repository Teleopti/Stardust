$files = '.angular-cli.json',`
         '.browserlistrc',`
         '.eslintrc',`
         '.prettierrc.json',`
         'gruntfile.js',`
         'index.tpl.html',`
         'index.webpack.tpl.html',`
         'index_desktop_client.tpl.html',`
         'karma.conf.js',`
         'manifest.webmanifest.json',`
         'protractor.conf.js',`
         'readme.md',`
         'tsconfig.json',`
         'tslint.json',`
         'webpack.config.js',`
         'yarn.lock',`
         'package.json',`
         'package-lock.json'

$relpath = "\Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM"
$delpath = $env:WorkingDirectory + $relpath

gci -Path "$delpath" | Remove-Item -Include $files -recurse -Force -Verbose


$delFolder1 = $delpath + "\app"
$delFolder2 = $delpath + "\src"
Remove-Item $delFolder1 -Recurse -Force -verbose
Remove-Item $delFolder2 -Recurse -Force -verbose