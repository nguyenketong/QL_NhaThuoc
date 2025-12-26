using Microsoft.EntityFrameworkCore;
using QL_NhaThuoc.Models;

namespace QL_NhaThuoc.Data
{
    public class QL_NhaThuocDbContext : DbContext
    {
        public QL_NhaThuocDbContext(DbContextOptions<QL_NhaThuocDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<NhomThuoc> NHOM_THUOC { get; set; }
        public DbSet<ThuongHieu> THUONG_HIEU { get; set; }
        public DbSet<NuocSanXuat> NUOC_SAN_XUAT { get; set; }
        public DbSet<DoiTuongSuDung> DOI_TUONG_SU_DUNG { get; set; }
        public DbSet<ThanhPhan> THANH_PHAN { get; set; }
        public DbSet<TacDungPhu> TAC_DUNG_PHU { get; set; }
        public DbSet<Thuoc> THUOC { get; set; }
        public DbSet<CT_ThanhPhan> CT_THANH_PHAN { get; set; }
        public DbSet<CT_DoiTuong> CT_DOI_TUONG { get; set; }
        public DbSet<CT_TacDungPhu> CT_TAC_DUNG_PHU { get; set; }
        public DbSet<NguoiDung> NGUOI_DUNG { get; set; }
        public DbSet<DonHang> DON_HANG { get; set; }
        public DbSet<ChiTietDonHang> CHI_TIET_DON_HANG { get; set; }
        public DbSet<BaiViet> BAI_VIET { get; set; }
        public DbSet<ThongBao> THONG_BAO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<NhomThuoc>().ToTable("NHOM_THUOC");
            modelBuilder.Entity<ThuongHieu>().ToTable("THUONG_HIEU");
            modelBuilder.Entity<NuocSanXuat>().ToTable("NUOC_SAN_XUAT");
            modelBuilder.Entity<DoiTuongSuDung>().ToTable("DOI_TUONG_SU_DUNG");
            modelBuilder.Entity<ThanhPhan>().ToTable("THANH_PHAN");
            modelBuilder.Entity<TacDungPhu>().ToTable("TAC_DUNG_PHU");
            modelBuilder.Entity<Thuoc>().ToTable("THUOC");
            modelBuilder.Entity<CT_ThanhPhan>().ToTable("CT_THANH_PHAN");
            modelBuilder.Entity<CT_DoiTuong>().ToTable("CT_DOI_TUONG");
            modelBuilder.Entity<CT_TacDungPhu>().ToTable("CT_TAC_DUNG_PHU");
            modelBuilder.Entity<NguoiDung>().ToTable("NGUOI_DUNG");
            modelBuilder.Entity<DonHang>().ToTable("DON_HANG");
            modelBuilder.Entity<ChiTietDonHang>().ToTable("CHI_TIET_DON_HANG");
            modelBuilder.Entity<BaiViet>().ToTable("BAIVIET");
            modelBuilder.Entity<ThongBao>().ToTable("THONG_BAO");

            // Configure primary keys
            modelBuilder.Entity<NhomThuoc>().HasKey(e => e.MaNhomThuoc);
            modelBuilder.Entity<ThuongHieu>().HasKey(e => e.MaThuongHieu);
            modelBuilder.Entity<NuocSanXuat>().HasKey(e => e.MaNuocSX);
            modelBuilder.Entity<DoiTuongSuDung>().HasKey(e => e.MaDoiTuong);
            modelBuilder.Entity<ThanhPhan>().HasKey(e => e.MaThanhPhan);
            modelBuilder.Entity<TacDungPhu>().HasKey(e => e.MaTacDungPhu);
            modelBuilder.Entity<Thuoc>().HasKey(e => e.MaThuoc);
            modelBuilder.Entity<NguoiDung>().HasKey(e => e.MaNguoiDung);
            modelBuilder.Entity<DonHang>().HasKey(e => e.MaDonHang);
            modelBuilder.Entity<ChiTietDonHang>().HasKey(e => e.MaChiTiet);
            modelBuilder.Entity<BaiViet>().HasKey(e => e.MaBaiViet);
            modelBuilder.Entity<ThongBao>().HasKey(e => e.MaThongBao);

            // Configure composite keys for junction tables
            modelBuilder.Entity<CT_ThanhPhan>()
                .HasKey(e => new { e.MaThuoc, e.MaThanhPhan });

            modelBuilder.Entity<CT_DoiTuong>()
                .HasKey(e => new { e.MaThuoc, e.MaDoiTuong });

            modelBuilder.Entity<CT_TacDungPhu>()
                .HasKey(e => new { e.MaThuoc, e.MaTacDungPhu });

            // Configure self-referencing relationship for NhomThuoc (danh mục cha-con)
            modelBuilder.Entity<NhomThuoc>()
                .HasOne(n => n.DanhMucCha)
                .WithMany(n => n.DanhMucCon)
                .HasForeignKey(n => n.MaDanhMucCha)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships - THUOC
            modelBuilder.Entity<Thuoc>()
                .HasOne(t => t.NhomThuoc)
                .WithMany(n => n.Thuocs)
                .HasForeignKey(t => t.MaNhomThuoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Thuoc>()
                .HasOne(t => t.NuocSanXuat)
                .WithMany(n => n.Thuocs)
                .HasForeignKey(t => t.MaNuocSX)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Thuoc>()
                .HasOne(t => t.ThuongHieu)
                .WithMany(th => th.Thuocs)
                .HasForeignKey(t => t.MaThuongHieu)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships - CT_THANH_PHAN
            modelBuilder.Entity<CT_ThanhPhan>()
                .HasOne(ct => ct.Thuoc)
                .WithMany(t => t.CT_ThanhPhans)
                .HasForeignKey(ct => ct.MaThuoc)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CT_ThanhPhan>()
                .HasOne(ct => ct.ThanhPhan)
                .WithMany(tp => tp.CT_ThanhPhans)
                .HasForeignKey(ct => ct.MaThanhPhan)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships - CT_DOI_TUONG
            modelBuilder.Entity<CT_DoiTuong>()
                .HasOne(ct => ct.Thuoc)
                .WithMany(t => t.CT_DoiTuongs)
                .HasForeignKey(ct => ct.MaThuoc)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CT_DoiTuong>()
                .HasOne(ct => ct.DoiTuongSuDung)
                .WithMany(dt => dt.CT_DoiTuongs)
                .HasForeignKey(ct => ct.MaDoiTuong)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships - CT_TAC_DUNG_PHU
            modelBuilder.Entity<CT_TacDungPhu>()
                .HasOne(ct => ct.Thuoc)
                .WithMany(t => t.CT_TacDungPhus)
                .HasForeignKey(ct => ct.MaThuoc)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CT_TacDungPhu>()
                .HasOne(ct => ct.TacDungPhu)
                .WithMany(tdp => tdp.CT_TacDungPhus)
                .HasForeignKey(ct => ct.MaTacDungPhu)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships - DON_HANG
            modelBuilder.Entity<DonHang>()
                .HasOne(dh => dh.NguoiDung)
                .WithMany(nd => nd.DonHangs)
                .HasForeignKey(dh => dh.MaNguoiDung)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships - CHI_TIET_DON_HANG
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaDonHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.Thuoc)
                .WithMany(t => t.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaThuoc)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure unique constraint for NGUOI_DUNG.SoDienThoai
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(nd => nd.SoDienThoai)
                .IsUnique();

            // Configure relationships - THONG_BAO
            modelBuilder.Entity<ThongBao>()
                .HasOne(tb => tb.NguoiDung)
                .WithMany()
                .HasForeignKey(tb => tb.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ThongBao>()
                .HasOne(tb => tb.DonHang)
                .WithMany()
                .HasForeignKey(tb => tb.MaDonHang)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure default values
            modelBuilder.Entity<NguoiDung>()
                .Property(nd => nd.NgayTao)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<DonHang>()
                .Property(dh => dh.NgayDatHang)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
