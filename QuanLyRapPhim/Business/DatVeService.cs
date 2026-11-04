using QuanLyRapPhim.Models;
using QuanLyRapPhim.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyRapPhim.Business
{
    public class SuatChieuGheInfo
    {
        public int ID_SuatChieu { get; set; }
        public string TenPhim { get; set; }
        public string TheLoai { get; set; }
        public int? ThoiLuong { get; set; }
        public string TenPhong { get; set; }
        public DateTime? GioChieu { get; set; }
        public decimal? Gia { get; set; }
    }

    public class DatVeService
    {
        public SuatChieuGheInfo LayThongTinSuatChieu(DataClasses1DataContext db, int idSuatChieu)
        {
            return db.SuatChieus
                .Where(s => s.ID_SuatChieu == idSuatChieu)
                .Select(s => new SuatChieuGheInfo
                {
                    ID_SuatChieu = s.ID_SuatChieu,
                    TenPhim = s.QLDanh_Sach_Phim != null ? s.QLDanh_Sach_Phim.TenPhim : "",
                    TheLoai = s.QLDanh_Sach_Phim != null ? s.QLDanh_Sach_Phim.TheLoai : "",
                    ThoiLuong = s.QLDanh_Sach_Phim != null ? s.QLDanh_Sach_Phim.ThoiLuong : 120,
                    TenPhong = s.Phong_Chieu != null ? s.Phong_Chieu.TenPhong : "",
                    GioChieu = s.GioChieu,
                    Gia = s.Gia
                })
                .FirstOrDefault();
        }

        public List<GheViewModel> LayDanhSachGhe(DataClasses1DataContext db, int idSuatChieu)
        {
            var suat = db.SuatChieus.FirstOrDefault(s => s.ID_SuatChieu == idSuatChieu);
            if (suat == null || !suat.ID_Phong.HasValue) return new List<GheViewModel>();

            var gheDaBan = db.Ves
                .Where(v => v.ID_SuatChieu == idSuatChieu && v.TrangThai == 2 && v.ID_Ghe.HasValue)
                .Select(v => v.ID_Ghe.Value)
                .Distinct()
                .ToList();

            return db.Ghes
                .Where(g => g.ID_Phong == suat.ID_Phong)
                .Select(g => new GheViewModel
                {
                    ID_Ghe = g.ID_Ghe,
                    So_Ghe = g.So_Ghe,
                    TrangThai = gheDaBan.Contains(g.ID_Ghe) ? 2 : 0
                })
                .OrderBy(g => g.So_Ghe)
                .ToList();
        }

        public (bool Success, string Message) DatVe(DataClasses1DataContext db, int idSuatChieu, int[] gheIds)
        {
            if (gheIds == null || gheIds.Length == 0)
                return (false, "Vui lòng chọn ghế!");

            try
            {
                foreach (var idGhe in gheIds)
                {
                    if (!db.Ves.Any(v => v.ID_SuatChieu == idSuatChieu && v.ID_Ghe == idGhe && v.TrangThai == 2))
                    {
                        var gheObj = db.Ghes.FirstOrDefault(g => g.ID_Ghe == idGhe);
                        db.Ves.InsertOnSubmit(new Ve
                        {
                            ID_SuatChieu = idSuatChieu,
                            ID_Ghe = idGhe,
                            So_Ghe = gheObj != null ? gheObj.So_Ghe : "",
                            TrangThai = 2 // Đã bán
                        });
                    }
                }
                db.SubmitChanges();
                return (true, $"Đặt thành công {gheIds.Length} ghế!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi giữ chỗ: " + ex.Message);
            }
        }

        public (bool Success, string Message) XacNhanThanhToan(
            DataClasses1DataContext db,
            int id,
            string gheChon,
            string tenDoAn,
            int soLuong,
            List<DoAnItem> doAnList,
            string tongTien,
            int idTK)
        {
            try
            {
                var gheList = (gheChon ?? "")
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                if (gheList.Count == 0)
                {
                    return (false, "Danh sách ghế thanh toán không hợp lệ!");
                }

                var suat = db.SuatChieus.FirstOrDefault(s => s.ID_SuatChieu == id);
                if (suat == null)
                {
                    return (false, "Suất chiếu không tồn tại trong hệ thống!");
                }

                int idPhong = suat.ID_Phong ?? 0;

                // 1. Kiểm tra xem có ghế nào đã có người mua trước chưa
                foreach (var ghe in gheList)
                {
                    var gheEntity = db.Ghes.FirstOrDefault(g => g.So_Ghe == ghe && g.ID_Phong == idPhong);
                    if (gheEntity != null)
                    {
                        bool daBan = db.Ves.Any(v => v.ID_SuatChieu == id && v.ID_Ghe == gheEntity.ID_Ghe && v.TrangThai == 2);
                        if (daBan)
                        {
                            return (false, $"Ghế {ghe} đã có người thanh toán trước đó. Vui lòng chọn ghế khác!");
                        }
                    }
                }

                // 2. Kiểm tra tồn kho đồ ăn trước khi trừ
                if (doAnList != null && doAnList.Count > 0)
                {
                    foreach (var item in doAnList)
                    {
                        var doAn = db.QuanLyDoAns.FirstOrDefault(d => d.ID == item.id);
                        if (doAn == null)
                        {
                            return (false, $"Món ăn mã #{item.id} không tồn tại!");
                        }

                        if (doAn.SoLuong < item.soLuong)
                        {
                            return (false, $"Món ăn '{doAn.TenDoAn}' không đủ số lượng trong kho (chỉ còn {doAn.SoLuong})!");
                        }
                    }

                    // Trừ tồn kho và cập nhật trạng thái
                    foreach (var item in doAnList)
                    {
                        var doAn = db.QuanLyDoAns.FirstOrDefault(d => d.ID == item.id);
                        if (doAn != null)
                        {
                            doAn.SoLuong -= item.soLuong;
                            if (doAn.SoLuong <= 0)
                            {
                                doAn.SoLuong = 0;
                                doAn.TrangThai = "Hết hàng";
                            }
                            else
                            {
                                doAn.TrangThai = "Còn hàng";
                            }
                        }
                    }
                }

                // 3. Tạo các bản ghi vé tương ứng từng ghế
                foreach (var ghe in gheList)
                {
                    var idGhe = db.Ghes
                        .Where(g => g.So_Ghe == ghe && g.ID_Phong == idPhong)
                        .Select(g => (int?)g.ID_Ghe)
                        .FirstOrDefault();

                    var veMoi = new Ve
                    {
                        ID_SuatChieu = id,
                        ID_Ghe = idGhe,
                        So_Ghe = ghe,
                        TenDoAn = tenDoAn,
                        SoLuongDoAn = soLuong,
                        TongTien = tongTien,
                        ID_TK = idTK > 0 ? (int?)idTK : null,
                        TrangThai = 2 // Đã thanh toán
                    };

                    db.Ves.InsertOnSubmit(veMoi);
                }

                db.SubmitChanges();
                return (true, "Thanh toán thành công!");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi trong quá trình thanh toán: " + ex.Message);
            }
        }
    }
}
