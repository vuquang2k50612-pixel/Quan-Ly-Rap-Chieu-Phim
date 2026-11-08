using QuanLyRapPhim.Business;
using QuanLyRapPhim.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyRapPhim.Controllers
{
    public class QuanLyDoAnController : Controller
    {
        private readonly DoAnService _doAnService = new DoAnService();

        // GET: QuanLyDoAn
        public ActionResult ThemDoAn()
        {
            return View();
        }

        public ActionResult ChinhSuaDoAn()
        {
            return View();
        }

        public ActionResult DSDoAn()
        {
            return View();
        }

        public string InsertDoAn()
        {
            try
            {
                string ten = Request["txt_TenDoAn"];
                string gia_str = Request["txt_Gia"];
                string mota = Request["txt_MoTa"];
                string soluong_str = Request["txt_SoLuong"];

                if (ten == "" || gia_str == "" || soluong_str == "")
                {
                    return "Vui lòng điền đầy đủ thông tin!";
                }

                int soluong;
                if (!int.TryParse(soluong_str, out soluong))
                {
                    return "Số lượng không hợp lệ";
                }

                decimal gia;
                if (!decimal.TryParse(gia_str, out gia))
                {
                    return "Giá không hợp lệ";
                }

                if (Request.Files.Count > 0)
                {
                    var file = Request.Files["txt_Anh"];
                    if (file != null)
                    {
                        string fileName = ten + ".jpg";
                        string path = Path.Combine(Server.MapPath("~/img"), fileName);
                        file.SaveAs(path);
                    }
                }

                DataClasses1DataContext db = new DataClasses1DataContext();
                var result = _doAnService.ThemDoAn(db, ten, gia, mota, soluong);
                return result.Message;
            }
            catch (Exception ex)
            {
                return "Lỗi: " + ex.Message;
            }
        }

        public string LayTTDoAn(int id)
        {
            DataClasses1DataContext db = new DataClasses1DataContext();
            var doan_qr = db.QuanLyDoAns.Where(o => o.ID == id);
            if (doan_qr.Any())
            {
                string rs = Newtonsoft.Json.JsonConvert.SerializeObject(doan_qr.SingleOrDefault());
                return rs;
            }
            else
            {
                return "Không tìm thấy đồ ăn!";
            }
        }

        public string UpdateDoAn(int id)
        {
            string ten = Request["txt_TenDoAn"];
            string gia_str = Request["txt_Gia"];
            string mota = Request["txt_MoTa"];
            string soluong_str = Request["txt_SoLuong"];
            if (ten == "" || gia_str == "" || soluong_str == "")
            {
                return "Vui lòng điền đầy đủ thông tin!";
            }
            int soluong;
            if (!int.TryParse(soluong_str, out soluong))
            {
                return "Số lượng không hợp lệ";
            }
            decimal gia;
            if (!decimal.TryParse(gia_str, out gia))
            {
                return "Giá không hợp lệ";
            }

            DataClasses1DataContext db = new DataClasses1DataContext();
            var result = _doAnService.CapNhatDoAn(db, id, ten, gia, mota, soluong);
            return result.Message;
        }

        public string Danhsachdoan()
        {
            DataClasses1DataContext db = new DataClasses1DataContext();
            var doan_qr = db.QuanLyDoAns.ToList();
            if (doan_qr.Any())
            {
                string rs = Newtonsoft.Json.JsonConvert.SerializeObject(doan_qr);
                return rs;
            }
            else
            {
                return "Không có đồ ăn nào!";
            }
        }

        public string XoaDoAn(int id)
        {
            DataClasses1DataContext db = new DataClasses1DataContext();
            var result = _doAnService.XoaDoAn(db, id);
            return result.Message;
        }
    }
}
