using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using QuanLyRapPhim.Models;

namespace CINEMA_K68.Controllers
{
    public class DNKHController : Controller
    {
        // Hàm lấy chuỗi kết nối chuẩn từ Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        public ActionResult DNKH()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DNKH(string tentk, string matkhau)
        {
            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                try
                {
                    // Kiểm tra tài khoản có tồn tại hay không
                    var tk = db.TaiKhoans.FirstOrDefault(x => x.TenTK == tentk);

                    if (tk == null)
                    {
                        ViewBag.ThongBao = "Tên tài khoản hoặc mật khẩu không đúng!";
                        return View();
                    }

                    if (tk.MatKhau != matkhau)
                    {
                        ViewBag.ThongBao = "Tên tài khoản hoặc mật khẩu không đúng!";
                        return View();
                    }
                    // Nếu đăng nhập đúng → lưu session
                    Session["TenTK"] = tk.TenTK;
                    Session["ID_TK"] = tk.ID_TK;

                    // Chuyển đến trang Home 
                    return RedirectToAction("TrangChuKhachHang", "TrangChuKhachHang");
                }

                catch (Exception ex)
                {
                    ViewBag.ThongBao = "Lỗi đăng nhập: " + ex.Message;
                    return View();
                }
            }
        }

    }
}
