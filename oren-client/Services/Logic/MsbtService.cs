using System.IO.Compression;
using Ionic.Zip;
using Microsoft.AspNetCore.Http;
using oren_client.Lib.Formats;
using oren_client.Models;
using oren_client.Services.Interfaces;

namespace oren_client.Services.Logic;

public class MsbtService : IMsbtService
{
    public async Task<List<string>> MsbtToStringList(IFormFile msbtFile)
    {
        MSBT msbt = new(msbtFile.OpenReadStream(), msbtFile.FileName);
        
        return msbt.MessagesToStringList();
    }

    public async Task<List<MsbtSheet>> ZipToSheets(IFormFile zipWithMsbts)
    {
        string tempFolderName = "temp\\";
        ZipInputStream zipStream = new(zipWithMsbts.OpenReadStream());
        unpackZip(zipStream, tempFolderName);

        bool isMultilanguage = Directory.GetFiles(tempFolderName).Length == 0 && Directory.GetDirectories(tempFolderName).Length != 0;
        
        List<string> languageDirectories = Directory.GetDirectories(tempFolderName).ToList();
        Dictionary<string, Dictionary<string, MSBT>> languages = new();

        foreach (var languageDirectory in languageDirectories)
        {
            string languageName = languageDirectory.Substring(languageDirectory.IndexOf('\\') + 1);
            List<string> msbtPaths = Directory.GetFiles(languageDirectory).ToList();
            
            Dictionary<string, MSBT> msbts = new();
            foreach (var msbtPath in msbtPaths)
            {
                if (Path.GetExtension(msbtPath) != ".msbt")
                {
                    continue;
                }
                byte[] msbtBytes = File.ReadAllBytes(msbtPath);
                string msbtName = Path.GetFileName(msbtPath);
                MSBT msbt = new(new MemoryStream(msbtBytes), msbtName, languageName);
                
                msbts.Add(msbtName, msbt);
            }
            
            languages.Add(languageName, msbts);
        }

        List<MsbtSheet> sheets = new();

        string baseLanguageName = languages.First().Key;

        foreach (var msbtPair in languages[baseLanguageName])
        {
            MsbtSheet sheet = new();
            
            List<string> labels = new();
            labels.Add("Labels");
            
            string msbtName = msbtPair.Key;
            MSBT baseLanguageMsbt = msbtPair.Value;
            
            foreach (var message in baseLanguageMsbt.Messages)
            {
                labels.Add(message.Key.ToString());
            }

            sheet.Labels = labels;
            sheet.Version = baseLanguageMsbt.Header.Version;
            sheet.IsBigEndian = baseLanguageMsbt.Header.Endianness == Endianness.BigEndian;
            sheet.UseIndices = baseLanguageMsbt.HasNLI1;
            sheet.UseStyles = baseLanguageMsbt.HasTSY1;
            sheet.UseAttributes = baseLanguageMsbt.HasATR1;
            sheet.BytesPerAttribute = (int)baseLanguageMsbt.BytesPerAttribute;
            sheet.Encoding = baseLanguageMsbt.Header.EncodingType.ToString();

            List<List<string>> columns = new();

            foreach (var languageEntry in languages)
            {
                List<string> column = new();
                
                string languageName = languageEntry.Key;
                column.Add(languageName);
                for (int i = 1; i < labels.Count; i++)
                {
                    string label = labels[i];
                    column.Add(languageEntry.Value[msbtName].Messages[label].Text);
                }
                
                columns.Add(column);
            }
            
            sheet.Columns = columns;
            
            sheets.Add(sheet);
        }
        
        Directory.Delete(tempFolderName, true);
        
        return sheets;
    }

    private void unpackZip(ZipInputStream zipStream, string unpackDirName)
    {
        string debug = "";
        ZipEntry entry;
        while ((entry = zipStream.GetNextEntry()) != null)
        {
            if (entry.FileName.EndsWith("/"))
            {
                Directory.CreateDirectory($"{unpackDirName}{entry.FileName.Replace(@"/", @"\")}");
            }
            else
            {
                string path = $"{unpackDirName}{entry.FileName.Replace(@"/", @"\")}";
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                FileStream streamWriter = File.Create($"{unpackDirName}\\{entry.FileName.Replace(@"/", @"\")}");
                
                int size = 1024 * 10;
                Byte[] data = new Byte[size];
                while (true)
                {
                    size = zipStream.Read(data, 0, data.Length);
                    if (size > 0)
                    {
                        streamWriter.Write(data, 0, (int)size);
                    }
                    else break;
                }
                streamWriter.Close();
            }
        }
    }
}