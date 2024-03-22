set unityDir="C:\Program Files\Unity2021.3.32f1\Editor\Unity.exe"
set projectDir="D:\Git\HOILAI-Galgame-Framework2"
set platform="WebGL"

%unityDir% -batchmode -quit -nographics -executeMethod XBuildCommandLine.BuildDevProject -projectPath %projectDir% -platform %platform%

pause