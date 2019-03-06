using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            textBox1.Text = @"C:\work\programing\cs\FileSync\test\1";
            textBox2.Text = @"C:\work\programing\cs\FileSync\test\2";
        }

        private void buttonExec_Click(object sender, EventArgs e)
        {
            USync sync = new USync();
            textBox3.Text = sync.Main(textBox1.Text, textBox2.Text, 1);
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            USync sync = new USync();
            textBox3.Text = sync.Main(textBox1.Text, textBox2.Text, 2);
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            // ファイルをドロップできるようにする
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBox2_DragEnter(object sender, DragEventArgs e)
        {
            // ファイルをドロップできるようにする
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップしたファイルを取得
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (fileNames.Length > 0)
            {
                ((TextBox)sender).Text = fileNames[0];
            }
        }

        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップしたファイルを取得
            string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (fileNames.Length > 0)
            {
                ((TextBox)sender).Text = fileNames[0];
            }
        }

    }
}
