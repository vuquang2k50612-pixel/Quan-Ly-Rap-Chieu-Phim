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
        // 1. Mở trang Danh Sách Phim (Giao diện Quản lý)
        public ActionResult DSP()
        {
            return View();
        }

        public ActionResult ThemDSP()
        {
            return View();
        }

        public ActionResult SuaDSP()
        {
            return View();
        }

        // Xem chi tiết phim
        public ActionResult ChiTiet(int id)
        {
        
            DataClasses1DataContext db = new DataClasses1DataContext();
            var phim = db.QLDanh_Sach_Phims.FirstOrDefault(p => p.ID_Phim == id);
            if (phim == null)
                return Content("Lỗi: không tìm thấy phim");

            return View(phim);
        }




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

        // 3. API lấy chi tiết 1 phim theo ID
        public string LayChiTietPhim(int id)
        {
            DataClasses1DataContext db = new DataClasses1DataContext();
            var phim = db.QLDanh_Sach_Phims.FirstOrDefault(p => p.ID_Phim == id);
            if (phim == null)
                return JsonConvert.SerializeObject(new { error = "Không tìm thấy phim" });

            return JsonConvert.SerializeObject(new
            {
                ID_Phim = phim.ID_Phim,
                TenPhim = phim.TenPhim,
                TheLoai = phim.TheLoai,
                ThoiLuong = phim.ThoiLuong,
                MoTa = phim.MoTa,
                NgayPhatHanh = phim.NgayPhatHanh
            });
        }
    
   

           
            // 4. API xóa phim
            public string XoaPhim(string TenPhim)
            {
                try
                {
                    DataClasses1DataContext db = new DataClasses1DataContext();
                    var phim = db.QLDanh_Sach_Phims.FirstOrDefault(p => p.TenPhim == TenPhim);
                    if (phim == null)
                        return "Không tìm thấy phim để xóa!";

                    db.QLDanh_Sach_Phims.DeleteOnSubmit(phim);
                    db.SubmitChanges();

                    return "Xóa phim thành công!";
                }
                catch (Exception ex)
                {
                    return "Lỗi khi xóa phim: " + ex.Message;
                }
            }

            // 5. API thêm phim mới
            public string ThemPhim(string TenPhim, string TheLoai, string ThoiLuong, string MoTa, string NgayPhatHanh, string TrailerID, HttpPostedFileBase AnhPhim)
            {
                try
                {
                    DataClasses1DataContext db = new DataClasses1DataContext();
                    QLDanh_Sach_Phim phim = new QLDanh_Sach_Phim();
                    phim.TenPhim = TenPhim;
                    phim.TheLoai = TheLoai;

                    if (!string.IsNullOrEmpty(ThoiLuong))
                        phim.ThoiLuong = int.Parse(ThoiLuong);

                    phim.MoTa = MoTa;
                    phim.TrailerID = TrailerID;

                    if (DateTime.TryParse(NgayPhatHanh, out DateTime NgayKhoiChieu))
                        phim.NgayPhatHanh = NgayKhoiChieu;

                    db.QLDanh_Sach_Phims.InsertOnSubmit(phim);
                    db.SubmitChanges();

                    // Lưu ảnh poster vào thư mục ~/img/
                    if (AnhPhim != null && AnhPhim.ContentLength > 0)
                    {
                        
                        var fileName = phim.TenPhim + ".jpg";

                    var path = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/img/"), fileName);
                }

                    return "Thêm phim thành công!";
                }
                catch (Exception ex)
                {
                    return "Lỗi khi thêm phim: " + ex.Message;
                }
            }

            // 6. API lấy thông tin phim để điền vào form sửa
            public string LayTTP()
            {
                try
                {
                    string id_str = Request["id"];
                    if (string.IsNullOrEmpty(id_str) || !int.TryParse(id_str, out int id))
                        return "{\"error\":\"ID phim không hợp lệ!\"}";

                    DataClasses1DataContext db = new DataClasses1DataContext();
                    var phim = db.QLDanh_Sach_Phims.FirstOrDefault(p => p.ID_Phim == id);

                    if (phim == null)
                        return "{\"error\":\"Không tìm thấy phim có ID = " + id + "\"}";

                    var result = new
                    {
                        ID_Phim = phim.ID_Phim,
                        TenPhim = phim.TenPhim ?? "",
                        TheLoai = phim.TheLoai ?? "",
                        ThoiLuong = phim.ThoiLuong ?? 0,
                        MoTa = phim.MoTa ?? "",
                        NgayPhatHanh = phim.NgayPhatHanh?.ToString("yyyy-MM-dd") ?? ""
                    };

                    return Newtonsoft.Json.JsonConvert.SerializeObject(result);
                }
                catch (Exception ex)
                {
                    return "{\"error\":\"Lỗi server: " + ex.Message.Replace("\"", "\\\"") + "\"}";
                }
            }

            // 7. API cập nhật phim đã sửa
            public string SuaPhim()
            {
                try
                {
                    string id_str = Request["ID_Phim"];
                    if (!int.TryParse(id_str, out int ID_Phim))
                        return "ID phim không hợp lệ!";

                    string TenPhim = Request["TenPhim"];
                    if (string.IsNullOrEmpty(TenPhim))
                        return "Tên phim không được để trống!";

                    string TheLoai = Request["TheLoai"];
                    string MoTa = Request["MoTa"];

                    string tl_str = Request["ThoiLuong"];
                    if (!int.TryParse(tl_str, out int ThoiLuong) || ThoiLuong <= 0)
                        return "Thời lượng phải là số dương!";

                    string ngay_str = Request["NgayPhatHanh"];
                    DateTime? NgayPhatHanh = null;
                    if (!string.IsNullOrEmpty(ngay_str))
                    {
                        if (DateTime.TryParse(ngay_str, out DateTime temp))
                            NgayPhatHanh = temp;
                    }

                    DataClasses1DataContext db = new DataClasses1DataContext();
                    var phim = db.QLDanh_Sach_Phims.FirstOrDefault(p => p.ID_Phim == ID_Phim);
                    if (phim == null)
                        return "Không tìm thấy phim để sửa!";

                    phim.TenPhim = TenPhim;
                    phim.TheLoai = TheLoai;
                    phim.ThoiLuong = ThoiLuong;
                    phim.MoTa = MoTa;
                    phim.NgayPhatHanh = NgayPhatHanh;

                    db.SubmitChanges();

                    return "Sửa phim thành công!";
                }
                catch (Exception ex)
                {
                    return "Lỗi server: " + ex.Message;
                }
            }
        }
    }   
   
    