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
        // Trang chọn đồ ăn 
        public ActionResult ChonDoAn(int id, int idTK)
        {
            ViewBag.IDSuatChieu = id;
            ViewBag.ID_TK = idTK;
            return View();
        }

        // API lấy danh sách combo đồ ăn còn hàng
        public string DSDoAn()
        {
            var db = new DataClasses1DataContext();
            var ds = db.QuanLyDoAns.Where(o => o.TrangThai == "Còn hàng").Select(o => new
            {
                o.ID,
                o.TenDoAn,
                o.Gia,
                o.MoTa,
                o.SoLuong
            }).ToList();
            return Newtonsoft.Json.JsonConvert.SerializeObject(ds);
        }

        // API lấy chi tiết phim đang đặt
        public string LayChiTietPhim(int id)
        {
            var db = new DataClasses1DataContext();
            var phim = db.SuatChieus.Where(p => p.ID_SuatChieu == id).Select(p => new
            {
                p.QLDanh_Sach_Phim.ID_Phim,
                p.QLDanh_Sach_Phim.TenPhim,
                p.QLDanh_Sach_Phim.TheLoai,
                p.QLDanh_Sach_Phim.ThoiLuong,
                p.GioChieu,
                p.Phong_Chieu.TenPhong
            }).FirstOrDefault();
            return Newtonsoft.Json.JsonConvert.SerializeObject(phim);
        }
    }
}