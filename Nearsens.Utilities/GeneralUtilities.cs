using Nearsens.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nearsens.Utilities
{
    public static class GeneralUtilities
    {
        public static void DeleteFromFileSystem(this DeletePhotosCommand dpc, HttpServerUtility server)
        {
            for (int i = 0; i < dpc.PhotosId.Length; i++)
            {
                string fileToDelete = server.MapPath("~\\" + dpc.PhotosPath[i]);
                if (File.Exists(fileToDelete))
                {
                    File.Delete(fileToDelete);
                }
            }
        }
    }
}
