using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace WordShuffle.Utils
{
    public static class FileIOEx
    {
        public static async Task<List<string>> ReadFileAsync(StorageFolder folder, string fileName,string encodingStr)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(encodingStr);
            var files = await folder.GetFilesAsync();
            List<string> result = new List<string>();
            StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            var buff = await FileIO.ReadBufferAsync(saveFile);
            using (DataReader reader = DataReader.FromBuffer(buff))
            {
                byte[] dataBuff = new byte[buff.Length];
                reader.ReadBytes(dataBuff);

                string dataStr = encoding.GetString(dataBuff, 0, dataBuff.Length);

                result = dataStr.Split('\n').ToList();
                if(result.Count > 0 && result.First() == string.Empty)
                {
                    result.Clear();
                }
            }

            return result;
        }

        public static async Task SaveFileAsync(StorageFolder folder, string fileName, List<string> lines,string encodingStr)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(encodingStr);
            var saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            string text = string.Join("\n", lines);
            byte[] buff = encoding.GetBytes(text);
            await FileIO.WriteBytesAsync(saveFile, buff);
        }
    }
}
