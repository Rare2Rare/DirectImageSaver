#ifndef PayloadDir
  #error "PayloadDir is not defined."
#endif

#ifndef OutputDir
  #error "OutputDir is not defined."
#endif

#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif

#define MyAppName "DirectImageSaver"
#define MyAppPublisher "Rare2Rare"
#define MyAppExeName "DirectImageSaver.App.exe"
#define MyExtensionId "kblklkfadcpplofmmfkkplglcmomicmm"
#define MyFirefoxExtensionId "directimagesaver@rare2rare"
#define MyHostName "com.directimagesaver.host"

[Setup]
AppId={{B11E3C43-74A9-4D17-9A54-7FD8DF8B6EF2}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\DirectImageSaver\current
DisableDirPage=no
DisableProgramGroupPage=yes
DisableWelcomePage=no
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
OutputDir={#OutputDir}
OutputBaseFilename=DirectImageSaver-Setup
SetupIconFile={#PayloadDir}\app\Assets\direct-image-saver.ico
UninstallDisplayIcon={app}\app\DirectImageSaver.App.exe
CloseApplications=yes

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[CustomMessages]
japanese.WelcomeLabel2=DirectImageSaver をインストールします。%n%nブラウザ上の画像や直リンク動画を即保存できるツールです。%n%n続行するには「次へ」をクリックしてください。

[Files]
Source: "{#PayloadDir}\app\*"; DestDir: "{app}\app"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PayloadDir}\nativehost\*"; DestDir: "{app}\nativehost"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PayloadDir}\extension\*"; DestDir: "{app}\extension"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PayloadDir}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\QUICKSTART.ja.md"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Registry]
Root: HKCU; Subkey: "Software\Google\Chrome\NativeMessagingHosts\{#MyHostName}"; ValueType: string; ValueName: ""; ValueData: "{app}\nativehost\{#MyHostName}.json"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Edge\NativeMessagingHosts\{#MyHostName}"; ValueType: string; ValueName: ""; ValueData: "{app}\nativehost\{#MyHostName}.json"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Mozilla\NativeMessagingHosts\{#MyHostName}"; ValueType: string; ValueName: ""; ValueData: "{app}\nativehost\{#MyHostName}.firefox.json"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "DirectImageSaver"; ValueData: """{app}\app\{#MyAppExeName}"" --background"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\app\{#MyAppExeName}"; Parameters: "--show-onboarding"; Description: "DirectImageSaver を起動"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
procedure WriteNativeHostManifest();
var
  ManifestPath: string;
  NativeHostPath: string;
  EscapedNativeHostPath: string;
  ManifestJson: string;
begin
  ManifestPath := ExpandConstant('{app}\nativehost\{#MyHostName}.json');
  NativeHostPath := ExpandConstant('{app}\nativehost\DirectImageSaver.NativeHost.exe');
  EscapedNativeHostPath := NativeHostPath;
  StringChangeEx(EscapedNativeHostPath, '\', '\\', True);

  ManifestJson :=
    '{'#13#10 +
    '  "name": "{#MyHostName}",'#13#10 +
    '  "description": "DirectImageSaver Native Messaging Host",'#13#10 +
    '  "path": "' + EscapedNativeHostPath + '",'#13#10 +
    '  "type": "stdio",'#13#10 +
    '  "allowed_origins": ['#13#10 +
    '    "chrome-extension://{#MyExtensionId}/"'#13#10 +
    '  ]'#13#10 +
    '}'#13#10;

  SaveStringToFile(ManifestPath, ManifestJson, False);
end;

procedure WriteFirefoxNativeHostManifest();
var
  ManifestPath: string;
  NativeHostPath: string;
  EscapedNativeHostPath: string;
  ManifestJson: string;
begin
  ManifestPath := ExpandConstant('{app}\nativehost\{#MyHostName}.firefox.json');
  NativeHostPath := ExpandConstant('{app}\nativehost\DirectImageSaver.NativeHost.exe');
  EscapedNativeHostPath := NativeHostPath;
  StringChangeEx(EscapedNativeHostPath, '\', '\\', True);

  ManifestJson :=
    '{'#13#10 +
    '  "name": "{#MyHostName}",'#13#10 +
    '  "description": "DirectImageSaver Native Messaging Host",'#13#10 +
    '  "path": "' + EscapedNativeHostPath + '",'#13#10 +
    '  "type": "stdio",'#13#10 +
    '  "allowed_extensions": ['#13#10 +
    '    "{#MyFirefoxExtensionId}"'#13#10 +
    '  ]'#13#10 +
    '}'#13#10;

  SaveStringToFile(ManifestPath, ManifestJson, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    WriteNativeHostManifest();
    WriteFirefoxNativeHostManifest();
  end;
end;
