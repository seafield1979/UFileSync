using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace FileSync
{
    class UFileInfo
    {
        //
        // Properties
        //
        public string fileName;
        public string filePath;     // ルートからの位置
        public long fileSize;
        public string hashMD5;          // MD5ハッシュ値
        public DateTime updatedTime;
        
        public UFileInfo(string fileName, string filePath, long fileSize, DateTime updatedTime, string hashMD5)
        {
            this.fileName = fileName;
            this.filePath = filePath;
            this.fileSize = fileSize;
            this.updatedTime = updatedTime;
            this.hashMD5 = hashMD5;
        }

        /// <summary>
        /// 同じかどうかをチェックする
        /// </summary>
        /// <param name="ufi"></param>
        /// <returns>false:異なる / true:同じ</returns>
        public bool equalTo(UFileInfo ufi)
        {
            if (this.fileSize == ufi.fileSize && 
                this.hashMD5 == ufi.hashMD5)
            {
                return true;
            }
            return false;
        }
    }

    class USync
    {
        // 
        // Properties
        //
        private List<UFileInfo> srcFiles;
        private List<UFileInfo> destFiles;

        private string srcRootPath;
        private string destRootPath;

        public USync()
        {   
        }

        /// <summary>
        /// ファイル移動実行メイン
        /// </summary>
        /// <param name="srcRoot"></param>
        /// <param name="destRoot"></param>
        /// <param name="mode">1:移動 / 2:移動なし</param>
        /// <returns></returns>
        public string Main(string srcRoot, string destRoot, int mode)
        {
            var log = new StringBuilder();
            int moveFileCnt = 0;

            srcRootPath = srcRoot;
            destRootPath = destRoot;

            srcFiles = GetFilesInfo(srcRoot);
            destFiles = GetFilesInfo(destRoot);

            // destFilesのファイルをsrcFilesから探す
            // 見つかったらsrcFilesのルートと同じ場所に移動する
            foreach (UFileInfo ufi in destFiles)
            {
                foreach (UFileInfo ufi2 in srcFiles)
                {
                    if (ufi.equalTo(ufi2))
                    {
                        // 移動
                        if (ufi.filePath != ufi2.filePath)
                        {
                            string path1 = destRootPath + ufi.filePath + @"\" + ufi.fileName;
                            string path2 = destRootPath + ufi2.filePath + @"\" + ufi2.fileName;

                            // 移動先のフォルダが存在しないなら作成する
                            if (Directory.Exists(destRootPath + ufi2.filePath) == false)
                            {
                                string dirPath = destRootPath + ufi2.filePath;
                                if (mode == 1)
                                {
                                    Directory.CreateDirectory(dirPath);
                                }
                                log.AppendLine("フォルダを作成" + dirPath);
                            }
                            
                            if (mode == 1)
                            {
                                File.Move(path1, path2);
                            }
                            srcFiles.Remove(ufi2);
                            log.AppendLine( "ファイル移動 " + ufi.filePath + @"\" + ufi.fileName + " から " + ufi2.filePath + @"\" + ufi2.fileName);
                            moveFileCnt++;
                            break;
                        }
                    }
                }
            }
            log.AppendLine(moveFileCnt + "個のファイルを移動しました。");
            return log.ToString();
        }

        public List<UFileInfo> GetFilesInfo(string rootDir)
        {
            //"C:\test"以下の".txt"ファイルをすべて取得する
            DirectoryInfo di = new System.IO.DirectoryInfo(rootDir);
            FileInfo[] files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            List<UFileInfo> list = new List<UFileInfo>();

            foreach (FileInfo f in files)
            {
                string fileName = Path.GetFileName(f.FullName);
                string filePath = Path.GetDirectoryName(f.FullName).Replace(rootDir, "");
                UFileInfo ufi = new UFileInfo(fileName, filePath, f.Length, f.LastWriteTime, getFileMD5(f.FullName));
                list.Add(ufi);
            }
            return list;
        }

        /// <summary>
        /// ファイルからMD5形式のハッシュ値を取得する
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>１６進数のハッシュ文字列</returns>
        private string getFileMD5(string fileName)
        {
            //ファイルを開く
            FileStream fs = new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            //MD5CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.MD5 md5 =
                System.Security.Cryptography.MD5.Create();

            //ハッシュ値を計算する
            byte[] bs = md5.ComputeHash(fs);

            //リソースを解放する
            md5.Clear();
            //ファイルを閉じる
            fs.Close();

            //byte型配列を16進数の文字列に変換
            StringBuilder result = new StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("x2"));
            }

            return result.ToString();
        }

    }
}
