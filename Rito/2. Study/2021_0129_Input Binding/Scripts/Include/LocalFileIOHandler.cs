using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

// 날짜 : 2021-01-29 PM 6:04:57
// 작성자 : Rito

namespace Rito
{
    public static class LocalFileIOHandler
    {
        private static string RootPath => $"{Application.dataPath}/";

        public static bool Save(in string text, in string dir, in string file, in string ext)
        {
            string path = Path.Combine(RootPath, dir, $"{file}.{ext}");

            FileInfo fi = new FileInfo(path);
            DirectoryInfo di = fi.Directory;

            // 1. Check Folder
            if (!di.Exists)
            {
                di.Create();
            }

            // 2. Write File
            File.WriteAllText(path, text);

            return true;
        }

        public static string Load(in string dir, in string file, in string ext)
        {
            string path = Path.Combine(RootPath, dir, $"{file}.{ext}");

            FileInfo fi = new FileInfo(path);

            // Not Existed
            if (!fi.Exists)
                return null;

            return File.ReadAllText(path);
        }
    }
}