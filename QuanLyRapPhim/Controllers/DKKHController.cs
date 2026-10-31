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
    public class DKKHController : Controller
    {
        // Hàm lấy chuỗi kết nối chuẩn từ Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        public ActionResult DKKH()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DKKH(string tentk, string matkhau)
        {
            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                try
                {

                    // Kiểm tra tên tài khoản đã tồn tại chưa
                    if (db.TaiKhoans.Any(x => x.TenTK == tentk))
                    {
                        ViewBag.ThongBao = "Tên tài khoản đã tồn tại!";
                        return View();
                    }

                    // Kiểm tra mật khẩu có ít nhất 1 chữ in hoa và 1 chữ số
                    var regexMatKhau = new Regex(@"^(?=.*[A-Z])(?=.*\d).+$");
                    if (!regexMatKhau.IsMatch(matkhau))
                    {
                        ViewBag.ThongBao = "Mật khẩu phải chứa ít nhất 1 chữ in hoa và 1 chữ số!";
                        return View();
                    }

                    // Tạo tài khoản mới
                    var tk = new TaiKhoan
                    {
                        TenTK = tentk,
                        MatKhau = matkhau,
                    };

                    db.TaiKhoans.InsertOnSubmit(tk);
                    db.SubmitChanges();

                    ViewBag.ThongBao = "Đăng ký thành công!";
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi kết nối hoặc truy vấn
                    ViewBag.ThongBao = "Lỗi: " + ex.Message;
                }
            }

            return View();
        }
    }
}
