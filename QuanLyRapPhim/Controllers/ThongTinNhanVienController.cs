using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using QuanLyRapPhim.Models;

namespace QuanLyRapPhim.Controllers
{
    public class ThongTinNhanVienController : Controller
    {
        // 1. Hàm lấy chuỗi kết nối chuẩn từ Web.config
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString;
        }

        // 2. Action hiển thị thông tin cá nhân nhân viên (GET)
        // Đường dẫn: /ThongTinNhanVien/ThongTinNhanVien
        [HttpGet]
        public ActionResult ThongTinNhanVien()
        {
            // Kiểm tra xem đã đăng nhập chưa
            if (Session["ID_NV"] == null)
            {
                // Chưa đăng nhập thì chuyển hướng về trang đăng nhập
                return RedirectToAction("DangNhapNhanVien", "DangNhapNhanVien");
            }

            string idNV = Session["ID_NV"].ToString();

            using (var db = new DataClasses1DataContext(GetConnectionString()))
            {
                var nv = db.NhanViens.SingleOrDefault(n => n.ID_NV == idNV);
                if (nv == null)
                {
                    return RedirectToAction("DangNhapNhanVien", "DangNhapNhanVien");
                }

                // Trả về View kèm dữ liệu nhân viên
                return View(nv);
            }
        }
    }
}
