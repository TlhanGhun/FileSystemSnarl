!addplugindir ".\"

!include "MUI2.nsh"
!include "checkDotNet3.nsh"
!include LogicLib.nsh
!include UAC.nsh

!define MIN_FRA_MAJOR "3"
!define MIN_FRA_MINOR "5"
!define MIN_FRA_BUILD "*"


; The name of the installer
Name "FileSystemSnarl"

; The file to write
OutFile "Setup-FileSystemSnarl.exe"





; The default installation directory
InstallDir "$PROGRAMFILES\Tlhan Ghun\FileSystemSnarl"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\FileSystemSnarl" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel user

Function .onInit
uac_tryagain:
!insertmacro UAC_RunElevated
#MessageBox mb_TopMost "0=$0 1=$1 2=$2 3=$3"
${Switch} $0
${Case} 0
	${IfThen} $1 = 1 ${|} Quit ${|} ;we are the outer process, the inner process has done its work, we are done
	${IfThen} $3 <> 0 ${|} ${Break} ${|} ;we are admin, let the show go on
	${If} $1 = 3 ;RunAs completed successfully, but with a non-admin user
		MessageBox mb_IconExclamation|mb_TopMost|mb_SetForeground "This installer requires admin access, try again" /SD IDNO IDOK uac_tryagain IDNO 0
	${EndIf}
	;fall-through and die
${Case} 1223
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "This installer requires admin privileges, aborting!"
	Quit
${Case} 1062
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Logon service not running, aborting!"
	Quit
${Default}
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Unable to elevate , error $0"
	Quit
${EndSwitch}
FunctionEnd
 


;--------------------------------

  !define MUI_ABORTWARNING



!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "logoSetupSmall.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "logoSetupBig.bmp"
!define MUI_WELCOMEPAGE_TITLE "FileSystemSnarl"
!define MUI_WELCOMEPAGE_TEXT "FileSystemSnarl is a small tool which monitors folders and sends Snarl notifications when files are changed$\r$\n$\r$\nPlease stop any instance of FileSystemSnarl prior to installing this version."
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "Tlhan Ghun\FileSystemSnarl"
!define MUI_ICON "..\FileSystemSnarl.ico"
!define MUI_UNICON "uninstall.ico"


Var StartMenuFolder
; Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY

  !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
  !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\FileSystemSnarl" 
  !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

  !insertmacro MUI_PAGE_INSTFILES
   !define MUI_FINISHPAGE_RUN
  !define MUI_FINISHPAGE_RUN_FUNCTION FinishRun   
  !insertmacro MUI_PAGE_FINISH

Function FinishRun
!insertmacro UAC_AsUser_ExecShell "" "FileSystemSnarl.exe" "" "" ""
FunctionEnd


  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH





;--------------------------------




!insertmacro MUI_LANGUAGE "English"

; LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
;--------------------------------
;Version Information

  VIProductVersion "2.0.0.0"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "FileSystemSnarl"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Tlhan Ghun"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© 2009 - 2012"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "FileSystemSnarl"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "2.0"







Function un.UninstallDirs
    Exch $R0 ;input string
    Exch
    Exch $R1 ;maximum number of dirs to check for
    Push $R2
    Push $R3
    Push $R4
    Push $R5
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     StrCpy $R5 0
    top:
     StrCpy $R2 0
     StrLen $R4 $R0
    loop:
     IntOp $R2 $R2 + 1
      StrCpy $R3 $R0 1 -$R2
     StrCmp $R2 $R4 exit
     StrCmp $R3 "\" 0 loop
      StrCpy $R0 $R0 -$R2
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     IntOp $R5 $R5 + 1
     StrCmp $R5 $R1 exit top
    exit:
    Pop $R5
    Pop $R4
    Pop $R3
    Pop $R2
    Pop $R1
    Pop $R0
FunctionEnd









; The stuff to install
Section "FileSystemSnarl"

  SectionIn RO
  
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  !insertmacro AbortIfBadFramework

  ; Put file there
  File "Documentation.URL"
  File "..\FileSystemSnarl.exe"
  File "..\FileSystemSnarl.pdb"
  File "..\FileSystemSnarl.exe.config"
  File "..\FileSystemSnarl.ico"
  File "LICENSE.txt"
  File "Documentation.ico"
  
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\FileSystemSnarl "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FileSystemSnarl" "DisplayName" "FileSystemSnarl"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FileSystemSnarl" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FileSystemSnarl" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FileSystemSnarl" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

    Push $R0
   ClearErrors
   ReadRegDword $R0 HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{9A25302D-30C0-39D9-BD6F-21E6EC160475}" "Version"



  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

!insertmacro MUI_STARTMENU_WRITE_BEGIN Application
SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\FileSystemSnarl.lnk" "$INSTDIR\FileSystemSnarl.exe" "" "$INSTDIR\FileSystemSnarl.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Documentation.lnk" "$INSTDIR\Documentation.URL" "" $INSTDIR\Documentation.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  SetShellVarContext current
!insertmacro MUI_STARTMENU_WRITE_END

  
SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"

  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FileSystemSnarl"
  DeleteRegKey HKLM "Software\FileSystemSnarl"
  ; Remove files and uninstaller
  Delete $INSTDIR\*.*

  ; Remove shortcuts, if any
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
    SetShellVarContext all
  Delete "$SMPROGRAMS\$StartMenuFolder\\*.*"
  SetShellVarContext current


  DeleteRegKey HKCU "Software\FileSystemSnarl"


  ; Remove directories used
   ; RMDir "$SMPROGRAMS\$StartMenuFolder"
Push 10 #maximum amount of directories to remove
  Push "$SMPROGRAMS\$StartMenuFolder" #input string
    Call un.UninstallDirs

   
  ; RMDir "$INSTDIR"
  
  Push 10 #maximum amount of directories to remove
  Push $INSTDIR #input string
    Call un.UninstallDirs


SectionEnd
