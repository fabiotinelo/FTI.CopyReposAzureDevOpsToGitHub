@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

:: Prompt for confirmation before deletion
set /p confirm=Are you sure you want to delete all 'bin' and 'obj' folders in the current directory and all subdirectories? (Y/N) 

if /i not "!confirm!"=="Y" (
    echo Operation cancelled.
    exit /b
)

:: Function to delete folders
:DeleteFolders
for /d /r %%d in (bin, obj) do (
    if exist "%%d" (
        echo Deleting "%%d"
        rd /s /q "%%d"
    )
)
echo All 'bin' and 'obj' folders deleted.
pause
ENDLOCAL
