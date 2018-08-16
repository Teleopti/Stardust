Set-Location $PSScriptRoot\..\WFM

Write-Output "Removing non-dist files"
Remove-Item @(
    ".browserlistrc",
    ".eslintrc",
    ".postcssrc",
    ".prettierignore",
    ".prettierrc.json",
    "angular.json",
    "Gruntfile.js",
    "index.tpl.html",
    "index_desktop_client.tpl.html",
    "package.json",
    "package-lock.json",
    "README.md",
    "styleguide_translation_keys.html",
    "tsconfig.json",
    "tslint.json",
    "app",
    "css",
    "e2e",
    "html",
    "src",
    "test"
) -Recurse -Force -Verbose

Write-Output "Removing .map files from dist"
Remove-Item .\dist\ -Recurse -Include *.map

Write-Output "Removing node_modules"
Remove-Item "node_modules" -Recurse -Force

Set-Location $PSScriptRoot