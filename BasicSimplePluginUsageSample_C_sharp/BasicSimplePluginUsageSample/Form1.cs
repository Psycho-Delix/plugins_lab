using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ImagesTest
{
    public partial class Form1 : Form
    {
        private Plugin PluginsHolder;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PluginsHolder = new Plugin();
            PluginsHolder.SettingsField = SettingsField;
            PluginsHolder.FaListBox = FunctionList;
            PluginsHolder.PluginsMenu = PluginsMenu;

            // Определяем путь к папке с плагинами (поумолчанию там где exe файл) 
            string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

            // Загружаем плагины
            var pluginFiles = new List<string>();
            PluginsHolder.LoadPlugins(pluginPath, "*.dllа", pluginFiles);
            FileListBox.Items.Clear();

            foreach (var pluginFile in pluginFiles)
            {
                FileListBox.Items.Add(pluginFile);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Необходимо открыть какое-либо изображение");
            }
            else if (FunctionList.SelectedIndex >= 0)
            {
                string selectedPlugin = FunctionList.Items[FunctionList.SelectedIndex].ToString();
                Bitmap inBitmap = new Bitmap(pictureBox1.Image);
                Bitmap outBitmap = new Bitmap(inBitmap.Width, inBitmap.Height);
                pictureBox2.Image = inBitmap;

                PluginsHolder.ApplyPluginIMG2IMG(selectedPlugin, inBitmap, ref outBitmap);

                pictureBox2.Image?.Dispose();
                pictureBox2.Image = outBitmap;
                pictureBox2.Invalidate();

                label1.Text = $"Time: {PluginsHolder.Time:0.0000} ms.";
            }
        }

        private void CS_Stretch_CheckedChanged(object sender, EventArgs e)
        {
            if (CS_Stretch.Checked)
            {
                pictureBox1.Dock = DockStyle.Fill;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox2.Dock = DockStyle.Fill;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            else
            {
                pictureBox1.Dock = DockStyle.None;
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox2.Dock = DockStyle.None;
                pictureBox2.SizeMode = PictureBoxSizeMode.AutoSize;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            PluginsHolder?.Dispose();
        }

        private void FunctionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FunctionList.SelectedIndex >= 0)
            {
                PluginsHolder.CreatePluginFunctionSettings(FunctionList.SelectedItem.ToString());
            }
        }



        private void N2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void N3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                string ext = Path.GetExtension(filePath).ToUpper();

                if (File.Exists(filePath))
                {
                    if (ext == ".JPG")
                    {
                        var jp = new Bitmap(filePath);
                        pictureBox1.Image = jp;
                    }
                    else if (ext == ".PNG")
                    {
                        var pn = new Bitmap(filePath);
                        pictureBox1.Image = pn;
                    }
                    else if (ext == ".BMP")
                    {
                        pictureBox1.Image = new Bitmap(filePath);
                    }
                    else
                    {
                        MessageBox.Show("Unsupported file format");
                    }
                }
            }
            CS_Stretch_CheckedChanged(this, EventArgs.Empty);
        }
    }
}
