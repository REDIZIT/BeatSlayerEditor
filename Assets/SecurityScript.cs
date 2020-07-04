using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class SecurityScript : MonoBehaviour {

    public static int KeyLength = 128;
    private const string SaltKey = "ShMG8hLyZ7k~Ge5@";
    private const string VIKey = "~6YUi0Sv5@|{aOZO"; // TODO: Generate random VI each encryption and store it with encrypted value
    public SecurityPlayerPrefs prefs;

    //public bool makeDump;

    /*void Update()
    {
        if(makeDump)
        {
            prefs.Dump();
            makeDump = false;
        }
    }*/


    public SecurityScript()
    {
        prefs = new SecurityPlayerPrefs(this);
    }


    public void WriteLines(string filepath, string[] lines)
    {
        string lineToEncrypt = "";
        for (int i = 0; i < lines.Length; i++)
        {
            if (i == lines.Length - 1)
            {
                lineToEncrypt += lines[i];
            }
            else
            {
                lineToEncrypt += lines[i] + @"
";
            }
        }
        File.WriteAllText(filepath, Encrypt(lineToEncrypt));
    }
    public string[] ReadLines(string filepath)
    {
        if (File.Exists(filepath))
        {
            string data = File.ReadAllText(filepath);
            string deData = Decrypt(data, "Sosi");
            return deData.Split('\n');
        }
        File.Create(filepath);
        return null;
    }


    public string Encrypt(byte[] value, string password)
    {
        var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
        var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
        var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(value, 0, value.Length);
                cryptoStream.FlushFinalBlock();
                cryptoStream.Close();
                memoryStream.Close();

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }

    public string Encrypt(string value)
    {
        return Encrypt(Encoding.UTF8.GetBytes(value), "Sosi");
    }

    public string Decrypt(string value, string password)
    {
        var cipherTextBytes = Convert.FromBase64String(value);
        var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
        var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None };
        var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

        using (var memoryStream = new MemoryStream(cipherTextBytes))
        {
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                memoryStream.Close();
                cryptoStream.Close();

                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
            }
        }
    }
}
public class SecurityPlayerPrefs
{
    private SecurityScript ss;

    public SecurityPlayerPrefs(SecurityScript ss)
    {
        this.ss = ss;
    }

    public void SetKey(string keyname, string value)
    {
        List<string> data = new List<string>();
        if (File.Exists(Application.persistentDataPath + "/Prefs.txt"))
        {
            data.AddRange(ss.ReadLines(Application.persistentDataPath + "/Prefs.txt"));
            bool hasKey = false;
            for (int i = 0; i < data.ToArray().Length; i++)
            {
                if (data[i].Contains(keyname))
                {
                    data[i] = keyname + ":" + value;
                    hasKey = true;
                }
            }
            if (!hasKey)
            {
                data.Add(keyname + ":" + value);
            }
            ss.WriteLines(Application.persistentDataPath + "/Prefs.txt", data.ToArray());
        }
        else
        {
            File.WriteAllText(Application.persistentDataPath + "/Prefs.txt", ss.Encrypt(keyname + ":" + value));
        }
    }
    public string GetKey(string keyname)
    {
        if (File.Exists(Application.persistentDataPath + "/Prefs.txt"))
        {
            List<string> data = new List<string>();
            data.AddRange(ss.ReadLines(Application.persistentDataPath + "/Prefs.txt"));
            for (int i = 0; i < data.ToArray().Length; i++)
            {
                if (data[i].Contains(keyname))
                {
                    return data[i].Split(':')[1];
                }
            }
        }
        return "";
    }
    public int GetInt(string keyname)
    {
        string d = GetKey(keyname);
        int i = 0;
        int.TryParse(d, out i);
        return i;
    }
    public bool GetBool(string keyname)
    {
        string d = GetKey(keyname);
        if (d == "") return false;
        return bool.Parse(d);
    }
    public void SetInt(string keyname,int value)
    {
        SetKey(keyname, value.ToString());
    }
    public void SetBool(string keyname, bool value)
    {
        SetKey(keyname, value.ToString());
    }
    public bool HasKey(string keyname)
    {
        if (File.Exists(Application.persistentDataPath + "/Prefs.txt"))
        {
            List<string> data = new List<string>();
            data.AddRange(ss.ReadLines(Application.persistentDataPath + "/Prefs.txt"));
            for (int i = 0; i < data.ToArray().Length; i++)
            {
                if (data[i].Contains(keyname))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void DeleteKey(string keyname)
    {
        if (File.Exists(Application.persistentDataPath + "/Prefs.txt"))
        {
            List<string> data = new List<string>();
            data.AddRange(ss.ReadLines(Application.persistentDataPath + "/Prefs.txt"));
            for (int i = 0; i < data.ToArray().Length; i++)
            {
                if (data[i].Contains(keyname))
                {
                    data.Remove(data[i]);
                }
            }
            ss.WriteLines(Application.persistentDataPath + "/Prefs.txt", data.ToArray());
        }
    }
    /*public void Dump()
    {
        string[] lines = ss.ReadLines(Application.persistentDataPath + "/Prefs.txt");
        File.WriteAllLines(Application.persistentDataPath + "/PrefsDUMP.txt", lines);
    }*/
}