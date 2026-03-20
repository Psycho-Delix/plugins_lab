#include "pch.h"
#include "BaseHeader.h"
#include <stdio.h>
#include <omp.h>


// Имя плагина
DLLEXPORT const char* DemoPluginGetName()
{
	return "Коррекция красного цвета";
}
// Описание функции плагина
DLLEXPORT const char* DemoPluginGetDescription()
{
	return "Изменение значений канала красного цвета в модели RGB";
}

// Получение типа плагина (например для того чтобы понять куда встраивать в интерфейс)
DLLEXPORT const char* DemoPluginGetPluginType()
{
	return "IMG2IMG";
}

// Получение описания (конфигурация) GUI элементов
DLLEXPORT const char* DemoPluginGetGetGUIinfo(char* str)
{
	return "Label;L1;10;10;Модификация канала красного!TrackBar;INPUT_1;10;30;150;0;510;255;255;1!Label;LBINPUT_1;170;40;0!1";
}

// получение в строковом формате  GUID (globally unique identifier) - уникального 128-битного идентификатора.
DLLEXPORT const char* DemoPluginGetGUIDString()
{
	return "{1C1BE4AA-3A58-4F66-A4AD-4C6A0449DFE4}";
}

//Пример  метода обработки изображения в рамках которого модифицуируются значения канала красного
DLLEXPORT double DemoPluginDoWork(unsigned char* InIMG, unsigned char* OutIMG, const int Width, const int Heigth, const int Stride, char* str)
{
	double t_start = omp_get_wtime();
	// Определение таблици преобразования
	unsigned char LUTtr[256];
	int bt = Stride / Width;
	//bt = 3 означает работу с изображением 24бит
	//bt = 4 означает работу с изображением 32бит
	int offset = 0;
	// получение параметров конфигурации (для разных функций может быть много параметров)
	sscanf_s(str, "%d", &offset);

	for (int i = 0; i < 256; i++)
	{
		int tmp =  i + offset;
		LUTtr[i] = min(max(0, tmp), 255);
	}

	unsigned char r = 0, g = 0, b = 0;
	
	int y = 0;
	int pos = 0;
	// Обработка изображения
	for (int dy = 0; dy < Heigth; dy++)
	{
		// Рассчет положения по Y в памяти
		y = dy * Stride;
		for (int kx = 0; kx < Width; kx++)
		{
			// Рассчет положения пиксела по координатам [Y][X] в памяти при одномерной интерпретации.
			pos = kx * bt + y;
			// считывание значений цветов rgb
			b = InIMG[pos];
			g = InIMG[pos + 1];
			r = InIMG[pos + 2];
			// коррекция канала r
			r = LUTtr[int(r)];
			// запись значений в память нового изображения
			OutIMG[pos] = b;
			OutIMG[pos + 1] = g;
			OutIMG[pos + 2] = r;
		}
	}
	double t_end = omp_get_wtime();
	return ((t_end-t_start)*1000);
}