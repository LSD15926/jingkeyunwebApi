using System;
using System.IO;
using System.DrawingCore;
using ZXing.Common;
using ZXing;
using ZXing.QrCode;
using ZXing.ZKWeb;

namespace jingkeyun.Class
{
    public static class QrCodeHelper
    {
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static Bitmap GeneratorQrImage(string contents, int width = 200, int height = 200)
        {
            if (string.IsNullOrEmpty(contents))
            {
                return null;
            }
            EncodingOptions options = null;
            BarcodeWriter writer = null;
            options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
            };
            writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;

            Bitmap bitmap = writer.Write(contents);
            return bitmap;
        }
    }
}
