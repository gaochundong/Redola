@ECHO OFF

SET CODEGENTOOL="tools\codegen\protobuf\dotnet\protogen.exe"

FOR %%i IN (Tests\Redola.Rpc.TestContracts\*.proto) DO (CALL :codegen %%i)

GOTO :done

:codegen
  SET protofile=%1
  SET csfile=%protofile:proto=cs%
  SET csfile=%csfile:cscol=Protocol%
  ECHO %CODEGENTOOL% -i:%protofile% -o:%csfile% -q
  %CODEGENTOOL% -i:%protofile% -o:%csfile% -q
  GOTO :EOF

:done
ECHO DONE!
