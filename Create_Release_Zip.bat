@ECHO OFF
"%ProgramFiles%\7-Zip\7z" a ..\GreyHak-DSP_Infinite_Resource_Nodes-9.zip icon.png LICENSE.txt manifest.json README.md
cd bin\Release
DEL patches\DSPInfiniteResourceNodes.dll
MKDIR patchers
COPY DSPInfiniteResourceNodes.dll patchers
"%ProgramFiles%\7-Zip\7z" u ..\..\..\GreyHak-DSP_Infinite_Resource_Nodes-9.zip patchers\DSPInfiniteResourceNodes.dll
DEL patchers\DSPInfiniteResourceNodes.dll
RMDIR patchers
