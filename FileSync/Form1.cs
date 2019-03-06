using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using USync.Utility;

namespace FileSync
{
    public partial class Form1 : Form
    {
        // 実行中のUSyncオブジェクト
        private USync mSync;

        public Form1()
        {
            InitializeComponent();

            IniFileManager.Singleton.ReadFromFile();
            string temp;
            if (IniFileManager.Singleton.GetString("form", "textbox1", out temp))
            {
                textBox1.Text = temp;
            }
            if (IniFileManager.Singleton.GetString("form", "textbox2", out temp))
            {
                textBox2.Text = temp;
            }
        }

        private void buttonExec_Click(object sender, EventArgs e)
        {
            // Progressクラスのインスタンスを作成
            var p = new Progress<int>(ShowProgress);

            mSync = new USync();
            textBox3.Text = mSync.Main(textBox1.Text, textBox2.Text, 1, p);
            mSync = null;
        }

        private async void buttonCheck_Click(object sender, EventArgs e)
        {
            mSync = new USync();

            progressBar1.Value = 0;
            labelProgress.Text = "処理中";

            // Progressクラスのインスタンスを作成
            var p = new Progress<int>(ShowProgress);

            // バックグラウンドで実行
            try
            {
                textBox3.Text = await Task.Run(() => mSync.Main(textBox1.Text, textBox2.Text, 2, p));
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            mSync = null;
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


        private void ShowProgress(int percent)
        {
            labelProgress.Text = percent + "%完了";
            progressBar1.Value = percent;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (mSync != null)
            {
                mSync.CancelFlag = true;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            IniFileManager.Singleton.SetString("form", "textbox1", textBox1.Text);
            IniFileManager.Singleton.SetString("form", "textbox2", textBox2.Text);
            IniFileManager.Singleton.WriteToFile();
        }
    }
}
