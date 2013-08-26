@ECHO OFF
hg log -l 1 --template {node} > %1