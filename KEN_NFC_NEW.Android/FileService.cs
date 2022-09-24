using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using KEN_NFC_NEW.Droid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(FileService))]
namespace KEN_NFC_NEW.Droid
{
    public class FileService : IFileService
    {
        public void SaveTextFile(string name, string text)
        {
            string rootPath = Application.Context.GetExternalFilesDir(null).ToString();
            var path = Path.Combine(rootPath, name);

            Java.IO.File dir = new Java.IO.File(rootPath);
            dir.Mkdir();

            try
            {
                System.IO.File.WriteAllText(path, text);
                System.Console.WriteLine("Wrote file to " + path);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("ERR: " + e.Message);
            }
        }

        public async Task SaveAndView(string fileName, String contentType, MemoryStream stream)
        {
            try
            {
                string root = null;
                //Get the root path in android device.
                if (Android.OS.Environment.IsExternalStorageEmulated)
                {
                    root = Android.OS.Environment.ExternalStorageDirectory.ToString();
                }
                else
                    root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

                //Create directory and file 
                Java.IO.File myDir = new Java.IO.File(root + "/Download");
                myDir.Mkdir();

                Java.IO.File file = new Java.IO.File(myDir, fileName);

                //Remove if the file exists
                //if (file.Exists()) file.Delete();

                //Write the stream into the file
                FileOutputStream outs = new FileOutputStream(file);
                outs.Write(stream.ToArray());

                outs.Flush();
                outs.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("err::: " + ex.Message);
                //PostLog.AppCenterLogExcecao(ex, new Dictionary<string, string> { { "origem", "OrderViewModel - 159" } });
            }
        }
    }
}