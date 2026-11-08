using QuanLyRapPhim.Models;
using QuanLyRapPhim.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyRapPhim.Business
{
    public class DatVeService
    {
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
            var gheList = (gheChon ?? "").Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            var suat = db.SuatChieus.FirstOrDefault(s => s.ID_SuatChieu == id);
            int idPhong = (suat != null && suat.ID_Phong.HasValue) ? suat.ID_Phong.Value : 0;

            foreach (var ghe in gheList)
            {
                var idGhe = db.Ghes
                    .Where(g => g.So_Ghe == ghe && g.ID_Phong == idPhong)
                    .Select(g => g.ID_Ghe)
                    .FirstOrDefault();

                if (!db.Ves.Any(v => v.ID_SuatChieu == id && v.ID_Ghe == idGhe))
                {
                    db.Ves.InsertOnSubmit(new Ve
                    {
                        ID_SuatChieu = id,
                        ID_Ghe = idGhe,
                        So_Ghe = gheChon,
                        TenDoAn = tenDoAn,
                        SoLuongDoAn = soLuong,
                        TongTien = tongTien,
                        ID_TK = idTK,
                        TrangThai = 2
                    });
                }
            }

            if (doAnList != null)
            {
                foreach (var item in doAnList)
                {
                    var doAn = db.QuanLyDoAns.FirstOrDefault(d => d.ID == item.id);
                    if (doAn != null)
                    {
                        if (doAn.SoLuong < item.soLuong)
                            return (false, "Món ăn không đủ số lượng!");

                        doAn.SoLuong -= item.soLuong;
                        if (doAn.SoLuong == 0)
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

            db.SubmitChanges();
            return (true, "Thanh toán thành công!");
        }
    }
}
