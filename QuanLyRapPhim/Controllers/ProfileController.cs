using QuanLyRapPhim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers 
{
    public class ProfileController : Controller
    {
        // 1. Trang hiển thị Lịch sử đặt vé
        public ActionResult LichSuDatVe()
        {
            if (Session["ID_TK"] == null)
            {
                return RedirectToAction("DNKH", "DNKH");
            }

            int idTK = Convert.ToInt32(Session["ID_TK"]);
            using (var db = new DataClasses1DataContext())
            {
                var user = db.TaiKhoans.FirstOrDefault(x => x.ID_TK == idTK);
                return View(user);
            }
        }

        // 2. API lấy danh sách vé đã đặt của khách hàng
        public JsonResult LayLichSuDatVe()
        {
            try
            {
                if (Session["ID_TK"] == null)
                    return Json(new { success = false, message = "Hết phiên đăng nhập!" }, JsonRequestBehavior.AllowGet);

                int idTK = Convert.ToInt32(Session["ID_TK"]);

                using (var db = new DataClasses1DataContext()) 
                {
                    // Lấy dữ liệu từ DB
                    var rawData = (from v in db.Ves
                                   join sc in db.SuatChieus on v.ID_SuatChieu equals sc.ID_SuatChieu
                                   join p in db.QLDanh_Sach_Phims on sc.ID_Phim equals p.ID_Phim
                                   join pc in db.Phong_Chieus on sc.ID_Phong equals pc.ID_Phong
                                   join g in db.Ghes on v.ID_Ghe equals g.ID_Ghe
                                   where v.ID_TK == idTK
                                   orderby v.ID_Ve descending
                                   select new
                                   {
                                       v.ID_Ve,
                                       p.TenPhim,
                                       sc.GioChieu,
                                       pc.TenPhong,
                                       g.So_Ghe,
                                       v.TenDoAn,
                                       v.TongTien,
                                       v.TrangThai
                                   }).ToList();

                    // Format ngày tháng và trạng thái
                    var data = rawData.Select(x => new
                    {
                        MaVe = x.ID_Ve,
                        TenPhim = x.TenPhim,
                        NgayChieu = string.Format("{0:HH:mm dd/MM/yyyy}", x.GioChieu),
                        TenPhong = x.TenPhong,
                        Ghe = x.So_Ghe,
                        DoAn = x.TenDoAn,
                        TongTien = x.TongTien,
                        TrangThai = x.TrangThai == 2 ? "Đã thanh toán" : "Chờ thanh toán"
                    });

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}