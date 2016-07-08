@ECHO OFF
SET /P VERSION_SUFFIX=Please enter version-suffix (can be left empty): 

dotnet "pack" "..\src\DapperWrapper" -c "Release" -o "." --version-suffix "%VERSION_SUFFIX%"
