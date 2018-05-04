using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace DenoiserGUI
{
    static class Program
    {
        /// <summary>
        /// Entry Point
        /// </summary>
        /// 

        public static Form1 form1;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form1 = new Form1();
            Application.Run(form1);
        }
    }

    public class Denoiser
    {
        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void progressCallBack(float progress);

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_denoiseImplement", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static IntPtr _denoiseImplement(char[] input_path, char[] output_path, float blend, bool is_batch);

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_setUpContext", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static void _setUpContext();

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_cleantUpContext", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static void _cleantUpContext();

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_jobStart", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static void _jobStart(int width, int height, float blend);

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_jobComplete", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static void _jobComplete();

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_getWidth", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static int _getWidth(char[] file_path);

        [DllImport("OptixAIDenoiser.dll", EntryPoint = "_getHeight", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        extern static int _getHeight(char[] file_path);

        public static void SetUpContext()
        {
            _setUpContext();
        }

        public static void CleantUpContext()
        {
            _cleantUpContext();
        }

        private static string _input_path = ".\\";
        private static string _output_path = ".\\";
        private static float _blend = 0.05f;
        private static bool _is_folder = false;

        public static void SetArgs(string input_path, string output_path, float blend, bool IsFolder)
        {
            _input_path = input_path;
            _output_path = output_path;
            _blend = blend;
            _is_folder = IsFolder;
        }

        public static int Denoise()
        {
            if (_is_folder)
            {
                if (Directory.Exists(_input_path) || Directory.Exists(_input_path.Substring(0, _input_path.LastIndexOf('\\') > 0 ? _input_path.LastIndexOf('\\') : 0)))
                {
                    if (Directory.Exists(_output_path) || Directory.Exists(_output_path.Substring(0, _output_path.LastIndexOf('\\') > 0 ? _output_path.LastIndexOf('\\') : 0)))
                    {
                        var file_paths = Directory.GetFiles(_input_path, "*.*", SearchOption.TopDirectoryOnly);
                        List<string> input_file_paths = new List<string>();
                        List<string> output_file_paths = new List<string>();
                        foreach (string file_path in file_paths)
                        {
                            Regex regex = new Regex(".\\.(bmp|jpg|png|tif|exr)$", RegexOptions.IgnoreCase);
                            if (regex.IsMatch(file_path))
                            {
                                input_file_paths.Add(file_path);
                                output_file_paths.Add(_output_path + "\\out_" + file_path.Substring(file_path.LastIndexOf('\\') + 1));
                                Console.WriteLine(file_path);
                            }
                        }
                        int width = _getWidth(input_file_paths[0].ToCharArray());
                        int height = _getHeight(input_file_paths[0].ToCharArray());

                        if (width == -1 || height == -1)
                        {
                            MessageBox.Show("Picture Format Error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Program.form1.unlockButton();
                            return -1;
                        }
                        //Bitmap pBuffer = new Bitmap(input_file_paths[0], false);
                        IntPtr tmpptr = new IntPtr();

                        _jobStart(width, height, _blend);
                        for (int i = 0; i < input_file_paths.Count; ++i)
                        {
                            Program.form1.SetProgress((float)i / (float)input_file_paths.Count);
                            if (File.Exists(output_file_paths[i]))
                                continue;
                            //IntPtr tmpptr = 
                            tmpptr = _denoiseImplement(input_file_paths[i].ToCharArray(), output_file_paths[i].ToCharArray(), _blend, _is_folder);
                            /*if (tmpptr != null)
                                if (Program.form1.DrawToPictureBox(tmpptr, pBuffer.Width, pBuffer.Height) == 0)
                                    continue;
                                else
                                {
                                    MessageBox.Show("Picture Size Error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                }
                            else
                            {
                                //MessageBox.Show("Picture Size Error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //break;
                            }*/
                        }

                        _jobComplete();
                        if (tmpptr != null)
                            if (Program.form1.DrawToPictureBox(tmpptr, width, height) != 0)
                                MessageBox.Show("Picture Size Error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        Program.form1.SetProgress(1.0f);
                        Program.form1.unlockButton();
                        return 0;
                    }
                    else
                        MessageBox.Show("Output folder does not exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("Input folder does not exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (File.Exists(_input_path))
                {
                    if (Directory.Exists(_output_path.Substring(0, _output_path.LastIndexOf('\\'))))
                    {
                        int width = _getWidth(_input_path.ToCharArray());
                        int height = _getHeight(_input_path.ToCharArray());
                        Console.WriteLine(width.ToString());
                        Console.WriteLine(height.ToString());
                        if (width == -1 || height == -1)
                        {
                            MessageBox.Show("Picture Format Error!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Program.form1.unlockButton();
                            return -1;
                        }

                        _jobStart(width, height, _blend);
                        IntPtr tmpptr = _denoiseImplement(_input_path.ToCharArray(), _output_path.ToCharArray(), _blend, _is_folder);
                        if (tmpptr != null)
                        {
                            if (Program.form1.DrawToPictureBox(tmpptr, width, height) == -1)
                                MessageBox.Show("Picture Size Error(NULL to Draw)!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                            MessageBox.Show("Picture Size Error!(NULL Return)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Program.form1.SetProgress(1.0f);
                        Program.form1.unlockButton();
                        _jobComplete();
                        return 0;
                    }
                    else
                        MessageBox.Show("Output folder does not exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show("Input file does not exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Program.form1.unlockButton();
            return -1;
        }
    }
}
