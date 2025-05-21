using condominio_API.Data;
using condominio_API.Models;
using Condominio_API.Utilitarios; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCoder;

namespace Condominio_API.Utilitarios
{
    public static class ByteToImage
    {
        public static byte[] ToByteArray(this System.Drawing.Bitmap bitmap)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}