using System;

namespace QuanLyRapPhim.ViewModels
{
    public class GheViewModel
    {
        public int ID_Ghe { get; set; }
        public string So_Ghe { get; set; }
        public string LoaiGhe { get; set; } = "Thường";
        public int TrangThai { get; set; } // 0 = trống, 1 = đang giữ, 2 = đã bán

        public string CssClass
        {
            get
            {
                switch (TrangThai)
                {
                    case 0: return "available";
                    case 1: return "holding";
                    case 2: return "sold";
                    default: return "unknown";
                }
            }
        }

        public bool IsAvailable => TrangThai == 0;
    }
}
