@echo off
setlocal enabledelayedexpansion

set isUpdateCSharp=false
set isUpdateLua=false
set isUpdatePrefab=false
set isUpdateOther=false
if not exist .\OldSvnVersion.txt (
	echo %svnVersion% > .\OldSvnVersion.txt
	set isUpdateCSharp=true
	set isUpdateLua=true
	set isUpdatePrefab=true
	set isUpdateOther=true
) else (
	for /f %%a in ('type .\OldSvnVersion.txt') do (
		echo SvnUrl:%svnUrl%
		echo SvnVersion:%svnVersion%
		set /a OldSvnVersion=%%a+1
		echo OldSvnVersion:!OldSvnVersion!
		for /f "delims=" %%b in ('svn log %svnUrl% -r %svnVersion%:!OldSvnVersion! -v') do (
			set tempUpdateCSharp=false
			set tempUpdateLua=false
			set tempUpdatePrefab=false
			for /f "delims=" %%c in ('echo "%%b" ^| findstr /r ".\.cs""" "./ProjectSettings/." "./Resources/."') do (
				if !isUpdateCSharp! == false (
					echo CSharp:%%b
				)
				set isUpdateCSharp=true
				set tempUpdateCSharp=true
			)
			if !tempUpdateCSharp! == false (
				for /f "delims=" %%c in ('echo "%%b" ^| findstr /r ".\.lua"""') do (
					if !isUpdateLua! == false (
						echo Lua:%%b
					)
					set isUpdateLua=true
					set tempUpdateLua=true
				)
			)
			if !tempUpdateCSharp! == false if !tempUpdateLua! == false (
				for /f "delims=" %%c in ('echo "%%b" ^| findstr /r "./Assets/GUI/."') do (
					if !isUpdatePrefab! == false (
						echo Prefab:%%b
					)
					set isUpdatePrefab=true
					set tempUpdatePrefab=true
				)
			)
			if !tempUpdateCSharp! == false if !tempUpdateLua! == false if !tempUpdatePrefab! == false (
				for /f "delims=" %%c in ('echo "%%b" ^| findstr /r "./Development/."') do (
					if !isUpdateOther! == false (
						echo Other:%%b
					)
					set isUpdateOther=true
				)
			)
		)
	)
)

set currentPath=%cd%
set buildLogPath=%currentPath%\log.txt
set OldSvnVersionPath=%currentPath%\OldSvnVersion.txt

cd ../

svn revert -R .

set projectDir=%cd%

set unityDir="C:\Program Files\Unity2018.4.8f1\Editor\Unity.exe"

echo projectDir:%projectDir%
echo unityDir:%unityDir%
echo version:%svnVersion%
echo buildPath:%buildPath%
echo webPath:%webPath%
echo platform:%platform%
echo forceRebuild:%forceRebuild%
echo isClear:%isClear%
echo isUpdateCSharp:!isUpdateCSharp!
echo isUpdateLua:!isUpdateLua!
echo isUpdatePrefab:!isUpdatePrefab!
echo isUpdateOther:!isUpdateOther!

cd.>%buildLogPath%

PowerShell -Command "& {$temp=Start-Process %currentPath%\tail.exe -ArgumentList '-f  %buildLogPath%' -NoNewWindow -PassThru;$temp.id > %currentPath%/temp}"
for /f %%a in ('type %currentPath%\temp') do ( set pid=%%a&&del /q %currentPath%\temp)

%unityDir% -batchmode -quit -nographics -executeMethod XBuildCommandLine.BuildDevProject -logFile %buildLogPath% -projectPath %projectDir% -version %svnVersion% -buildPath %buildPath% -platform %platform% -forceRebuild %forceRebuild% -isClear %isClear% -isUpdateCSharp !isUpdateCSharp! -isUpdateLua !isUpdateLua! -isUpdatePrefab !isUpdatePrefab! -isUpdateOther !isUpdateOther!

IF ERRORLEVEL 1 goto error
IF ERRORLEVEL 0 goto success
:error
echo error

taskkill /pid %pid% -f

exit /b 123456789
:success

set devpath1=%buildPath%\%platform%\00\version.txt
set devpath2=%buildPath%\%platform%\01\version.txt
set artpath=%buildPath%\%platform%\02\version.txt

if exist %devpath1% (
	if exist %devpath2% (
		if exist %artpath% (
			echo d|xcopy %buildPath%\%platform%  %webPath%\%platform% /s /e /y
			echo f|xcopy %projectDir%\out.apk  %webPath%\%platform%\out.apk /y
			echo f|xcopy %projectDir%\out_il2cpp.apk  %webPath%\%platform%\out_il2cpp.apk /y
			echo f|xcopy %projectDir%\out.7z  %webPath%\%platform%\out.7z /y
			goto end			
		) else (
			goto error
		)
	) else (
		goto error
	)
	 else (
		goto error
	)
)

:end
echo %svnVersion% > %OldSvnVersionPath%
taskkill /pid %pid% -f
