@ECHO OFF

SET CODEGENTOOL="tools\codegen\protobuf\google\protoc.exe"

FOR %%i IN (Tests\Redola.Rpc.TestContracts\*.proto) DO (CALL :codegen %%i)

GOTO :done

:codegen
  SET protofile=%1
  ECHO %CODEGENTOOL% --python_out=. %protofile%
  %CODEGENTOOL% --python_out=. %protofile%
  GOTO :EOF

:done
ECHO DONE!
