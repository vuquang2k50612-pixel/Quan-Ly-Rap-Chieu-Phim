using QuanLyRapPhim.Business;
using QuanLyRapPhim.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers
{
    public class SuatChieuController : Controller
    {
        private readonly SuatChieuService _suatChieuService = new SuatChieuService();

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        private DataClasses1DataContext GetDataContext()
        {
            return new DataClasses1DataContext(GetConnectionString());
        }

        // GET: SuatChieu
        public ActionResult DSP_SC()
        {
            return View();
        }

        public ActionResult ChiTietSuatChieu(int id)
        {
            using (var db = GetDataContext())
            {
                // Lấy danh sách suất chiếu theo ID phim
                var dsSuatChieu = db.SuatChieus
                    .Where(s => s.ID_Phim == id)
                    .ToList();

                // Lấy tên phim để hiển thị tiêu đề
                var tenPhim = db.QLDanh_Sach_Phims
                    .Where(p => p.ID_Phim == id)
                    .Select(p => p.TenPhim)
                    .FirstOrDefault();

                ViewBag.TenPhim = tenPhim;
                ViewBag.IDPhim = id;

                return View(dsSuatChieu);
            }
        }

        public string LayDSP_SC()
        {
            using (var db = GetDataContext())
            {
                var dsp_qr = db.QLDanh_Sach_Phims;

                if (dsp_qr.Any())
                {
                    // bỏ qua vòng lặp khi serialize JSON
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

        public ActionResult ThemSuatChieu()
        {
            return View();
        }

        public string LayDuLieuCombo()
        {
            try
            {
                using (var db = GetDataContext())
                {
                    var data = new
                    {
                        phim = db.QLDanh_Sach_Phims
                                 .Select(p => new { p.ID_Phim, p.TenPhim })
                                 .OrderBy(p => p.TenPhim)
                                 .ToList(),
                        phong = db.Phong_Chieus
                                  .Select(r => new { r.ID_Phong, r.TenPhong })
                                  .OrderBy(r => r.TenPhong)
                                  .ToList()
                    };

                    return JsonConvert.SerializeObject(data);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string ThemSC()
        {
            try
            {
                if (!int.TryParse(Request["ID_Phim"], out int ID))
                    return "Vui lòng chọn phim!";
                if (!int.TryParse(Request["ID_Phong"], out int Phong))
                    return "Vui lòng chọn phòng!";
                if (!DateTime.TryParse(Request["GioChieu"], out DateTime gioChieu))
                    return "Giờ chiếu không hợp lệ!";
                if (!decimal.TryParse(Request["Gia"], out decimal gia) || gia <= 0)
                    return "Giá vé phải lớn hơn 0!";

                using (var db = GetDataContext())
                {
                    var result = _suatChieuService.ThemSuatChieu(db, ID, Phong, gioChieu, gia);
                    return result.Message;
                }
            }
            catch (Exception ex)
            {
                return "Lỗi server: " + ex.Message;
            }
        }

        public ActionResult XemGhe(int id)
        {
            return View(id);
        }

        // Lấy danh sách ghế và trạng thái đã bán cho suất chiếu
        public string LayGhe(int id)
        {
            using (var db = GetDataContext())
            {
                var sc = db.SuatChieus.FirstOrDefault(x => x.ID_SuatChieu == id);
                if (sc == null) return "{\"success\":false}";
                int idPhong = sc.ID_Phong.Value;
                var gheList = db.Ghes
                    .Where(g => g.ID_Phong == idPhong)
                    .Select(g => new { g.ID_Ghe, g.So_Ghe })
                    .ToList();
                var gheDaBan = db.Ves
                    .Where(v => v.ID_SuatChieu == id)
                    .Select(v => v.ID_Ghe)
                    .ToList();
                var data = new
                {
                    success = true,
                    tenPhim = sc.QLDanh_Sach_Phim != null ? sc.QLDanh_Sach_Phim.TenPhim : "",
                    gioChieu = sc.GioChieu.HasValue ? sc.GioChieu.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    gheList,
                    gheDaBan
                };
                return JsonConvert.SerializeObject(data);
            }
        }

        public string XoaSC()
        {
            try
            {
                if (!int.TryParse(Request["id"], out int id))
                    return "ID suất chiếu không hợp lệ!";

                using (var db = GetDataContext())
                {
                    var result = _suatChieuService.XoaSuatChieu(db, id);
                    return result.Message;
                }
            }
            catch (Exception ex)
            {
                return "Lỗi server: " + ex.Message;
            }
        }
    }
}
