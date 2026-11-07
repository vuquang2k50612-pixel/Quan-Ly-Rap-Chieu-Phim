using QuanLyRapPhim.Business;
using QuanLyRapPhim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyRapPhim
{
    public class DatVeController : Controller
    {
        public ActionResult ChonGhe(int id)
        {
            try
            {
                Session["Time"] = DateTime.Now;

                var db = new DataClasses1DataContext(); 
                var suat = db.SuatChieus.FirstOrDefault(s => s.ID_SuatChieu == id);
                if (suat == null)
                    return HttpNotFound("Không tìm thấy suất chiếu!");

                ViewBag.TenPhim = suat.QLDanh_Sach_Phim.TenPhim;
                ViewBag.TenPhong = suat.Phong_Chieu.TenPhong;
                ViewBag.GioChieu = suat.GioChieu?.ToString("dd/MM/yyyy HH:mm");
                ViewBag.GiaVe = suat.Gia;
                ViewBag.IDSuatChieu = id;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message;
                return View();
            }
        }
    }
}