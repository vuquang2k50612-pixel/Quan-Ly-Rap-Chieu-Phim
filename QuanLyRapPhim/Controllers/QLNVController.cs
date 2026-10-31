using QuanLyRapPhim.Business;
using Newtonsoft.Json;
using QuanLyRapPhim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers
{
    public class QLNVController : Controller
    {
        private readonly NhanVienService _nhanVienService = new NhanVienService();

        public ActionResult DSNV()
        {
            return View();
        }

        public ActionResult ThemNV()
        {
            return View();
        }

        public ActionResult ChinhSua()
        {
            return View();
        }

        public ActionResult XoaNV()
        {
            return View();
        }

        public string Insert()
        {
            try
            {
                string hoten = Request["txt_HoTen"];
                string ngaysinh_str = Request["txt_NgaySinh"];
                string thoigiantuyen_str = Request["txt_ThoiGianTuyen"];
                string matkhau = Request["txt_MatKhau"];
                string gioitinh = Request["slc_GioiTinh"];
                string id = Request["txt_MNV"];
                string sdt = Request["txt_SDT"];
                string email = Request["txt_Email"];
                string vaitro = Request["txt_VaiTro"];
                string luong_str = Request["txt_Luong"];

                if (hoten == "" || ngaysinh_str == "" || thoigiantuyen_str == "" || gioitinh == "" || id == "" || sdt == "" || email == "" || vaitro == "" || luong_str == "" || matkhau == "")
                {
                    return "Vui lòng điền đầy đủ thông tin";
                }

                if (!email.EndsWith("@gmail.com"))
                {
                    return "Email không hợp lệ";
                }

                decimal luong;
                if (!decimal.TryParse(luong_str, out luong))
                {
                    return "Lương không hợp lệ";
                }

                DateTime ngaysinh;
                if (!DateTime.TryParse(ngaysinh_str, out ngaysinh))
                {
                    return "Ngày sinh không hợp lệ";
                }

                DateTime thoigiantuyen;
                if (!DateTime.TryParse(thoigiantuyen_str, out thoigiantuyen))
                {
                    return "Ngày tuyển không hợp lệ";
                }

                DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
                NhanVien nv = new NhanVien
                {
                    TenNV = hoten,
                    NgaySinh = ngaysinh,
                    PasswordHash = matkhau,
                    ThoiGianTuyen = thoigiantuyen,
                    GioiTinh = gioitinh,
                    ID_NV = id,
                    SDT = sdt,
                    Email = email,
                    VaiTro = vaitro,
                    Luong = luong
                };

                var result = _nhanVienService.ThemNhanVien(db, nv);
                return result.Message;
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.Message;
            }
        }

        public string Update(string id)
        {
            try
            {
                DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
                var nv_qr = db.NhanViens.Where(o => o.ID_NV == id);
                if (nv_qr.Any())
                {
                    string hoten = Request["txt_HoTen"];
                    string ngaysinh_str = Request["txt_NgaySinh"];
                    string matkhau = Request["txt_MatKhau"];
                    string thoigiantuyen_str = Request["txt_ThoiGianTuyen"];
                    string gioitinh = Request["slc_GioiTinh"];
                    string sdt = Request["txt_SDT"];
                    string email = Request["txt_Email"];
                    string vaitro = Request["txt_VaiTro"];
                    string luong_str = Request["txt_Luong"];

                    decimal luong;
                    if (!decimal.TryParse(luong_str, out luong))
                    {
                        return "Lương không hợp lệ";
                    }

                    if (!email.EndsWith("@gmail.com"))
                    {
                        return "Email không hợp lệ";
                    }

                    DateTime ngaysinh;
                    if (!DateTime.TryParse(ngaysinh_str, out ngaysinh))
                    {
                        return "Ngày sinh không hợp lệ";
                    }

                    DateTime thoigiantuyen;
                    if (!DateTime.TryParse(thoigiantuyen_str, out thoigiantuyen))
                    {
                        return "Thời gian tuyển không hợp lệ";
                    }

                    NhanVien nvMoi = new NhanVien
                    {
                        ID_NV = id,
                        TenNV = hoten,
                        NgaySinh = ngaysinh,
                        PasswordHash = matkhau,
                        ThoiGianTuyen = thoigiantuyen,
                        GioiTinh = gioitinh,
                        Email = email,
                        VaiTro = vaitro,
                        SDT = sdt,
                        Luong = luong
                    };

                    var result = _nhanVienService.CapNhatNhanVien(db, nvMoi);
                    return result.Message;
                }
                else
                {
                    return "Không tìm thấy nhân viên!";
                }
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.Message;
            }
        }

        public string Lay_DSNV()
        {
            DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
            var nv_pr = db.NhanViens.ToList();
            if (nv_pr.Any())
            {
                string rs = JsonConvert.SerializeObject(nv_pr);
                return rs;
            }
            else
            {
                return "Không có nhân viên nào trong hệ thống.";
            }
        }

        public string LayTTNV(string id)
        {
            DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
            var nv_pr = db.NhanViens.Where(o => o.ID_NV == id);
            if (nv_pr.Any())
            {
                string rs = JsonConvert.SerializeObject(nv_pr.SingleOrDefault());
                return rs;
            }
            else
            {
                return "Không tìm thấy nhân viên!";
            }
        }

        public string XoaNV_Ajax(string id)
        {
            try
            {
                DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
                var result = _nhanVienService.XoaNhanVien(db, id);
                return result.Message;
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.Message;
            }
        }
    }
}
