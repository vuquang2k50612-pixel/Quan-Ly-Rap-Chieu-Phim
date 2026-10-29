using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using QuanLyRapPhim.Models;

namespace CINEMA_K68.Controllers
{
    public class DSPController : Controller
    {
        // Hàm lấy chuỗi kết nối chuẩn từ Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        public string LayDSP()
        {
            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                var dsp_qr = db.QLDanh_Sach_Phims;

                if (dsp_qr.Any())
                {
                    // Bỏ qua vòng lặp khi serialize JSON
                    string rs = JsonConvert.SerializeObject(dsp_qr.ToList(),
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

                    return rs;
                }
                else
                {
                    return "DSP rỗng";
                }
            }
        }

        public string LayTrailerHeader()
        {
            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                var trailerData = db.QLDanh_Sach_Phims
                    .Where(p => p.TrailerID != null && p.TrailerID != "")
                    .Select(p => new
                    {
                        id = p.TrailerID,
                        title = p.TenPhim,
                        background = p.TenPhim + ".jpg"
                    })
                    .ToList(); 

                string rs = JsonConvert.SerializeObject(trailerData,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                );

                return rs;
            }
        }
    }
}
