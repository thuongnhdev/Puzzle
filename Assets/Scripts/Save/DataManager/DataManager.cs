using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using com.F4A.MobileThird;

namespace DataCore
{
    /// <summary>
    /// Data Manager class. Use to store and load json files. Works on Android And Ios.
    /// </summary>
    public static class DataManager
    {
        public static string DataExt(string dataName)
        {
            string filePathData = string.Format("/Extensions/{0}.dat", dataName);
            string fileExt = "";
            if (File.Exists(Application.persistentDataPath + filePathData))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream loadData = File.Open(Application.persistentDataPath + filePathData, FileMode.Open);

                if(loadData.Length > 0)
                {
                    fileExt = (string)bf.Deserialize(loadData);
                    loadData.Close();
                }
            }
            return fileExt;
        }

        /// <summary>
        /// save data
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="fileName">Data storage Key</param>
        /// <param name="data">data source</param>
        public static void Save<T>(string dataName, T data)
        {
            if (data == null) return;

            Type dataType = data.GetType();
            var extensionPath = string.Format("{0}/Extensions", Application.persistentDataPath);
            var dataTypePath = string.Format("{0}/DataType", Application.persistentDataPath);
            if (!Directory.Exists(extensionPath))
                Directory.CreateDirectory(extensionPath);
            if (!Directory.Exists(dataTypePath))
                Directory.CreateDirectory(dataTypePath);
            BinaryFormatter formatter = new BinaryFormatter();
            string filePathData = "/Extensions/" + dataName + ".dat";
            FileStream saveStreamData = File.Create(Application.persistentDataPath + filePathData);
            if(saveStreamData != null)
            {
                string dat = dataType.Name;
                if (dat.Contains("`")) dat = dat.Split('`')[0];
                if (dat.Contains("[]")) dat = "Array";
                formatter.Serialize(saveStreamData, dat);
                saveStreamData.Close();

                BinaryFormatter bf = new BinaryFormatter();
                string filePath = string.Format("/DataType/{0}.{1}", dataName, dat);
                FileStream saveStream = File.Create(Application.persistentDataPath + filePath);
                if(saveStream != null)
                {
                    bf.Serialize(saveStream, data);
                    saveStream.Close();
                }
               
            }
        

        }

        public static T Load<T>(string dataName)
        {
            T result = default(T);
            string filePathData = string.Format("/Extensions/{0}.dat", dataName);
            string fileExt = DataExt(dataName);
            if (string.IsNullOrEmpty(fileExt))
                return result;

            string filePath = string.Format("/DataType/{0}.{1}", dataName, fileExt);

            if (File.Exists(Application.persistentDataPath + filePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = File.Open(Application.persistentDataPath + filePath, FileMode.Open);
                DataCore.Debug.Log("Load data from:" + Application.persistentDataPath + filePath, false);

                if (fs.Length > 0)
                {
                    result = (T)bf.Deserialize(fs);
                    fs.Close();
                }
            }
            else
            {
                DataCore.Debug.Log("Doesn't Exit:" + filePath);
            }
            return result;
        }

        /// <summary>
        /// Check if the data exists
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public static bool Exit(string dataName)
        {
            string filePathData = string.Format("{0}/Extensions/{1}.dat", Application.persistentDataPath, dataName);
            if (File.Exists(filePathData))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Delete some data
        /// </summary>
        /// <param name="dataName">data name</param>
        public static void Delete(string dataName)
        {
            string filePathData = string.Format("{0}/Extensions/{1}.dat", Application.persistentDataPath, dataName);
            if (File.Exists(filePathData))
            {
                File.Delete(filePathData);
            }
            string dataExt = DataExt(dataName);
            if (!string.IsNullOrEmpty(dataExt))
            {
                string filePath = string.Format("{0}/DataType/{1}.{2}", Application.persistentDataPath, dataName, dataExt);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        /// <summary>
        /// Delete all data
        /// </summary>
        public static void DeleteAllData()
        {
            var path = string.Format("{0}/Extensions", Application.persistentDataPath);
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }
            }
            path = string.Format("{0}/DataType", Application.persistentDataPath);
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    File.Decrypt(file);
                }
            }
        }
    }
}