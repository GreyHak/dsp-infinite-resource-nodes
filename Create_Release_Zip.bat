@ECHO OFF
"%ProgramFiles%\7-Zip\7z" a ..\GreyHak-DSP_Infinite_Resource_Nodes-9.zip icon.png LICENSE.txt manifest.json README.md
cd bin\Release
"%ProgramFiles%\7-Zip\7z" u ..\..\..\GreyHak-DSP_Infinite_Resource_Nodes-9.zip DSPInfiniteResourceNodes.dll
