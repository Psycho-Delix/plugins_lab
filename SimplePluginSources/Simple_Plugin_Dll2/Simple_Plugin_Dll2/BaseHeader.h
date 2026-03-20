#pragma once
// Экспорт Функций
#define DLLEXPORT extern "C" __declspec(dllexport)

// Простые интерфейсные функции
// Имя плагина
DLLEXPORT const char* DemoPluginGetName();
// Описание функции плагина
DLLEXPORT const char* DemoPluginGetDescription();
// Получение типа плагина (например для того чтобы понять куда встраивать в интерфейс)
DLLEXPORT const char* DemoPluginGetPluginType();
// Получение описания (конфигурация) GUI элементов
DLLEXPORT const char* DemoPluginGetGetGUIinfo(char* str);
// Получение в строковом формате  GUID (globally unique identifier) - уникального 128-битного идентификатора.
DLLEXPORT const char* DemoPluginGetGUIDString();
// Метод выполнения работы
DLLEXPORT const char* DemoPluginDoWork();
