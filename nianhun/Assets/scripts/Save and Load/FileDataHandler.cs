using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class FileDataHandler 
{
    private string dataDirPath = "";
    private string dataFileName = "";

    private bool encryptData = false;
    private readonly string codeWord = "nianhun";//安全密钥保护数据不被修改

    public FileDataHandler(string dataDirPath, string dataFileName, bool encryptData)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.encryptData = encryptData;
        
    }

    public void Save(GameData data)
    {
        string fullPath =Path.Combine(dataDirPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataTostore = JsonUtility.ToJson(data,true);//要存储的数据

            if (encryptData)
                dataTostore = EncryptDecrypt(dataTostore);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create))//创建文件
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataTostore);//用writer写入文件
                }
            }
        }

        catch(Exception e)
        {
            Debug.LogError("在试图保存到文件时出错" +  fullPath + "\n" + e);
        }
    }//保存函数
    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadData = null;//默认为空

        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                using(FileStream stream = new FileStream(fullPath, FileMode.Open))//打开文件
                {
                    using(StreamReader reader =  new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();//用reader读取
                    }
                }

                if(encryptData)
                    dataToLoad = EncryptDecrypt(dataToLoad);

                loadData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("在试图加载文件时候发生错误" + fullPath + "\n" + e);
            }
        }

        return loadData;
    }

    public void Delete()
    {
        string fullPath =Path.Combine(dataDirPath, dataFileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string EncryptDecrypt(string data)
    {
        StringBuilder modifiedData = new StringBuilder(data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData.Append((char)(data[i] ^ codeWord[i % codeWord.Length]));

        }
            return modifiedData.ToString();
    }
}
