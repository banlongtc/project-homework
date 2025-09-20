using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MPLUS_GW_WebCore.Models;

public partial class ExportDataQadContext : DbContext
{
    public ExportDataQadContext()
    {
    }

    public ExportDataQadContext(DbContextOptions<ExportDataQadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EvsWo> EvsWos { get; set; }

    public virtual DbSet<ItemMaster> ItemMasters { get; set; }

    public virtual DbSet<TransEv> TransEvs { get; set; }

    public virtual DbSet<TransExport> TransExports { get; set; }

    public virtual DbSet<TransGw> TransGws { get; set; }

    public virtual DbSet<WoBillBrowse> WoBillBrowses { get; set; }

    public virtual DbSet<WoBrowse> WoBrowses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=10.239.1.54;Initial Catalog=Export_data_QAD;User ID=sa;Password=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EvsWo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("EVS_WO");

            entity.Property(e => e.Description1).HasMaxLength(255);
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemNumber)
                .IsUnicode(false)
                .HasColumnName("item_number");
            entity.Property(e => e.Lot)
                .IsUnicode(false)
                .HasColumnName("lot");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderQty).HasColumnName("order_qty");
            entity.Property(e => e.PartNumber)
                .IsUnicode(false)
                .HasColumnName("part_number");
            entity.Property(e => e.ProdLine)
                .HasMaxLength(255)
                .HasColumnName("Prod_Line");
            entity.Property(e => e.QtyComp).HasColumnName("qty_comp");
            entity.Property(e => e.QtyIssued).HasColumnName("qty_issued");
            entity.Property(e => e.QtyReq).HasColumnName("qty_req");
            entity.Property(e => e.QtyToIssue).HasColumnName("qty_to_issue");
            entity.Property(e => e.WoStatus)
                .IsUnicode(false)
                .HasColumnName("wo_status");
            entity.Property(e => e.WorkOrder).HasColumnName("work_order");
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("item_master");

            entity.Property(e => e.AbcClass)
                .HasMaxLength(255)
                .HasColumnName("ABC_Class");
            entity.Property(e => e.Active).HasMaxLength(255);
            entity.Property(e => e.Active1).HasMaxLength(255);
            entity.Property(e => e.Added).HasColumnType("datetime");
            entity.Property(e => e.AllocateSingleLot)
                .HasMaxLength(255)
                .HasColumnName("Allocate_Single_Lot");
            entity.Property(e => e.AllocateWholeLot)
                .HasMaxLength(255)
                .HasColumnName("Allocate_whole_lot");
            entity.Property(e => e.ArticleNumber)
                .HasMaxLength(255)
                .HasColumnName("Article_Number");
            entity.Property(e => e.AutoLotNum)
                .HasMaxLength(255)
                .HasColumnName("Auto_Lot_Num");
            entity.Property(e => e.AutoQo)
                .HasMaxLength(255)
                .HasColumnName("Auto QO");
            entity.Property(e => e.AvgInterval).HasColumnName("Avg_Interval");
            entity.Property(e => e.Comment).HasMaxLength(255);
            entity.Property(e => e.CommodityCode)
                .HasMaxLength(255)
                .HasColumnName("Commodity_Code");
            entity.Property(e => e.CycleCountInterval).HasColumnName("Cycle_Count_Interval");
            entity.Property(e => e.Description1).HasMaxLength(255);
            entity.Property(e => e.Description2).HasMaxLength(255);
            entity.Property(e => e.DrawLoc)
                .HasMaxLength(255)
                .HasColumnName("Draw_Loc");
            entity.Property(e => e.DrawSize).HasColumnName("Draw_Size");
            entity.Property(e => e.Drawing).HasMaxLength(255);
            entity.Property(e => e.DsgnGrp)
                .HasMaxLength(255)
                .HasColumnName("Dsgn_Grp");
            entity.Property(e => e.FreightClass)
                .HasMaxLength(255)
                .HasColumnName("Freight_Class");
            entity.Property(e => e.Group).HasMaxLength(255);
            entity.Property(e => e.ItemNumber)
                .HasMaxLength(255)
                .HasColumnName("Item_Number");
            entity.Property(e => e.ItemRevision)
                .HasMaxLength(255)
                .HasColumnName("Item Revision");
            entity.Property(e => e.ItemType)
                .HasMaxLength(255)
                .HasColumnName("Item_Type");
            entity.Property(e => e.KeyItem)
                .HasMaxLength(255)
                .HasColumnName("Key_Item");
            entity.Property(e => e.Loc).HasMaxLength(255);
            entity.Property(e => e.LocType)
                .HasMaxLength(255)
                .HasColumnName("Loc_Type");
            entity.Property(e => e.LocatorGroup)
                .HasMaxLength(255)
                .HasColumnName("Locator_Group");
            entity.Property(e => e.LotControl)
                .HasMaxLength(255)
                .HasColumnName("Lot_Control");
            entity.Property(e => e.LotGroup)
                .HasMaxLength(255)
                .HasColumnName("Lot_Group");
            entity.Property(e => e.NetWeight).HasColumnName("Net_Weight");
            entity.Property(e => e.PoRptStat)
                .HasColumnType("datetime")
                .HasColumnName("PO_Rpt_Stat");
            entity.Property(e => e.PriceBreakCat)
                .HasMaxLength(255)
                .HasColumnName("Price_Break_Cat");
            entity.Property(e => e.ProdLine)
                .HasMaxLength(255)
                .HasColumnName("Prod_Line");
            entity.Property(e => e.PromoGrp)
                .HasMaxLength(255)
                .HasColumnName("Promo_Grp");
            entity.Property(e => e.QtyCarton).HasColumnName("Qty_Carton");
            entity.Property(e => e.QuantityPallet).HasColumnName("Quantity_Pallet");
            entity.Property(e => e.ShelfLife).HasColumnName("Shelf_Life");
            entity.Property(e => e.ShipWeight).HasColumnName("Ship_Weight");
            entity.Property(e => e.Site).HasMaxLength(255);
            entity.Property(e => e.SizeUm)
                .HasMaxLength(255)
                .HasColumnName("Size_UM");
            entity.Property(e => e.Status).HasMaxLength(255);
            entity.Property(e => e.Um)
                .HasMaxLength(255)
                .HasColumnName("UM");
            entity.Property(e => e.Um1)
                .HasMaxLength(255)
                .HasColumnName("UM1");
            entity.Property(e => e.Um2)
                .HasMaxLength(255)
                .HasColumnName("UM2");
            entity.Property(e => e.WoRptStat)
                .HasMaxLength(255)
                .HasColumnName("WO_Rpt_Stat");
        });

        modelBuilder.Entity<TransEv>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("trans_EVS");

            entity.Property(e => e.ChangeQty).HasColumnName("change_qty");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.Description1).HasMaxLength(255);
            entity.Property(e => e.Effdate)
                .HasColumnType("date")
                .HasColumnName("effdate");
            entity.Property(e => e.InvenStatus)
                .HasMaxLength(50)
                .HasColumnName("inven_status");
            entity.Property(e => e.ItemNumber)
                .HasMaxLength(50)
                .HasColumnName("item_number");
            entity.Property(e => e.Location)
                .HasMaxLength(50)
                .HasColumnName("location");
            entity.Property(e => e.Lot).HasMaxLength(50);
            entity.Property(e => e.Order)
                .HasMaxLength(50)
                .HasColumnName("order");
            entity.Property(e => e.ProdLine)
                .HasMaxLength(255)
                .HasColumnName("Prod_Line");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TranType)
                .HasMaxLength(50)
                .HasColumnName("tran_type");
        });

        modelBuilder.Entity<TransExport>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("trans_export");

            entity.Property(e => e.ChangeQty).HasColumnName("change_qty");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.Effdate)
                .HasColumnType("date")
                .HasColumnName("effdate");
            entity.Property(e => e.InvenStatus)
                .HasMaxLength(50)
                .HasColumnName("inven_status");
            entity.Property(e => e.ItemNumber)
                .HasMaxLength(50)
                .HasColumnName("item_number");
            entity.Property(e => e.Location)
                .HasMaxLength(50)
                .HasColumnName("location");
            entity.Property(e => e.Lot).HasMaxLength(50);
            entity.Property(e => e.Order)
                .HasMaxLength(50)
                .HasColumnName("order");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TranType)
                .HasMaxLength(50)
                .HasColumnName("tran_type");
        });

        modelBuilder.Entity<TransGw>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TRANS_GW");

            entity.Property(e => e.ChangeQty).HasColumnName("change_qty");
            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("date");
            entity.Property(e => e.Description1).HasMaxLength(255);
            entity.Property(e => e.Effdate)
                .HasColumnType("date")
                .HasColumnName("effdate");
            entity.Property(e => e.InvenStatus)
                .HasMaxLength(50)
                .HasColumnName("inven_status");
            entity.Property(e => e.ItemNumber)
                .HasMaxLength(50)
                .HasColumnName("item_number");
            entity.Property(e => e.Location)
                .HasMaxLength(50)
                .HasColumnName("location");
            entity.Property(e => e.Lot).HasMaxLength(50);
            entity.Property(e => e.Order)
                .HasMaxLength(50)
                .HasColumnName("order");
            entity.Property(e => e.ProdLine)
                .HasMaxLength(255)
                .HasColumnName("Prod_Line");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TranType)
                .HasMaxLength(50)
                .HasColumnName("tran_type");
        });

        modelBuilder.Entity<WoBillBrowse>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("wo_bill_browse");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IssueDate)
                .HasColumnType("datetime")
                .HasColumnName("issue_date");
            entity.Property(e => e.PartNumber)
                .IsUnicode(false)
                .HasColumnName("part_number");
            entity.Property(e => e.QtyIssued).HasColumnName("qty_issued");
            entity.Property(e => e.QtyReq).HasColumnName("qty_req");
            entity.Property(e => e.QtyToIssue).HasColumnName("qty_to_issue");
            entity.Property(e => e.WorkOrder).HasColumnName("work_order");
        });

        modelBuilder.Entity<WoBrowse>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("wo_browse");

            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemNumber)
                .IsUnicode(false)
                .HasColumnName("item_number");
            entity.Property(e => e.Lot)
                .IsUnicode(false)
                .HasColumnName("lot");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderQty).HasColumnName("order_qty");
            entity.Property(e => e.QtyComp).HasColumnName("qty_comp");
            entity.Property(e => e.WoStatus)
                .IsUnicode(false)
                .HasColumnName("wo_status");
            entity.Property(e => e.WorkOrder).HasColumnName("work_order");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
