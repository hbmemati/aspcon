using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;


namespace rgb565_converter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                string veri= context.Request.Query["id"].ToString();

                
                if (veri!="mamo")
                {
                    await context.Response.WriteAsync("index");
                }
                else
                {
                    string base64_image = context.Request.Form["image"];

                    byte[] bytes2 = Convert.FromBase64String(base64_image);

                    Image image;

                    using (MemoryStream ms = new MemoryStream(bytes2))
                    {
                        image = Image.FromStream(ms);
                    }


                    StringBuilder result = new StringBuilder();
                    string codeFormat = "";
                    int bitInBlock = 7; // For BW/1bpp conversion
                    bool is_1bpp = false;


                    int Width = image.Width;
                    int Height = image.Height;

                    result.Append("uint16_t");
                    codeFormat = "0x{0:x4}, ";

                    result.Append(" image = {" + Environment.NewLine + "\t");



                    using (Bitmap bmp = new Bitmap(image, Width, Height))
                    {
                        int rowPos = 0;

                        int pixelsTotal = bmp.Width * bmp.Height;
                        int pixelsCurrent = 0;
                        int ColorByte = 0;



                        // ===========================================================
                        // Optimisation: Upto 5x faster than GetPixel();
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        System.Drawing.Imaging.BitmapData bmpData =
                            bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                            bmp.PixelFormat);

                        IntPtr ptr = bmpData.Scan0;
                        int bytes = bmpData.Stride * bmp.Height;    // ARGB: Width * Height * 4 (Stride = Width * 4)
                        byte[] rgbValues = new byte[bytes];

                        // Format BGRA (GRB+Alpha, inverted). Example: BBBBBBBB GGGGGGGG RRRRRRRR AAAAAAAA
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                        int pixelByte = 0;
                        // ===========================================================

                        // Read image pixel's from left to right, top to bottom (Like a book)
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                pixelByte = (y * bmp.Width + x) * 4;

                                ColorByte =
                                         (rgbValues[pixelByte + 2] & 0xF8) << 8 |    // 0xF8 = 1111 1000
                                         (rgbValues[pixelByte + 1] & 0xFC) << 3 |    // 0xFC = 1111 1100
                                         (rgbValues[pixelByte + 0] & 0xF8) >> 3;


                                if (!is_1bpp || bitInBlock == 0)
                                {
                                    result.AppendFormat(codeFormat, ColorByte);
                                    pixelsCurrent++;
                                    rowPos++;

                                    if (rowPos == 16)
                                    {
                                        rowPos = 0;
                                        result.Append(Environment.NewLine + "\t");
                                    }

                                    bitInBlock = 7; // BW/1bpp bit position reset
                                    ColorByte = 0;  // Reset data
                                }
                                else
                                {
                                    bitInBlock--;
                                }
                            }


                        }
                    }

                    result.Append(Environment.NewLine + "};");



                    await context.Response.WriteAsync(result.ToString());

                }




            });
        }
    }
}
