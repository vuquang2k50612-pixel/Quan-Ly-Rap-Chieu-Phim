using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using QuanLyRapPhim.Models;
using System.Configuration; // Bắt buộc có thư viện này để đọc Web.config

namespace CINEMA_K68.Controllers
{
    public class DangNhapNhanVienController : Controller
    {
        // Hàm lấy chuỗi kết nối chuẩn từ Web.config
        private string GetConnectionString()
        {
            // Lấy chính xác chuỗi có tên "PhimConnectionString"
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        [HttpGet]
        public ActionResult DangNhapNhanVien()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhapNhanVien(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // KẾT NỐI: Truyền trực tiếp chuỗi kết nối vào đây để không bao giờ sai
            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                try
                {
                    // Tìm nhân viên
                    var nv = db.NhanViens.SingleOrDefault(n => n.SDT == username);

                    if (nv == null)
                    {
                        ViewBag.Error = "Tài khoản không tồn tại!";
                        return View();
                    }

                    if (nv.PasswordHash == password)
                    {
                        // Đăng nhập thành công
                        FormsAuthentication.SetAuthCookie(nv.ID_NV, false);
                        Session["Role"] = nv.VaiTro;
                        Session["ID_NV"] = nv.ID_NV;
                        Session["TenNV"] = nv.TenNV;

                        if (nv.VaiTro == "Quản lý" || nv.VaiTro == "Admin")
                        {
                            return RedirectToAction("DSNV", "QLNV");
                        }
                        else
                        {
                            return RedirectToAction("ThongTinNhanVien", "ThongTinNhanVien");
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Mật khẩu không đúng!";
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    // Bắt lỗi kết nối ở đây để hiển thị ra màn hình cho dễ sửa
                    ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                    return View();
                }
            }
        }

        public ActionResult DangXuat()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("DangNhapNhanVien");
        }
    }
}