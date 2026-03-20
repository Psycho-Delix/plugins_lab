library Simple_PluginForm;

{ Important note about DLL memory management: ShareMem must be the
  first unit in your library's USES clause AND your project's (select
  Project-View Source) USES clause if your DLL exports any procedures or
  functions that pass strings as parameters or function results. This
  applies to all strings passed to and from your DLL--even those that
  are nested in records and classes. ShareMem is the interface unit to
  the BORLNDMM.DLL shared memory manager, which must be deployed along
  with your DLL. To avoid using BORLNDMM.DLL, pass string information
  using PChar or ShortString parameters. }

uses
  SysUtils,
  Classes, Forms,
  Dform in 'Dform.pas' { DemoForm } ;
{$R *.res}



function DemoPluginGetName(): PAnsiChar; cdecl;
begin
  result := 'Запуск формы';
end;

function DemoPluginGetDescription(): PAnsiChar; cdecl;
begin
  result := 'Демонстрация открытия формы';
end;

function DemoPluginGetPluginType(): PAnsiChar; cdecl;
begin
  result := 'DForm';
end;

function DemoPluginGetGUIDString(): PAnsiChar; cdecl;
begin
  result := '{73530DCA-972C-43E3-A8C9-4E890957F6D3}';
end;

function DemoPluginGetGetGUIinfo(): PAnsiChar; cdecl;
begin
  result := '';
end;


function DemoPluginDoWork(): PAnsiChar; cdecl;
var
formprt:Pointer;
begin
  formprt:=TDemoForm.Create(nil);
  TDemoForm(formprt).ShowModal;
  TDemoForm(formprt).Free;
  result := 'ok';
end;


 { Указывается какие функции экспортируются }
 exports
   DemoPluginGetName,
   DemoPluginGetDescription,
   DemoPluginGetPluginType,
   DemoPluginGetGUIDString,
   DemoPluginGetGetGUIinfo,
   DemoPluginDoWork;

begin
 end.
