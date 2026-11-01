using QuanLyRapPhim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyRapPhim.Business
{
    public class SuatChieuService
    {
        public bool KiemTraTrungGioChieu(DataClasses1DataContext db, int idPhim, int idPhong, DateTime gioChieu)
        {
            var thoiLuong = db.QLDanh_Sach_Phims
                              .Where(p => p.ID_Phim == idPhim)
                              .Select(p => p.ThoiLuong ?? 120)
                              .FirstOrDefault();

            // Lấy danh sách suất chiếu của phòng về bộ nhớ để tính toán AddMinutes chính xác
            var dsSuatChieu = db.SuatChieus
                .Where(s => s.ID_Phong == idPhong && s.GioChieu.HasValue)
                .ToList();

            return dsSuatChieu.Any(s =>
                s.GioChieu.Value.Date == gioChieu.Date &&
                gioChieu < s.GioChieu.Value.AddMinutes(thoiLuong) &&
                gioChieu.AddMinutes(thoiLuong) > s.GioChieu.Value);
        }

        public (bool Success, string Message) ThemSuatChieu(DataClasses1DataContext db, int idPhim, int idPhong, DateTime gioChieu, decimal gia)
        {
            if (KiemTraTrungGioChieu(db, idPhim, idPhong, gioChieu))
            {
                return (false, "Suất chiếu bị trùng giờ trong cùng phòng chiếu!");
            }

            SuatChieu sc = new SuatChieu
            {
                ID_Phim = idPhim,
                ID_Phong = idPhong,
                GioChieu = gioChieu,
                Gia = gia
            };

            db.SuatChieus.InsertOnSubmit(sc);
            db.SubmitChanges();
            return (true, "Thêm suất chiếu thành công!");
        }

        public (bool Success, string Message) XoaSuatChieu(DataClasses1DataContext db, int id)
        {
            var sc = db.SuatChieus.FirstOrDefault(s => s.ID_SuatChieu == id);
            if (sc == null)
                return (false, "Không tìm thấy suất chiếu ");

            bool daCoVe = db.Ves.Any(v => v.ID_SuatChieu == id);
            if (daCoVe)
                return (false, "Không thể xóa vì đã có khách đặt vé");

            db.SuatChieus.DeleteOnSubmit(sc);
            db.SubmitChanges();
            return (true, "Xóa suất chiếu thành công!");
        }
    }
}
