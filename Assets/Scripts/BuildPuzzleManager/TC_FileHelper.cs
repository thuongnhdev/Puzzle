using System.IO;
using UnityEngine;

public class TC_FileHelper : MonoBehaviour
{
    public static string ReadString(string path = "Assets/Resources/Counter.txt")
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string line = "";

        line = reader.ReadLine();

        reader.Close();

        return line;
    }

    public static void WriteString(string strValue, string path = "INPUT")
    {
        //path = Application.persistentDataPath + "/";
        string outPath = Application.dataPath + "/Resources";
        path = outPath + "/" + path + "/";
            
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path + ".txt", false);
        writer.WriteLine(strValue);
        
        writer.Close();        
    }
}
