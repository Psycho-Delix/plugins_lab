using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Linq;
using System.Globalization;


[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr TIMG_info_plugin();


[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate double TIMG_proc_img_plugin(IntPtr Inmas, IntPtr Outmas, int width, int height, int stride, IntPtr cfg);


public class PluginFDetails
{
    public IntPtr PluginLibHandle;
    public string PluginGUID;
    public string PluginName;
    public string PluginDescription;
    public string PluginType;
    public string PluginGUIconfig;
    public IntPtr PluginDoWorkFunction;
}

public class Plugin
{
    // Параметры для проверки построителя интерфейса
    private int fPFParamCNT;
    private string fPrewPluginFunctionName;

    // Параметры для вещественных значений в трекбарах
    private Dictionary<string, int> Offcets;
    private Dictionary<string, int> Divizors;

    // Перечень функций из плагинов
    private Dictionary<string, string> FLoaded_GUIDPluginsList;
    private Dictionary<string, PluginFDetails> FPluginsList;

    // Поле для построение интерфейса (можно переделать панель на иное это не имеет значение)
    public Panel SettingsField;
    // Перечень функций для обработки картинок
    public ListBox FaListBox { get; set; }
    public ListBox PListBox { get; set; }
    // Место где будут добавлятся элементы меню
    public ToolStripMenuItem PluginsMenu { get; set; }
    // Время выполнения обработки изображения (ms)
    public double Time;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpLibFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    public Plugin()
    {
        Offcets = new Dictionary<string, int>();
        Divizors = new Dictionary<string, int>();
        FPluginsList = new Dictionary<string, PluginFDetails>();
        FLoaded_GUIDPluginsList = new Dictionary<string, string>();
    }

    public void Dispose()
    {
        foreach (var key in FPluginsList.Keys)
        {
            FreeLibrary(FPluginsList[key].PluginLibHandle);
        }
        FPluginsList.Clear();
        GC.SuppressFinalize(this);
    }

    ~Plugin()
    {
        Dispose();
    }

    public void ListPluginFileDir(string path, string mask, List<string> fileList)
    {
        try
        {
            foreach (var file in Directory.GetFiles(path, mask))
            {
                // Проверяем, является ли файл плагином
                if (VerifyPluginFile(file))
                {
                    fileList.Add(Path.GetFileName(file));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при поиске плагинов: " + ex.Message);
        }
    }

    /// Проверка является ли файл модулем расширения (плагином)
    public bool VerifyPluginFile(string filename)
    {
        IntPtr libHandle = IntPtr.Zero;
        bool result = false;

        try
        {
            libHandle = LoadLibrary(filename);
            if (libHandle != IntPtr.Zero)
            {
                // Проверка наличия нужных функций в библиотеке
                if (GetProcAddress(libHandle, "DemoPluginGetName") != IntPtr.Zero &&
                    GetProcAddress(libHandle, "DemoPluginGetDescription") != IntPtr.Zero &&
                    GetProcAddress(libHandle, "DemoPluginGetPluginType") != IntPtr.Zero &&
                    GetProcAddress(libHandle, "DemoPluginGetGUIDString") != IntPtr.Zero &&
                    GetProcAddress(libHandle, "DemoPluginGetGetGUIinfo") != IntPtr.Zero &&
                    GetProcAddress(libHandle, "DemoPluginDoWork") != IntPtr.Zero)
                {
                    result = true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при загрузке плагина: " + ex.Message);
        }
        finally
        {
            if (libHandle != IntPtr.Zero)
            {
                FreeLibrary(libHandle);
            }
        }

        return result;
    }

    // Загрузка модуля расширений
    public bool LoadPluginFunction(string filename)
    {
        IntPtr thLib = IntPtr.Zero;
        bool isAdded = false;
        bool result = false;

        if (thLib == IntPtr.Zero)
            thLib = LoadLibrary(filename);

        if (thLib != IntPtr.Zero)
        {
            TIMG_info_plugin GetPluginData = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                GetProcAddress(thLib, "DemoPluginGetGUIDString"), typeof(TIMG_info_plugin));

            string guidStr = string.Empty;
            if (GetPluginData != null)
            {
                guidStr = Marshal.PtrToStringAnsi(GetPluginData());
                isAdded = FLoaded_GUIDPluginsList.TryGetValue(guidStr, out string searchFileName);
            }

            if (!isAdded)
            {


                FLoaded_GUIDPluginsList.Add(guidStr, filename);
                result = true;

                PluginFDetails pfItem = new PluginFDetails();
                pfItem.PluginLibHandle = thLib;
                pfItem.PluginGUID = guidStr;

                TIMG_info_plugin GFPN;
                // Получаем имя функции плагина
                GFPN = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    GetProcAddress(thLib, "DemoPluginGetName"), typeof(TIMG_info_plugin));
                pfItem.PluginName = GFPN != null ? Marshal.PtrToStringAnsi(GFPN()) : string.Empty;
                // Получаем описание функции плагина
                GFPN = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    GetProcAddress(thLib, "DemoPluginGetDescription"), typeof(TIMG_info_plugin));
                pfItem.PluginDescription = GFPN != null ? Marshal.PtrToStringAnsi(GFPN()) : string.Empty;
                // Получаем тип плагина
                GFPN = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    GetProcAddress(thLib, "DemoPluginGetPluginType"), typeof(TIMG_info_plugin));
                pfItem.PluginType = GFPN != null ? Marshal.PtrToStringAnsi(GFPN()) : string.Empty;
                // Получаем информацию о GUI плагина
                GFPN = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    GetProcAddress(thLib, "DemoPluginGetGetGUIinfo"), typeof(TIMG_info_plugin));
                pfItem.PluginGUIconfig = GFPN != null ? Marshal.PtrToStringAnsi(GFPN()) : string.Empty;
                // Получаем указатель на рабочую функцию плагина
                pfItem.PluginDoWorkFunction = GetProcAddress(thLib, "DemoPluginDoWork");
                // Добавляем плагин в список
                FPluginsList.Add(pfItem.PluginName, pfItem);
                if (pfItem.PluginType == "IMG2IMG")
                {
                    FaListBox.Items.Add(pfItem.PluginName);
                }
                if (pfItem.PluginType == "MSGBox" || pfItem.PluginType == "DForm")
                {
                    ToolStripMenuItem elm = new ToolStripMenuItem
                    {
                        Name = pfItem.PluginName.Replace(" ", "_"),
                        Text = pfItem.PluginName
                    };
                    elm.Click += MenuElClick;
                    PluginsMenu.DropDownItems.Add(elm);
                }



            }
        }
        return result;
    }

    public void LoadPlugins(string path, string mask, List<string> pluginFilesList)
    {
        // Создание списка файлов и поиск модулей (плагинов по маске)
        string[] files = Directory.GetFiles(path, mask);
        // Перебираем все плагины
        foreach (string file in files)
        {
            // делаем проверку с одновременной загрузкой
            if (LoadPluginFunction(file))
            {
                // в случае умпешной загрузки добовляем файл в список загруженых плагинов
                pluginFilesList.Add(file);
            }
        }
    }

    public void CreatePluginFunctionSettings(string fPluginName)
    {
        if (SettingsField != null && FPluginsList.TryGetValue(fPluginName, out PluginFDetails PFItem))
        {
            // если выбираемая функция отличается от того что было ранее
            if (fPrewPluginFunctionName != PFItem.PluginName)
            {
                // удаляем элементы GUI
                ClearSettingsField();
                // в случае наличия конфигурации (длинна более 10 символов)
                if (PFItem.PluginGUIconfig.Length > 10)
                {
                    // генерируем новые элементы интерфейса (GUI) на основе
                    // строки конфигурации
                    CreateSettingsField(PFItem.PluginGUIconfig);
                }
            }
            fPrewPluginFunctionName = PFItem.PluginName;
        }
    }

    public void ApplyPluginIMG2IMG(string pluginName, Bitmap inBitmap, ref Bitmap outBitmap)
    {
        if (FPluginsList.TryGetValue(pluginName, out PluginFDetails pfItem))
        {
            string setting = ParseSettings();

            // создаем временный Bitmap для входного изображения
            Bitmap TMPBitmapIn = null;
            TMPBitmapIn = new Bitmap(inBitmap.Width, inBitmap.Height,
                   System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(TMPBitmapIn))
            {
                gr.DrawImage(inBitmap, new Rectangle(0, 0, TMPBitmapIn.Width, TMPBitmapIn.Height));
            }
            Rectangle rect = new Rectangle(0, 0, TMPBitmapIn.Width, TMPBitmapIn.Height);
            System.Drawing.Imaging.BitmapData btData = TMPBitmapIn.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, TMPBitmapIn.PixelFormat);
            // создание байтовых массивов
            IntPtr ptr = btData.Scan0;
            int stride = btData.Stride;
            int bytes = TMPBitmapIn.Height * Math.Abs(btData.Stride);
            byte[] bitmapIn = new byte[bytes];
            byte[] bitmapOut = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, bitmapIn, 0, bytes);

            TMPBitmapIn.UnlockBits(btData);

            // Проверяем наличие обработчика в плагине
            if (pfItem.PluginDoWorkFunction != IntPtr.Zero)
            {
                try
                {
                    TIMG_proc_img_plugin pluginF = (TIMG_proc_img_plugin)Marshal.GetDelegateForFunctionPointer(
                        pfItem.PluginDoWorkFunction, typeof(TIMG_proc_img_plugin));

                    // Запускаем обработку
                    Time = pluginF(Marshal.UnsafeAddrOfPinnedArrayElement(bitmapIn, 0), Marshal.UnsafeAddrOfPinnedArrayElement(bitmapOut, 0), inBitmap.Width, inBitmap.Height, stride,
                        Marshal.StringToHGlobalAnsi(setting));
                }
                finally
                {
                    // Разблокируем изображения
                    outBitmap = new Bitmap(inBitmap.Width, inBitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    System.Drawing.Imaging.BitmapData bmpData = outBitmap.LockBits(
                                 new Rectangle(0, 0, outBitmap.Width, outBitmap.Height),
                                 System.Drawing.Imaging.ImageLockMode.WriteOnly, outBitmap.PixelFormat);
                    // Копирование данных из массива байтов в BitmapData.Scan0
                    Marshal.Copy(bitmapOut, 0, bmpData.Scan0, bitmapOut.Length);
                    // Разблокировка пикселей
                    outBitmap.UnlockBits(bmpData);
                }
            }
        }
    }

    private void TrackbarPos(object sender, EventArgs e)
    {
        if (sender is TrackBar trackBar)
        {
            string cnam = "LB" + trackBar.Name;
            Control comp = SettingsField.Controls.Find(cnam, true).FirstOrDefault();

            if (comp is Label label)
            {
                int offset = Offcets[trackBar.Name];
                int divisor = Divizors[trackBar.Name];
                label.Text = ((float)trackBar.Value / divisor - offset).ToString();
            }
        }
    }

    public void ClearSettingsField()
    {
        for (int i = SettingsField.Controls.Count - 1; i >= 0; i--)
        {
            var item = SettingsField.Controls[i];
            item.Dispose();
        }
    }

    public void CreateSettingsField(string cfg)
    {
        ClearSettingsField();
        string str = cfg;
        // Создание списков для загрузки конфигурации интерфейса
        var list1 = new List<string>();
        var list2 = new List<string>();

        /// заполнение информации о создаваемых компонентах в качестве разделителя !
        list1 = str.Split('!').ToList();
        list2 = new List<string>();

        if (Offcets != null)
            Offcets.Clear();
        if (Divizors != null)
            Divizors.Clear();
        // создание GUI на основе описательной схемы из DLL
        foreach (var item in list1)
        {
            // заполнение информации о параметрах компонента согласно разделителя ;
            list2 = item.Split(';').ToList();
            // Если параметров больше 4 то это описние компонента
            if (list2.Count > 4)
            {
                if (list2[0] == "Label")
                {
                    // Создание Label'ов
                    var lb = new Label();
                    lb.Name = list2[1];
                    lb.Parent = SettingsField;
                    lb.Left = int.Parse(list2[2]);
                    lb.Top = int.Parse(list2[3]);
                    lb.Text = list2[4];
                    lb.AutoSize = true;
                }
                if (list2[0] == "Edit")
                {
                    // Создание Edit'ов т.н. TextBox элементов C#
                    var ed = new TextBox();
                    ed.Name = list2[1];
                    ed.Parent = SettingsField;
                    ed.Left = int.Parse(list2[2]);
                    ed.Top = int.Parse(list2[3]);
                    ed.Width = int.Parse(list2[4]);
                    ed.Text = list2[5];
                }
                if (list2[0] == "TrackBar")
                {
                    // Создание TrackBar'ов
                    var tb = new TrackBar();
                    tb.Name = list2[1];
                    tb.Parent = SettingsField;
                    tb.Left = int.Parse(list2[2]);
                    tb.Top = int.Parse(list2[3]);
                    tb.Width = int.Parse(list2[4]);
                    tb.Minimum = int.Parse(list2[5]);
                    tb.Maximum = int.Parse(list2[6]);
                    tb.Value = int.Parse(list2[7]);
                    tb.LargeChange = 15;
                    tb.TickStyle = TickStyle.Both;
                    Offcets.Add(tb.Name, int.Parse(list2[8]));
                    Divizors.Add(tb.Name, int.Parse(list2[9]));
                    tb.Scroll += TrackbarPos;
                }
                if (list2[0] == "CheckBox")
                {
                    // Создание CheckBox'ов
                    var cb = new CheckBox();
                    cb.Name = list2[1];
                    cb.Parent = SettingsField;
                    cb.Left = int.Parse(list2[2]);
                    cb.Top = int.Parse(list2[3]);
                    cb.Width = int.Parse(list2[4]);
                    cb.Checked = bool.Parse(list2[5]);
                    cb.Text = list2[6];
                }
            }
            else
                // Если параметров меньше 4х то в 0 строке указано количество
                // регулируемых параметров
                fPFParamCNT = int.Parse(list2[0]);
        }
    }

    private void MenuElClick(object sender, EventArgs e)
    {
        string fn = sender.ToString();
        ApplyPluginMenuEl(fn);
    }

    private void ApplyPluginMenuEl(string fPluginName)
    {
        if (FPluginsList.TryGetValue(fPluginName, out PluginFDetails PFItem))
        {
            if (PFItem.PluginType == "MSGBox")
            {
                var PluginF = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    PFItem.PluginDoWorkFunction, typeof(TIMG_info_plugin));

                string msg = Marshal.PtrToStringAnsi(PluginF());
                MessageBox.Show(msg);
            }
            else if (PFItem.PluginType == "DForm")
            {
                var PluginF = (TIMG_info_plugin)Marshal.GetDelegateForFunctionPointer(
                    PFItem.PluginDoWorkFunction, typeof(TIMG_info_plugin));

                PluginF();
            }
        }
    }

    public string ParseSettings()
    {
        string res = "";

        for (int i = 1; i <= fPFParamCNT; i++)
        {
            Control comp = SettingsField.Controls.Find("INPUT_" + i, true).FirstOrDefault();

            if (comp != null)
            {
                if (i > 1)
                    res += " ";

                if (comp is TextBox textBox)
                    res += textBox.Text;
                else if (comp is CheckBox checkBox)
                    res += checkBox.Checked.ToString().ToLower();
                else if (comp is TrackBar trackBar)
                    res += ((float)trackBar.Value / Divizors[comp.Name] - Offcets[comp.Name]).ToString("G", CultureInfo.InvariantCulture);
            }
        }

        return res;
    }

}