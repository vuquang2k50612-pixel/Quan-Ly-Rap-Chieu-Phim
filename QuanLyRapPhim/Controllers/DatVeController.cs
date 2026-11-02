using QuanLyRapPhim.Business;
using QuanLyRapPhim.Models;
using QuanLyRapPhim.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers
{
    public class DatVeController : Controller
    {
        private readonly DatVeService _datVeService = new DatVeService();

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        private DataClasses1DataContext GetDataContext()
        {
            return new DataClasses1DataContext(GetConnectionString());
        }

        // ==========================================
        // 1. TRANG CHỌN GHẾ (SCRUM-26)
        // ==========================================
        public ActionResult ChonGhe(int id)
        {
            try
            {
                Session["Time"] = DateTime.Now;

                using (var db = GetDataContext())
                {
                    var suat = _datVeService.LayThongTinSuatChieu(db, id);
                    if (suat == null)
                        return HttpNotFound("Không tìm thấy suất chiếu yêu cầu!");

                    ViewBag.TenPhim = suat.TenPhim;
                    ViewBag.TenPhong = suat.TenPhong;
                    ViewBag.TheLoai = suat.TheLoai;
                    ViewBag.ThoiLuong = suat.ThoiLuong;
                    ViewBag.GioChieu = suat.GioChieu?.ToString("dd/MM/yyyy HH:mm");
                    ViewBag.GiaVe = suat.Gia ?? 0;
                    ViewBag.IDSuatChieu = id;

                    var gheList = _datVeService.LayDanhSachGhe(db, id);
                    return View(gheList);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải sơ đồ ghế: " + ex.Message;
                return View(new List<GheViewModel>());
            }
        }

        [HttpPost]
        public JsonResult DatVe(int idSuatChieu, int[] gheIds)
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var result = _datVeService.DatVe(db, idSuatChieu, gheIds);
                    return Json(new { success = result.Success, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xử lý đặt ghế: " + ex.Message });
            }
        }

        // ==========================================
        // 2. TRANG CHỌN ĐỒ ĂN (SCRUM-27)
        // ==========================================
        public ActionResult ChonDoAn(int id, int? idTK)
        {
            ViewBag.IDSuatChieu = id;
            int tkId = idTK.HasValue && idTK.Value > 0 ? idTK.Value : (Session["ID_TK"] != null ? Convert.ToInt32(Session["ID_TK"]) : 0);
            ViewBag.ID_TK = tkId;
            return View();
        }

        [HttpGet]
        public string DSDoAn()
        {
            using (var db = GetDataContext())
            {
                var ds = db.QuanLyDoAns
                    .Where(o => o.TrangThai == "Còn hàng" && o.SoLuong > 0)
                    .Select(o => new
                    {
                        o.ID,
                        o.TenDoAn,
                        o.Gia,
                        o.MoTa,
                        o.SoLuong
                    })
                    .ToList();

                return JsonConvert.SerializeObject(ds);
            }
        }

        [HttpGet]
        public string LayChiTietPhim(int id)
        {
            using (var db = GetDataContext())
            {
                var phim = db.SuatChieus
                    .Where(p => p.ID_SuatChieu == id)
                    .Select(p => new
                    {
                        ID_Phim = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.ID_Phim : 0,
                        TenPhim = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.TenPhim : "",
                        TheLoai = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.TheLoai : "",
                        ThoiLuong = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.ThoiLuong : 120,
                        GioChieu = p.GioChieu.HasValue ? p.GioChieu.Value.ToString("HH:mm dd/MM/yyyy") : "",
                        TenPhong = p.Phong_Chieu != null ? p.Phong_Chieu.TenPhong : ""
                    })
                    .FirstOrDefault();

                return JsonConvert.SerializeObject(phim);
            }
        }

        // ==========================================
        // 3. TRANG THANH TOÁN (SCRUM-28)
        // ==========================================
        [HttpGet]
        public ActionResult ThanhToan(int id, string ghe, decimal tongVe = 0, string doAn = null, int? idTK = null)
        {
            using (var db = GetDataContext())
            {
                var suat = db.SuatChieus
                    .Where(p => p.ID_SuatChieu == id)
                    .Select(p => new
                    {
                        TenPhim = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.TenPhim : "Phim chiếu rạp",
                        TheLoai = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.TheLoai : "",
                        ThoiLuong = p.QLDanh_Sach_Phim != null ? p.QLDanh_Sach_Phim.ThoiLuong : 120,
                        GioChieu = p.GioChieu,
                        TenPhong = p.Phong_Chieu != null ? p.Phong_Chieu.TenPhong : "Phòng chiếu"
                    })
                    .FirstOrDefault();

                if (suat == null)
                {
                    return HttpNotFound("Không tìm thấy thông tin suất chiếu để thanh toán!");
                }

                decimal parsedTongDoAn = 0;
                string queryTongDoAn = Request.QueryString["tongDoAn"];
                if (!string.IsNullOrEmpty(queryTongDoAn))
                {
                    decimal.TryParse(queryTongDoAn, out parsedTongDoAn);
                }

                int resolvedIdTK = idTK.HasValue && idTK.Value > 0
                    ? idTK.Value
                    : (Session["ID_TK"] != null ? Convert.ToInt32(Session["ID_TK"]) : 0);

                decimal tongThanhToan = tongVe + parsedTongDoAn;

                ViewBag.TenPhim = suat.TenPhim;
                ViewBag.TheLoai = suat.TheLoai;
                ViewBag.ThoiLuong = suat.ThoiLuong;
                ViewBag.GioChieu = suat.GioChieu?.ToString("HH:mm - dd/MM/yyyy");
                ViewBag.TenPhong = suat.TenPhong;
                ViewBag.IDSuatChieu = id;
                ViewBag.GheChon = ghe ?? "";
                ViewBag.TongVe = tongVe;
                ViewBag.DoAn = string.IsNullOrEmpty(doAn) ? "[]" : doAn;
                ViewBag.TongDoAn = parsedTongDoAn;
                ViewBag.TongThanhToan = tongThanhToan;
                ViewBag.ID_TK = resolvedIdTK;

                // QR Code thanh toán mặc định & VietQR động
                ViewBag.QRCode = "/img/qr-thanh-toan.jpg";

                string bankCode = "MB";
                string accNum = "0025168888";
                string cleanGhe = (ghe ?? "").Replace(" ", "").Replace(",", "-");
                string memo = $"CINEMA-SC{id}-GHE{cleanGhe}";
                long amount = (long)tongThanhToan;
                ViewBag.VietQR = $"https://img.vietqr.io/image/{bankCode}-{accNum}-compact2.jpg?amount={amount}&addInfo={Uri.EscapeDataString(memo)}";

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
                {
                    idTK = Convert.ToInt32(Session["ID_TK"]);
                }

                using (var db = GetDataContext())
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
