using QuanLyRapPhim.Models;
using System.Linq;

namespace QuanLyRapPhim.Business
{
    public class NhanVienService
    {
        public (bool Success, string Message) ThemNhanVien(DataClasses1DataContext db, NhanVien nv)
        {
            if (db.NhanViens.Any(o => o.ID_NV == nv.ID_NV))
                return (false, "Mã nhân viên đã tồn tại!");

            if (db.NhanViens.Any(o => o.SDT == nv.SDT))
                return (false, "Số điện thoại đã tồn tại!");

            db.NhanViens.InsertOnSubmit(nv);
            db.SubmitChanges();
            return (true, "Thêm nhân viên thành công");
        }

        public (bool Success, string Message) CapNhatNhanVien(DataClasses1DataContext db, NhanVien nvMoi)
        {
            var nv = db.NhanViens.FirstOrDefault(o => o.ID_NV == nvMoi.ID_NV);
            if (nv == null)
                return (false, "Không tìm thấy nhân viên!");

            if (db.NhanViens.Any(o => o.SDT == nvMoi.SDT && o.ID_NV != nvMoi.ID_NV))
                return (false, "Số điện thoại đã tồn tại!");

            nv.TenNV = nvMoi.TenNV;
            nv.NgaySinh = nvMoi.NgaySinh;
            nv.PasswordHash = nvMoi.PasswordHash;
            nv.ThoiGianTuyen = nvMoi.ThoiGianTuyen;
            nv.GioiTinh = nvMoi.GioiTinh;
            nv.Email = nvMoi.Email;
            nv.VaiTro = nvMoi.VaiTro;
            nv.SDT = nvMoi.SDT;
            nv.Luong = nvMoi.Luong;

            db.SubmitChanges();
            return (true, "Cập nhật thông tin nhân viên thành công");
        }

        public (bool Success, string Message) XoaNhanVien(DataClasses1DataContext db, string id)
        {
            var nv = db.NhanViens.FirstOrDefault(o => o.ID_NV == id);
            if (nv == null)
                return (false, "Không tìm thấy nhân viên!");

            db.NhanViens.DeleteOnSubmit(nv);
            db.SubmitChanges();
            return (true, "Xóa nhân viên thành công");
        }
    }
}
