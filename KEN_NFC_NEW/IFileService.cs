using System;
using System.IO;
using System.Threading.Tasks;

namespace KEN_NFC_NEW
{
    public interface IFileService
    {
        void SaveTextFile(string name, string content);
        Task SaveAndView(string fileName, String contentType, MemoryStream stream);
    }
}