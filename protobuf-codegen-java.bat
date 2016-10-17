@ECHO OFF

SET CODEGENTOOL="tools\codegen\protobuf\google\protoc.exe"

FOR %%i IN (Tests\Redola.Rpc.TestContracts\*.proto) DO (CALL :codegen %%i)

GOTO :done

:codegen
  SET protofile=%1
  ECHO %CODEGENTOOL% --java_out=. %protofile%
  %CODEGENTOOL% --java_out=. %protofile%
  GOTO :EOF

:done
ECHO DONE!
