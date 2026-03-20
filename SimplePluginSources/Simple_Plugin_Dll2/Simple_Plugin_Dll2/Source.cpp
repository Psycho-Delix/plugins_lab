#include "pch.h"
#include "BaseHeader.h"
#include <stdio.h>
#include <omp.h>


// Имя плагина
DLLEXPORT const char* DemoPluginGetName()
{
	return "Демо пункт меню";
}
// Описание функции плагина
DLLEXPORT const char* DemoPluginGetDescription()
{
	return "Демонстрация интеграции в меню с вызовом отображения сообщения";
}

// Получение типа плагина (например для того чтобы понять куда встраивать в интерфейс)
DLLEXPORT const char* DemoPluginGetPluginType()
{
	return "MSGBox";
}

// Получение описания (конфигурация) GUI элементов
DLLEXPORT const char* DemoPluginGetGetGUIinfo(char* str)
{
	return "";
}

// Получение в строковом формате  GUID (globally unique identifier) - уникального 128-битного идентификатора.
DLLEXPORT const char* DemoPluginGetGUIDString()
{
	return "{F88D8EBB-AD8B-4DA2-816C-C7DC76EEC9CE}";
}


// Метод выполнения работы (передача сообщения)
DLLEXPORT const char* DemoPluginDoWork()
{
	return "Демонстрационное сообщение из модуля расширения";
}