using QuanLyRapPhim.Models;
using QuanLyRapPhim.Models;
using System.Linq;

namespace QuanLyRapPhim.Business
{
    public class DoAnService
    {
        public (bool Success, string Message) ThemDoAn(DataClasses1DataContext db, string ten, decimal gia, string mota, int soluong)
        {
            var exist = db.QuanLyDoAns.Any(o => o.TenDoAn == ten);
            if (exist)
                return (false, "Đồ ăn đã tồn tại!");

            QuanLyDoAn doAn = new QuanLyDoAn
            {
                TenDoAn = ten,
                Gia = gia,
                MoTa = mota,
                SoLuong = soluong,
                TrangThai = soluong > 0 ? "Còn hàng" : "Hết hàng"
            };

            db.QuanLyDoAns.InsertOnSubmit(doAn);
            db.SubmitChanges();
            return (true, "Thêm đồ ăn thành công");
        }

        public (bool Success, string Message) CapNhatDoAn(DataClasses1DataContext db, int id, string ten, decimal gia, string mota, int soluong)
        {
            var doAn = db.QuanLyDoAns.FirstOrDefault(o => o.ID == id);
            if (doAn == null)
                return (false, "Không tìm thấy đồ ăn!");

            doAn.TenDoAn = ten;
            doAn.Gia = gia;
            doAn.MoTa = mota;
            doAn.SoLuong = soluong;
            doAn.TrangThai = soluong > 0 ? "Còn hàng" : "Hết hàng";

            db.SubmitChanges();
            return (true, "Cập nhật đồ ăn thành công");
        }

        public (bool Success, string Message) XoaDoAn(DataClasses1DataContext db, int id)
        {
            var doAn = db.QuanLyDoAns.FirstOrDefault(o => o.ID == id);
            if (doAn == null)
                return (false, "Không tìm thấy đồ ăn!");

            db.QuanLyDoAns.DeleteOnSubmit(doAn);
            db.SubmitChanges();
            return (true, "Xóa đồ ăn thành công");
        }
    }
}
