﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace FileSync
{
    // 進捗表示用の情報
    public class MyProgress
    {
        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private int progress;

        public int Progress
        {
            get { return progress; }
            set { progress = value; }
        }

        public MyProgress(string title, int progress)
        {
            this.title = title;
            this.progress = progress;
        }
    }

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
        //private List<UFileInfo> srcFiles;
        private List<UFileInfo> destFiles;
        private Dictionary<string, UFileInfo> srcFiles;

        private List<string> renameCopyList;

        private string srcRootPath;
        private string destRootPath;

        // 外部からのキャンセル要求
        private bool cancelFlag;
        public bool CancelFlag
        {
            get { return cancelFlag; }
            set { cancelFlag = value; }
        }



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
        public string Main(string srcRoot, string destRoot, int mode, IProgress<MyProgress> p)
        {
            var log = new StringBuilder();
            int moveFileCnt = 0;
            int fileCnt = 0;
            var myProgress = new MyProgress("Main", 0);

            srcRootPath = srcRoot;
            destRootPath = destRoot;

            try
            {
                log.AppendLine("*** new リストを作成する ***");
                srcFiles = GetFilesInfo2(srcRoot, log, p);

                log.AppendLine("*** old リストを作成する ***");
                destFiles = GetFilesInfo(destRoot, p);

                renameCopyList = new List<string>();

                p.Report(myProgress);

                // destFilesのファイルをsrcFilesから探す
                // 見つかったらsrcFilesのルートと同じ場所に移動する
                foreach (UFileInfo ufi in destFiles)
                {
                    if (cancelFlag)
                    {
                        throw new Exception("Canceled");
                    }

                    if (srcFiles.ContainsKey(ufi.hashMD5))
                    {
                        UFileInfo ufi2 = srcFiles[ufi.hashMD5];
                        if (ufi.equalTo(ufi2))
                        {
                            // 移動
                            if (ufi.filePath != ufi2.filePath || ufi.fileName != ufi2.fileName)
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
                                    log.AppendLine("Create Folder: " + dirPath);
                                }

                                if (mode == 1)
                                {
                                    // 移動先に同名のファイルが存在していたらリネームしてコピー
                                    if (File.Exists(path2) == true)
                                    {
                                        // 後で .tmpを除去するためにリストに追加
                                        renameCopyList.Add(path2);
                                        path2 += ".tmp";
                                    }
                                    File.Move(path1, path2);
                                }
                                log.AppendLine("Move File: From " + ufi.filePath + @"\" + ufi.fileName + " To " + ufi2.filePath + @"\" + ufi2.fileName);
                                moveFileCnt++;
                            }
                        }
                    }
                    fileCnt++;
                    myProgress.Progress = (int)((float)fileCnt / (float)destFiles.Count * 100.0f);
                    p.Report(myProgress);
                }

                // .tmpをつけてコピーしたファイルを元の名前に戻す
                foreach (string fileName in renameCopyList)
                {
                    File.Move(fileName + ".tmp", fileName);
                }

                myProgress.Progress = 100;
                p.Report(myProgress);

                log.AppendLine(moveFileCnt + "個のファイルを移動しました。");
            }
            catch (Exception e)
            {
                log.AppendLine("Error: " + e.Message);
            }
            return log.ToString();
        }

        /// <summary>
        /// 指定ディレクトリ以下にあるファイルのリストを作成する
        /// </summary>
        /// <param name="rootDir">ルートディレクトリパス</param>
        /// <returns>ファイルリスト(UFileInfo)</returns>
        public List<UFileInfo> GetFilesInfo(string rootDir, IProgress<MyProgress> p)
        {
            //"C:\test"以下の".txt"ファイルをすべて取得する
            DirectoryInfo di = new System.IO.DirectoryInfo(rootDir);
            FileInfo[] files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            var myProgress = new MyProgress("GetFilesInfo", 0);
            int processCnt = 0;

            List<UFileInfo> list = new List<UFileInfo>();

            p.Report(myProgress);

            foreach (FileInfo f in files)
            {
                if (cancelFlag)
                {
                    throw new Exception("キャンセルされました");
                }

                string fileName = Path.GetFileName(f.FullName);
                string filePath = Path.GetDirectoryName(f.FullName).Replace(rootDir, "");
                UFileInfo ufi = new UFileInfo(fileName, filePath, f.Length, f.LastWriteTime, getFileMD5(f.FullName));
                list.Add(ufi);

                processCnt++;
                myProgress.Progress = (int)((float)processCnt / (float)files.Length * 100.0f);
                p.Report(myProgress);
            }
            return list;
        }

        /// <summary>
        /// 指定ディレクトリ以下のファイルをファイルのハッシュ(MD5)をキーにした辞書データを作成する。
        /// </summary>
        /// <param name="rootDir">ルートディレクトリパス</param>
        /// <param name="log">ログ出力先</param>
        /// <returns>ファイル辞書データ(ファイルのハッシュがキー)</returns>
        public Dictionary<string, UFileInfo> GetFilesInfo2(string rootDir, StringBuilder log, IProgress<MyProgress> p)
        {
            //"C:\test"以下の".txt"ファイルをすべて取得する
            DirectoryInfo di = new DirectoryInfo(rootDir);
            FileInfo[] files = di.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            var myProgress = new MyProgress("GetFilesInfo2", 0);
            int processCnt = 0;

            var dic = new Dictionary<string, UFileInfo>();

            p.Report(myProgress);

            foreach (FileInfo f in files)
            {
                if (cancelFlag)
                {
                    throw new Exception("キャンセルされました");
                }

                string key = getFileMD5(f.FullName);
                string fileName = Path.GetFileName(f.FullName);
                string filePath = Path.GetDirectoryName(f.FullName).Replace(rootDir, "");

                if (dic.ContainsKey(key) == false)
                {
                    UFileInfo ufi = new UFileInfo(fileName, filePath, f.Length, f.LastWriteTime, key);
                    dic[key] = ufi;
                }
                else
                {
                    log.AppendLine(Path.Combine(filePath, fileName) + "は既に存在します。(" + Path.Combine(dic[key].filePath, dic[key].fileName) + "と同じファイル)");
                }
                processCnt++;
                myProgress.Progress = (int)((float)processCnt / (float)files.Length * 100.0f);
                p.Report(myProgress);
            }
            return dic;
        }

        /// <summary>
        /// ファイルからMD5形式のハッシュ値を取得する。
        /// ※ファイル全体からハッシュ値を取得するのは重すぎるため、先頭中間末尾からそれぞれ1KByteずつ
        /// サンプルしている。
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
            byte[] fileData;

            // ファイル全体のMD5は重いため簡易版に置き換え
            //MD5CryptoServiceProviderオブジェクトを作成
            System.Security.Cryptography.MD5 md5 =
                System.Security.Cryptography.MD5.Create();

            // ファイルの先頭、中間、末尾からそれぞれ1KByteを読み込む(計3KByte)
            long fileLen = fs.Length;
            const int chankSize = 1024;
            if (fileLen >= 2147483648)
            {
                fileLen = 2147483648;
            }

            if (fileLen >= chankSize * 3)
            {
                fileData = new byte[chankSize * 3];
                var tempData = new byte[chankSize];
                fs.Read(fileData, 0, chankSize);
                fs.Seek((fileLen / 2) - chankSize / 2, SeekOrigin.Begin);
                fs.Read(fileData, chankSize, chankSize);
                fs.Seek(fileLen - chankSize, SeekOrigin.Begin);
                fs.Read(fileData, chankSize * 2, chankSize);
            }
            else
            {
                fileData = new byte[fileLen];
                fs.Read(fileData, 0, (int)fileLen);
            }
            byte[] bs = md5.ComputeHash(fileData);
            md5.Clear();

            //ファイルを閉じる
            fs.Close();

            //byte型配列を16進数の文字列に変換
            StringBuilder result = new StringBuilder();
            foreach (byte b in bs)
            {
                result.Append(b.ToString("x2"));
            }
            // ファイルサイズの4byte目までも追加
            result.Append(String.Format("{0:X8}", (int)fileLen));

            return result.ToString();
        }

    }
}
