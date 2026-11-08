using QuanLyRapPhim.Business;
using QuanLyRapPhim.Models;
using QuanLyRapPhim.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyRapPhim
{
    public class DatVeController : Controller
    {
        private readonly DatVeService _datVeService = new DatVeService();

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

        // ==========================================
        // TRANG THANH TOÁN
        // ==========================================
        public ActionResult ThanhToan(int id, string ghe = null, decimal tongVe = 0, string doAn = null, int? idTK = null)
        {
            using (var db = new DataClasses1DataContext())
            {
                var suat = db.SuatChieus.FirstOrDefault(p => p.ID_SuatChieu == id);
                if (suat == null)
                    return HttpNotFound("Không tìm thấy thông tin suất chiếu!");

                ViewBag.TenPhim = suat.QLDanh_Sach_Phim != null ? suat.QLDanh_Sach_Phim.TenPhim : "Phim chiếu rạp";
                ViewBag.GioChieu = suat.GioChieu?.ToString("HH:mm  dd/MM/yyyy");
                ViewBag.TenPhong = suat.Phong_Chieu != null ? suat.Phong_Chieu.TenPhong : "Phòng chiếu";
                ViewBag.IDSuatChieu = id;

                // Fallback dữ liệu nếu mở trực tiếp link
                ViewBag.GheChon = string.IsNullOrEmpty(ghe) ? "A1, A2" : ghe;
                decimal giaSuat = suat.Gia ?? 60000;
                ViewBag.TongVe = tongVe > 0 ? tongVe : giaSuat;
                ViewBag.DoAn = string.IsNullOrEmpty(doAn) ? "[]" : doAn;

                decimal tongDoAn = 0;
                string queryTongDoAn = Request.QueryString["tongDoAn"];
                if (!string.IsNullOrEmpty(queryTongDoAn))
                {
                    decimal.TryParse(queryTongDoAn, out tongDoAn);
                }
                ViewBag.TongDoAn = tongDoAn;
                ViewBag.TongThanhToan = (decimal)ViewBag.TongVe + tongDoAn;

                int resolvedIdTK = idTK.HasValue && idTK.Value > 0
                    ? idTK.Value
                    : (Session["ID_TK"] != null ? Convert.ToInt32(Session["ID_TK"]) : 0);
                ViewBag.ID_TK = resolvedIdTK;
                ViewBag.QRCode = "/img/qr-thanh-toan.jpg";

                return View();
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult XacNhanThanhToan(int id, string gheChon, string tenDoAn, int soLuong, List<DoAnItem> doAnList, string tongTien, int idTK)
        {
            try
            {
                if (idTK <= 0 && Session["ID_TK"] != null)
                    idTK = Convert.ToInt32(Session["ID_TK"]);

                using (var db = new DataClasses1DataContext())
                {
                    var result = _datVeService.XacNhanThanhToan(db, id, gheChon, tenDoAn, soLuong, doAnList, tongTien, idTK);
                    return Json(new { success = result.Success, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi trong quá trình thanh toán: " + ex.Message });
            }
        }
    }
}
