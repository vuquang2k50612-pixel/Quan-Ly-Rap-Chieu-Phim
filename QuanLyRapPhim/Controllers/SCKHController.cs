using QuanLyRapPhim.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers
{
    public class SCKHController : Controller
    {
        public ActionResult SCKH()
        {
            return View();
        }

        // Lấy suất chiếu theo phim
        [HttpPost]
        public JsonResult LaySuatCuaPhim(int idPhim)
        {
            try
            {
                DataClasses1DataContext db = new DataClasses1DataContext(System.Configuration.ConfigurationManager.ConnectionStrings["PhimConnectionString"].ConnectionString);
                {
                    var suatchieu_qr = db.SuatChieus
                        .Where(s => s.ID_Phim == idPhim)
                        .OrderBy(s => s.GioChieu)
                        .Select(s => new
                        {
                            s.ID_SuatChieu,
                            GioChieu = s.GioChieu.Value,
                            s.Gia,
                            s.ID_Phong
                        })
                        .ToList();

                    return Json(new { success = true, data = suatchieu_qr }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}