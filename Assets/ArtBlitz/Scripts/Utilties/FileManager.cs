using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace LeoScript.Utilties
{
    /// <summary>
    /// File manager
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class FileManager : MonoBehaviour
    {
        private static FileManager instance;
        public static FileManager Instance
        {
            get
            {
                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

 
        /// <summary>
        /// Save the texture to local disk.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="filePath">File path.</param>
        public static void SaveTextureToFile(Texture2D texture, string filePath, bool hasAlpha = false)
        {
            Debug.Log("save texture to file " + filePath);

            FileInfo info = new FileInfo(filePath);

            if (!Directory.Exists(info.DirectoryName))
            {
                Directory.CreateDirectory(info.DirectoryName);
            }

            byte[] data;

            if (!hasAlpha)
            {
                data = texture.EncodeToJPG(100);
            }
            else
            {
                data = texture.EncodeToPNG();
            }

            FileStream fs;
            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();

            Debug.Log(filePath + " saved");
        }

        /// <summary>
        /// Save the texture to local disk.
        /// </summary>
        /// <param name="texture">Texture.</param>
        /// <param name="fileFolder">File folder.</param>
        /// <param name="fileName">File name.</param>
        public static void SaveTextureToFile(Texture2D texture, string fileFolder, string fileName)
        {
            fileFolder = fileFolder.Replace("\\", "/");

            if (!Directory.Exists(fileFolder))
            {
                Directory.CreateDirectory(fileFolder);
            }
            SaveTextureToFile(texture, fileFolder + "/" + fileName);
        }

        public static void SaveDataToFile(byte[] data, string filePath)
        {
            FileInfo info = new FileInfo(filePath);

            if (!info.Directory.Exists)
            {
                Directory.CreateDirectory(info.DirectoryName);
            }

            File.WriteAllBytes(filePath, data);
        }

        /// <summary>
        /// Save the texture to local.
        /// </summary>
        /// <param name="tex">The texture.</param>
        public static void SaveTextureToLocalGallery(Texture2D tex, System.Action<string> onComplete)
        {
            string fileName = "temp.png";

#if UNITY_EDITOR || UNITY_STANDALONE

            string screenshotFolder = Application.persistentDataPath + "/Screenshots/";

            Debug.Log(screenshotFolder);

            screenshotFolder = screenshotFolder.Replace("\\", "/");

            SaveTextureToFile(tex, screenshotFolder, fileName);

            if (onComplete != null)
            {
                onComplete(screenshotFolder + "/" + fileName);
            }


            //#elif UNITY_ANDROID || UNITY_IOS

            //            try
            //            {
            //                NativeGallery.SaveImageToGallery(tex, "LiveOrbis", fileName);
            //            }
            //            catch (Exception e)
            //            {
            //    //            DebugLogManager.Instance.AddLog(e.ToString());
            //            }    

            //            if (onComplete != null)
            //            {
            //                onComplete(NativeGallery.GetSavePath("LiveOrbis",fileName));
            //            }
#endif

        }


        public static bool HasFileInPersistentPath(string fileName)
        {
            string filePath = GetPersistentFilePath(fileName);

            Debug.Log(filePath);

            return File.Exists(filePath);
        }

        /// <summary>
        /// Load the text from persistent path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string LoadTextFromPersistentPath(string fileName)
        {
            string path = GetPersistentFilePath(fileName); ;

            return LoadTextFromPath(path);
        }

        public static string LoadTextFromPath(string fullFilePath)
        {
            if (File.Exists(fullFilePath))
            {
                Debug.Log("file exist " + fullFilePath);
                return File.ReadAllText(fullFilePath);
            }
            else
            {
                Debug.LogError("file doesn't exist " + fullFilePath);
                return "";
            }
        }

        /// <summary>
        /// Save the text to persistent path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contents">The contents.</param>
        public static void SaveTextToPersistentPath(string fileName, string contents)
        {
            Debug.Log("save file to " + GetPersistentFilePath(fileName));
            File.WriteAllText(GetPersistentFilePath(fileName), contents);
        }

        public static void SaveTextToPath(string fileName, string contents)
        {
            FileInfo fi = new FileInfo(fileName);
            if (!fi.Directory.Exists)
            {
                Directory.CreateDirectory(fi.DirectoryName);
            }
            File.WriteAllText(fileName, contents);
        }


        /// <summary>
        /// Load texture from persistent path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static Texture2D LoadTextureFromPersistentPath(string fileName)
        {
            string path = GetPersistentFilePath(fileName);

            Texture2D texture = LoadTextureFromPath(path);

            if (texture != null)
            {
                return texture;
            }
            else
            {
                return null;
            }
        }

        public static Texture2D LoadTextureFromPath(string fileName)
        {
            if (File.Exists(fileName))
            {
                Texture2D tex = new Texture2D(2, 2);

                byte[] data = File.ReadAllBytes(fileName);

                tex.LoadImage(data,false);

                return tex;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Save the texture to persistent path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="tex">The texture.</param>
        public static void SaveTextureToPersistentPath(string fileName, Texture2D tex,bool hasAlpha)
        {
            string path = GetPersistentFilePath(fileName);

            FileInfo info = new FileInfo(path);

            if (!info.Directory.Exists)
            {
                Directory.CreateDirectory(info.DirectoryName);
                Debug.Log("create directory " + info.DirectoryName);
            }

            SaveTextureToFile(tex, path, hasAlpha);
        }

        /// <summary>
        /// Get the persistent file path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static string GetPersistentFilePath(string fileName)
        {
            return Application.persistentDataPath + "/" + fileName;
        }

        public static bool FileExistsInPersistentPath(string fileName)
        {
            return File.Exists(GetPersistentFilePath(fileName));
        }

        public static void DeleteFileInPersistentPath(string fileName)
        {
            File.Delete(GetPersistentFilePath(fileName));
        }

        public static void DeleteFileInPath(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                Debug.LogWarning("file trying to delete doesn't exist " + filePath);
            }
        }

        public static void SaveDataToPersistentPath(string fileName, byte[] data)
        {
            string path = GetPersistentFilePath(fileName);

            FileInfo info = new FileInfo(path);

            if (!info.Directory.Exists)
            {
                Debug.Log("create directory " + info.DirectoryName);
                Directory.CreateDirectory(info.DirectoryName);
            }

            File.WriteAllBytes(path, data);
        }

        public static byte[] LoadDataFromPersistentPath(string fileName)
        {
            string path = GetPersistentFilePath(fileName);

            return File.ReadAllBytes(path);
        }

        public static void CopyFolder(string SourcePath, string DestinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string path in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                string newPath = path.Replace(SourcePath, DestinationPath);

                Debug.Log("copy from " + path + " to " + newPath);

                File.Copy(path, newPath, true);
            }
        }

        public static List<string> GetAllFilesInFolder(string SourcePath)
        {
            List<string> allFilePaths = new List<string>();
            foreach (string path in Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories))
            {
                allFilePaths.Add(path);
            }
            return allFilePaths;
        }

        public static void DeleteAllFilesInFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                List<string> allFiles = GetAllFilesInFolder(folder);

                foreach (string path in allFiles)
                {
                    File.Delete(path);
                }

                DeleteEmptyFolders(folder);
            }
        }


        public static void DeleteEmptyFolders(string root)
        {
            foreach (string directory in Directory.GetDirectories(root))
            {
                DeleteEmptyFolders(directory);

                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}