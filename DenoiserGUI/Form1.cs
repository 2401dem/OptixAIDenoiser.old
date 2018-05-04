using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DenoiserGUI
{
    public partial class Form1 : Form
    {
        private bool IsFolder = false;

        Bitmap pBuffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void lockButton()
        {
            button3.Enabled = false;
        }

        public void unlockButton()
        {
            button3.Enabled = true;
        }

        private void ShowDone()
        {
            button3.Text = "Down!";
        }

        private void Recover()
        {

            button3.Text = IsFolder ? "Denoise Folder" : "Denoise Image";
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Denoiser.SetUpContext();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Denoiser.CleantUpContext();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (IsFolder)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;


                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string tmp = dialog.FileName;
                    if (tmp.LastIndexOf('\\') != -1)
                    {
                        textBox1.Text = tmp;
                        if (!Directory.Exists(textBox2.Text))
                            textBox2.Text = tmp;
                    }
                }

            }
            else
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Supported Files (*.bmp, *.jpg, *.png, *.tif, *.exr)|*.jpg;*.png;*.bmp;*.tif;*.exr|Bitmap Images (*.bmp)|*.bmp|JPEG Images (*.jpg)|*.jpg|PNG Images (*.png)|*.png|TIFF Images (*.tif)|*.tif|OpenEXR Images (*.exr)|*.exr";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string tmp = dialog.FileName;
                    if (tmp.LastIndexOf('\\') != -1)
                    {
                        textBox1.Text = tmp;
                        textBox2.Text = tmp.Substring(0, tmp.LastIndexOf('\\') + 1) + "out_" + tmp.Substring(tmp.LastIndexOf('\\') + 1);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsFolder)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;


                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string tmp = dialog.FileName;
                    if (tmp.LastIndexOf('\\') != -1)
                    {
                        textBox2.Text = tmp;
                    }
                }
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Bitmap Images (*.bmp)|*.bmp|JPEG Images (*.jpg)|*.jpg|PNG Images (*.png)|*.png|TIFF Images (*.tif)|*.tif|OpenEXR Images (*.exr)|*.exr";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string tmp = dialog.FileName;
                    if (tmp.LastIndexOf('\\') != -1)
                    {
                        textBox2.Text = tmp;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            Recover();
            lockButton();
            Denoiser.SetArgs(textBox1.Text, textBox2.Text, (float)numericUpDown1.Value, checkBox1.Checked);
            Denoiser.Denoise();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (IsFolder)
                if (textBox2.Text != "" && Directory.Exists(textBox2.Text))
                    System.Diagnostics.Process.Start(textBox2.Text);
                else
                    MessageBox.Show("Output folder does not exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                if (textBox2.Text.LastIndexOf('\\') != -1 && Directory.Exists(textBox2.Text.Substring(0, textBox2.Text.LastIndexOf('\\'))))
                System.Diagnostics.Process.Start(textBox2.Text.Substring(0, textBox2.Text.LastIndexOf('\\')));
            else
                MessageBox.Show("Output folder does not exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public int DrawToPictureBox(IntPtr data, int width, int height)
        {
            ShowDone();
            pBuffer = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, data);
            if (pBuffer == null)
                return -1;
            pBuffer.RotateFlip(RotateFlipType.Rotate180FlipX);
            pictureBox1.Image = pBuffer;
            Console.WriteLine("DrawToPictureBox.");
            return 0;
        }

        public void SetProgress(float progress)
        {
            progressBar1.Value = (int)(progress * 100.0f);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Recover();
            IsFolder = checkBox1.Checked;
            if (IsFolder)
            {
                label1.Text = "Input Folder Path";
                label2.Text = "Output Folder Path";
                button3.Text = "Denoise Folder";
            }
            else
            {
                label1.Text = "Input Image Path";
                label2.Text = "Output Image Path";
                button3.Text = "Denoise Image";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            numericUpDown1.Value = (decimal)trackBar1.Value / 100;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)(numericUpDown1.Value * 100);
        }
    }
}
