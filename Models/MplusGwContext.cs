using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MPLUS_GW_WebCore.Models;

public partial class MplusGwContext : DbContext
{
    public MplusGwContext()
    {
    }

    public MplusGwContext(DbContextOptions<MplusGwContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<TblCalcTimeDivLine> TblCalcTimeDivLines { get; set; }

    public virtual DbSet<TblChecksheetEntryValue> TblChecksheetEntryValues { get; set; }

    public virtual DbSet<TblChecksheetEntryValueHistory> TblChecksheetEntryValueHistories { get; set; }

    public virtual DbSet<TblChecksheetForm> TblChecksheetForms { get; set; }

    public virtual DbSet<TblChecksheetFormEntry> TblChecksheetFormEntries { get; set; }

    public virtual DbSet<TblChecksheetFormEntryHistory> TblChecksheetFormEntryHistories { get; set; }

    public virtual DbSet<TblChecksheetFormField> TblChecksheetFormFields { get; set; }

    public virtual DbSet<TblChecksheetItemAssignment> TblChecksheetItemAssignments { get; set; }

    public virtual DbSet<TblChecksheetVersion> TblChecksheetVersions { get; set; }

    public virtual DbSet<TblChecksheetWorkstationAssignment> TblChecksheetWorkstationAssignments { get; set; }

    public virtual DbSet<TblChecksheetsUpload> TblChecksheetsUploads { get; set; }

    public virtual DbSet<TblDetailTc> TblDetailTcs { get; set; }

    public virtual DbSet<TblDetailTcmay> TblDetailTcmays { get; set; }

    public virtual DbSet<TblDetailWofrequency> TblDetailWofrequencies { get; set; }

    public virtual DbSet<TblDetailsPreMaterial> TblDetailsPreMaterials { get; set; }

    public virtual DbSet<TblDivLineForLot> TblDivLineForLots { get; set; }

    public virtual DbSet<TblDivLineMcdetail> TblDivLineMcdetails { get; set; }

    public virtual DbSet<TblDivLineProd> TblDivLineProds { get; set; }

    public virtual DbSet<TblDivMaterialPrintLabel> TblDivMaterialPrintLabels { get; set; }

    public virtual DbSet<TblDivMcprod> TblDivMcprods { get; set; }

    public virtual DbSet<TblDivideLineForMaterial> TblDivideLineForMaterials { get; set; }

    public virtual DbSet<TblExportWh> TblExportWhs { get; set; }

    public virtual DbSet<TblHistoryLogin> TblHistoryLogins { get; set; }

    public virtual DbSet<TblImportedItem> TblImportedItems { get; set; }

    public virtual DbSet<TblInventoryMe> TblInventoryMes { get; set; }

    public virtual DbSet<TblItemLocation> TblItemLocations { get; set; }

    public virtual DbSet<TblItemValTc> TblItemValTcs { get; set; }

    public virtual DbSet<TblLeadTime> TblLeadTimes { get; set; }

    public virtual DbSet<TblLocation> TblLocations { get; set; }

    public virtual DbSet<TblLocationC> TblLocationCs { get; set; }

    public virtual DbSet<TblLocationError> TblLocationErrors { get; set; }

    public virtual DbSet<TblLocationTansuat> TblLocationTansuats { get; set; }

    public virtual DbSet<TblMachineValTc> TblMachineValTcs { get; set; }

    public virtual DbSet<TblManageMaterial> TblManageMaterials { get; set; }

    public virtual DbSet<TblMasterCycleTime> TblMasterCycleTimes { get; set; }

    public virtual DbSet<TblMasterError> TblMasterErrors { get; set; }

    public virtual DbSet<TblMasterJig> TblMasterJigs { get; set; }

    public virtual DbSet<TblMasterMachine> TblMasterMachines { get; set; }

    public virtual DbSet<TblMasterPosition> TblMasterPositions { get; set; }

    public virtual DbSet<TblMasterProductItem> TblMasterProductItems { get; set; }

    public virtual DbSet<TblMasterTc> TblMasterTcs { get; set; }

    public virtual DbSet<TblMasterTool> TblMasterTools { get; set; }

    public virtual DbSet<TblPreImportItem> TblPreImportItems { get; set; }

    public virtual DbSet<TblProdLine> TblProdLines { get; set; }

    public virtual DbSet<TblRecevingPlme> TblRecevingPlmes { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblSection> TblSections { get; set; }

    public virtual DbSet<TblShift> TblShifts { get; set; }

    public virtual DbSet<TblShiftSchedule> TblShiftSchedules { get; set; }

    public virtual DbSet<TblStock> TblStocks { get; set; }

    public virtual DbSet<TblSubMaterial> TblSubMaterials { get; set; }

    public virtual DbSet<TblTansuat> TblTansuats { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

    public virtual DbSet<TblWorkOrderMe> TblWorkOrderMes { get; set; }

    public virtual DbSet<TblWorkOrderProcessing> TblWorkOrderProcessings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=10.239.1.54;Initial Catalog=MPLUS_GW;User ID=sa;Password=123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregatedCounter>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK_HangFire_CounterAggregated");

            entity.ToTable("AggregatedCounter", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Job");

            entity.ToTable("Job", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName").HasFilter("([StateName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(20);
        });

        modelBuilder.Entity<JobParameter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Name }).HasName("PK_HangFire_JobParameter");

            entity.ToTable("JobParameter", "HangFire");

            entity.Property(e => e.Name).HasMaxLength(40);

            entity.HasOne(d => d.Job).WithMany(p => p.JobParameters)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_JobParameter_Job");
        });

        modelBuilder.Entity<JobQueue>(entity =>
        {
            entity.HasKey(e => new { e.Queue, e.Id }).HasName("PK_HangFire_JobQueue");

            entity.ToTable("JobQueue", "HangFire");

            entity.Property(e => e.Queue).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FetchedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_List");

            entity.ToTable("List", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("PK_HangFire_Schema");

            entity.ToTable("Schema", "HangFire");

            entity.Property(e => e.Version).ValueGeneratedNever();
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Server");

            entity.ToTable("Server", "HangFire");

            entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Value }).HasName("PK_HangFire_Set");

            entity.ToTable("Set", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(256);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Id }).HasName("PK_HangFire_State");

            entity.ToTable("State", "HangFire");

            entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(100);

            entity.HasOne(d => d.Job).WithMany(p => p.States)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_State_Job");
        });

        modelBuilder.Entity<TblCalcTimeDivLine>(entity =>
        {
            entity.HasKey(e => e.CalcTimeId);

            entity.ToTable("tbl_CalcTimeDivLine");

            entity.Property(e => e.CalcTimeId).HasColumnName("CalcTimeID");
            entity.Property(e => e.Character)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CycleTime).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NgayDuDinhSanXuat).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.SoTt).HasColumnName("SoTT");
            entity.Property(e => e.ThoiGianSanXuat).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblChecksheetEntryValue>(entity =>
        {
            entity.HasKey(e => e.EntryValueId).HasName("PK__tbl_Chec__241B2F3AED494D3B");

            entity.ToTable("tbl_ChecksheetEntryValues");

            entity.HasIndex(e => e.FormEntryId, "IX_ChecksheetEntryValues_FormEntryId");

            entity.HasOne(d => d.FormEntry).WithMany(p => p.TblChecksheetEntryValues)
                .HasForeignKey(d => d.FormEntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CSEV_ChecksheetFormEntry");
        });

        modelBuilder.Entity<TblChecksheetEntryValueHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__tbl_Chec__4D7B4ABDA14B8282");

            entity.ToTable("tbl_ChecksheetEntryValueHistory");

            entity.HasIndex(e => e.ActionAt, "IX_ChecksheetEntryValueHistory_ActionAt");

            entity.HasIndex(e => e.ActionBy, "IX_ChecksheetEntryValueHistory_ActionBy");

            entity.HasIndex(e => e.ActionType, "IX_ChecksheetEntryValueHistory_ActionType");

            entity.HasIndex(e => e.FormEntryId, "IX_ChecksheetEntryValueHistory_FormEntryId");

            entity.Property(e => e.ActionAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionBy).HasMaxLength(100);
            entity.Property(e => e.ActionType).HasMaxLength(50);
        });

        modelBuilder.Entity<TblChecksheetForm>(entity =>
        {
            entity.HasKey(e => e.FormId).HasName("PK__tbl_Chec__FB05B7DD2383B392");

            entity.ToTable("tbl_ChecksheetForms");

            entity.HasIndex(e => e.ChecksheetVersionId, "IX_ChecksheetForms_ChecksheetVersionId");

            entity.HasIndex(e => e.IsActive, "IX_ChecksheetForms_IsActive");

            entity.Property(e => e.FormName).HasMaxLength(250);
            entity.Property(e => e.FormPosition).HasMaxLength(250);
            entity.Property(e => e.FormType).HasMaxLength(200);

            entity.HasOne(d => d.ChecksheetVersion).WithMany(p => p.TblChecksheetForms)
                .HasForeignKey(d => d.ChecksheetVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CFS_ChecksheetVersions");
        });

        modelBuilder.Entity<TblChecksheetFormEntry>(entity =>
        {
            entity.HasKey(e => e.FormEntryId).HasName("PK__tbl_Chec__414A4467A72BBB1A");

            entity.ToTable("tbl_ChecksheetFormEntry");

            entity.HasIndex(e => e.CreatedAt, "IX_ChecksheetFormEntry_CreatedAt");

            entity.HasIndex(e => e.ChecksheetVerId, "IX_ChecksheetFormEntry_FormId");

            entity.HasIndex(e => e.IsAbnormal, "IX_ChecksheetFormEntry_IsAbnormal");

            entity.HasIndex(e => e.IsExported, "IX_ChecksheetFormEntry_IsExported");

            entity.HasIndex(e => e.IsLeaderApproved, "IX_ChecksheetFormEntry_IsLeaderApproved");

            entity.HasIndex(e => e.IsStopped, "IX_ChecksheetFormEntry_IsStopped");

            entity.HasIndex(e => e.ItemAssignmentId, "IX_ChecksheetFormEntry_ItemAssignmentId");

            entity.HasIndex(e => e.ProcessStatus, "IX_ChecksheetFormEntry_Status");

            entity.HasIndex(e => e.WorkOrderCode, "IX_ChecksheetFormEntry_WorkOrderCode");

            entity.Property(e => e.AbnormalReportedAt).HasColumnType("datetime");
            entity.Property(e => e.AbnormalReportedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ExportedAt).HasColumnType("datetime");
            entity.Property(e => e.ExportedBy).HasMaxLength(100);
            entity.Property(e => e.LeaderApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.LeaderApprovedBy).HasMaxLength(100);
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProcessStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("('InProgress')");
            entity.Property(e => e.QtyNg).HasColumnName("QtyNG");
            entity.Property(e => e.QtyOk).HasColumnName("QtyOK");
            entity.Property(e => e.RestartApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.RestartApprovedBy).HasMaxLength(100);
            entity.Property(e => e.RestartTimeUtc)
                .HasColumnType("datetime")
                .HasColumnName("RestartTimeUTC");
            entity.Property(e => e.StoppedAt).HasColumnType("datetime");
            entity.Property(e => e.StoppedBy).HasMaxLength(100);
            entity.Property(e => e.TrayNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrderCode).HasMaxLength(50);

            entity.HasOne(d => d.ChecksheetVer).WithMany(p => p.TblChecksheetFormEntries)
                .HasForeignKey(d => d.ChecksheetVerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CFE_ChecksheetForm");

            entity.HasOne(d => d.ItemAssignment).WithMany(p => p.TblChecksheetFormEntries)
                .HasForeignKey(d => d.ItemAssignmentId)
                .HasConstraintName("FK_CFE_ItemAssignment");
        });

        modelBuilder.Entity<TblChecksheetFormEntryHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__tbl_Chec__4D7B4ABD1DA16AE0");

            entity.ToTable("tbl_ChecksheetFormEntryHistory");

            entity.HasIndex(e => e.ActionAt, "IX_ChecksheetFormEntryHistory_ActionAt");

            entity.HasIndex(e => e.ActionType, "IX_ChecksheetFormEntryHistory_ActionType");

            entity.HasIndex(e => e.OriginalFormEntryId, "IX_ChecksheetFormEntryHistory_OriginalFormEntryId");

            entity.HasIndex(e => e.Status, "IX_ChecksheetFormEntryHistory_Status");

            entity.HasIndex(e => e.WorkOrderCode, "IX_ChecksheetFormEntryHistory_WorkOrderCode");

            entity.Property(e => e.AbnormalReportedAt).HasColumnType("datetime");
            entity.Property(e => e.AbnormalReportedBy).HasMaxLength(100);
            entity.Property(e => e.ActionAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionBy).HasMaxLength(100);
            entity.Property(e => e.ActionType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ExportedAt).HasColumnType("datetime");
            entity.Property(e => e.ExportedBy).HasMaxLength(100);
            entity.Property(e => e.LeaderApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.LeaderApprovedBy).HasMaxLength(100);
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.QtyNg).HasColumnName("QtyNG");
            entity.Property(e => e.QtyOk).HasColumnName("QtyOK");
            entity.Property(e => e.RestartApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.RestartApprovedBy).HasMaxLength(100);
            entity.Property(e => e.RestartTimeUtc)
                .HasColumnType("datetime")
                .HasColumnName("RestartTimeUTC");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StoppedAt).HasColumnType("datetime");
            entity.Property(e => e.StoppedBy).HasMaxLength(100);
            entity.Property(e => e.TrayNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrderCode).HasMaxLength(50);
        });

        modelBuilder.Entity<TblChecksheetFormField>(entity =>
        {
            entity.HasKey(e => e.FieldId).HasName("PK__tbl_Chec__C8B6FF079E3E98D1");

            entity.ToTable("tbl_ChecksheetFormFields");

            entity.HasIndex(e => e.FormId, "IX_ChecksheetFormFields_FormId");

            entity.Property(e => e.ColClass)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DataSource).HasMaxLength(200);
            entity.Property(e => e.ElementId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ElementType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InputType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LabelText).HasMaxLength(255);
            entity.Property(e => e.SectionId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartCell)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Form).WithMany(p => p.TblChecksheetFormFields)
                .HasForeignKey(d => d.FormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CFFS_ChecksheetForms");
        });

        modelBuilder.Entity<TblChecksheetItemAssignment>(entity =>
        {
            entity.HasKey(e => e.ItemAssignmentId).HasName("PK__tbl_Chec__A7A6CB019D92DEBC");

            entity.ToTable("tbl_ChecksheetItemAssignments");

            entity.HasIndex(e => e.ChecksheetId, "IX_ChecksheetItemAssignments_ChecksheetId");

            entity.HasIndex(e => new { e.ProductItem, e.ProductLot }, "IX_ChecksheetItemAssignments_ProductItem_ProductLot");

            entity.Property(e => e.AssignmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsLocked).HasDefaultValueSql("((0))");
            entity.Property(e => e.ProductItem)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ProductLot)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Checksheet).WithMany(p => p.TblChecksheetItemAssignments)
                .HasForeignKey(d => d.ChecksheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ICA_Checksheets");

            entity.HasOne(d => d.LastUsedChecksheetVersion).WithMany(p => p.TblChecksheetItemAssignments)
                .HasForeignKey(d => d.LastUsedChecksheetVersionId)
                .HasConstraintName("FK_ICA_LastChecksheetVer");
        });

        modelBuilder.Entity<TblChecksheetVersion>(entity =>
        {
            entity.HasKey(e => e.ChecksheetVersionId).HasName("PK__tbl_Chec__54E32D23ACA2B502");

            entity.ToTable("tbl_ChecksheetVersions");

            entity.HasIndex(e => e.ChecksheetId, "IX_ChecksheetVersions_ChecksheetId");

            entity.HasIndex(e => e.Statusname, "IX_ChecksheetVersions_Statusname");

            entity.HasIndex(e => new { e.ChecksheetId, e.VersionNumber }, "UQ_ChecksheetVer_Code_Ver").IsUnique();

            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);
            entity.Property(e => e.ChecksheetType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.IsChangeForm).HasDefaultValueSql("((0))");
            entity.Property(e => e.PositionWorkingCode)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SheetName).HasMaxLength(500);
            entity.Property(e => e.Statusname)
                .HasMaxLength(100)
                .HasDefaultValueSql("('Draft')");
            entity.Property(e => e.VersionNumber).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Checksheet).WithMany(p => p.TblChecksheetVersions)
                .HasForeignKey(d => d.ChecksheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PK_ChecksheetVers_Checksheets");
        });

        modelBuilder.Entity<TblChecksheetWorkstationAssignment>(entity =>
        {
            entity.HasKey(e => e.WorkstationAssignmentId).HasName("PK__tbl_Chec__0DF661FC3C1C279F");

            entity.ToTable("tbl_ChecksheetWorkstationAssignments");

            entity.HasIndex(e => e.ChecksheetId, "IX_ChecksheetWorkstationAssignments_ChecksheetId");

            entity.HasIndex(e => e.WorkstationId, "IX_ChecksheetWorkstationAssignments_WorkstationId");

            entity.HasIndex(e => new { e.WorkstationId, e.ChecksheetId }, "UNQ_WCA").IsUnique();

            entity.Property(e => e.AssignmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Checksheet).WithMany(p => p.TblChecksheetWorkstationAssignments)
                .HasForeignKey(d => d.ChecksheetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WCA_Checksheets");

            entity.HasOne(d => d.LastUsedChecksheetVersion).WithMany(p => p.TblChecksheetWorkstationAssignments)
                .HasForeignKey(d => d.LastUsedChecksheetVersionId)
                .HasConstraintName("FK_WCA_LastChecksheetVer");

            entity.HasOne(d => d.Workstation).WithMany(p => p.TblChecksheetWorkstationAssignments)
                .HasForeignKey(d => d.WorkstationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WCA_Workstations");
        });

        modelBuilder.Entity<TblChecksheetsUpload>(entity =>
        {
            entity.HasKey(e => e.ChecksheetId).HasName("PK__tbl_Chec__B9BBC624C8F814E0");

            entity.ToTable("tbl_ChecksheetsUpload");

            entity.HasIndex(e => e.ChecksheetCode, "IX_ChecksheetsUpload_ChecksheetCode");

            entity.HasIndex(e => e.ChecksheetCode, "UQ__tbl_Chec__AC6EFC5CF9BFA2C7").IsUnique();

            entity.Property(e => e.ChecksheetCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CurrentVersion).WithMany(p => p.TblChecksheetsUploads)
                .HasForeignKey(d => d.CurrentVersionId)
                .HasConstraintName("FK_Checksheets_CurrentVersion");

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblChecksheetsUploads)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_tbl_ChecksheetsUpload_tbl_Location");
        });

        modelBuilder.Entity<TblDetailTc>(entity =>
        {
            entity.HasKey(e => e.IdDetail).HasName("PK_tbl_Detail_TC2");

            entity.ToTable("tbl_Detail_TC");

            entity.Property(e => e.IdDetail).HasColumnName("ID_detail");
            entity.Property(e => e.IdTc).HasColumnName("ID_tc");
            entity.Property(e => e.MoTa).HasMaxLength(250);
            entity.Property(e => e.TenTc)
                .HasMaxLength(500)
                .HasColumnName("Ten_TC");
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.ValueDecimal)
                .HasColumnType("decimal(18, 8)")
                .HasColumnName("Value_decimal");
            entity.Property(e => e.ValueInt).HasColumnName("Value_int");
            entity.Property(e => e.ValueText)
                .HasMaxLength(350)
                .HasColumnName("Value_text");
            entity.Property(e => e.ValueUnit)
                .HasMaxLength(20)
                .HasColumnName("Value_Unit");
        });

        modelBuilder.Entity<TblDetailTcmay>(entity =>
        {
            entity.ToTable("tbl_Detail_TCMay");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CdmDonvi)
                .HasMaxLength(10)
                .HasColumnName("CDM_Donvi");
            entity.Property(e => e.CdmMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("CDM_Max");
            entity.Property(e => e.CdmMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("CDM_Min");
            entity.Property(e => e.CodeTcmay)
                .HasMaxLength(50)
                .HasColumnName("Code_TCMay");
            entity.Property(e => e.GcApsuatdapDonvi)
                .HasMaxLength(10)
                .HasColumnName("GC_Apsuatdap_Donvi");
            entity.Property(e => e.GcApsuatdapMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatdap_Max");
            entity.Property(e => e.GcApsuatdapMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatdap_Min");
            entity.Property(e => e.GcApsuatkDonvi)
                .HasMaxLength(10)
                .HasColumnName("GC_Apsuatk_Donvi");
            entity.Property(e => e.GcApsuatkMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatk_Max");
            entity.Property(e => e.GcApsuatkMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatk_Min");
            entity.Property(e => e.GcApsuattDonvi)
                .HasMaxLength(10)
                .HasColumnName("GC_Apsuatt_Donvi");
            entity.Property(e => e.GcApsuattMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatt_Max");
            entity.Property(e => e.GcApsuattMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GC_Apsuatt_Min");
            entity.Property(e => e.GcdApsuatkDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCD_Apsuatk_Donvi");
            entity.Property(e => e.GcdApsuatkMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Apsuatk_Max");
            entity.Property(e => e.GcdApsuatkMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Apsuatk_Min");
            entity.Property(e => e.GcdPanmeDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCD_Panme_Donvi");
            entity.Property(e => e.GcdPanmeMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Panme_Max");
            entity.Property(e => e.GcdPanmeMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Panme_Min");
            entity.Property(e => e.GcdTemgcDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCD_Temgc_Donvi");
            entity.Property(e => e.GcdTemgcMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Temgc_Max");
            entity.Property(e => e.GcdTemgcMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Temgc_Min");
            entity.Property(e => e.GcdTimedmDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCD_Timedm_Donvi");
            entity.Property(e => e.GcdTimedmMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Timedm_Max");
            entity.Property(e => e.GcdTimedmMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Timedm_Min");
            entity.Property(e => e.GcdTimegcDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCD_Timegc_Donvi");
            entity.Property(e => e.GcdTimegcMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Timegc_Max");
            entity.Property(e => e.GcdTimegcMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCD_Timegc_Min");
            entity.Property(e => e.GcdmApsuattkDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCDM_Apsuattk_Donvi");
            entity.Property(e => e.GcdmApsuattkMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Apsuattk_Max");
            entity.Property(e => e.GcdmApsuattkMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Apsuattk_Min");
            entity.Property(e => e.GcdmTimegcDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCDM_Timegc_Donvi");
            entity.Property(e => e.GcdmTimegcMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Timegc_Max");
            entity.Property(e => e.GcdmTimegcMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Timegc_Min");
            entity.Property(e => e.GcdmTimetkDonvi)
                .HasMaxLength(10)
                .HasColumnName("GCDM_Timetk_Donvi");
            entity.Property(e => e.GcdmTimetkMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Timetk_Max");
            entity.Property(e => e.GcdmTimetkMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("GCDM_Timetk_Min");
            entity.Property(e => e.IcApsuatciDonvi)
                .HasMaxLength(10)
                .HasColumnName("IC_Apsuatci_Donvi");
            entity.Property(e => e.IcApsuatciMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatci_Max");
            entity.Property(e => e.IcApsuatciMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatci_Min");
            entity.Property(e => e.IcApsuatkDonvi)
                .HasMaxLength(10)
                .HasColumnName("IC_Apsuatk_Donvi");
            entity.Property(e => e.IcApsuatkMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatk_Max");
            entity.Property(e => e.IcApsuatkMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatk_Min");
            entity.Property(e => e.IcApsuattDonvi)
                .HasMaxLength(10)
                .HasColumnName("IC_Apsuatt_Donvi");
            entity.Property(e => e.IcApsuattMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatt_Max");
            entity.Property(e => e.IcApsuattMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Apsuatt_Min");
            entity.Property(e => e.IcTembgnDonvi)
                .HasMaxLength(10)
                .HasColumnName("IC_Tembgn_Donvi");
            entity.Property(e => e.IcTembgnMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Tembgn_Max");
            entity.Property(e => e.IcTembgnMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Tembgn_Min");
            entity.Property(e => e.IcTimeinDonvi)
                .HasMaxLength(10)
                .HasColumnName("IC_Timein_Donvi");
            entity.Property(e => e.IcTimeinMax)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Timein_Max");
            entity.Property(e => e.IcTimeinMin)
                .HasColumnType("decimal(18, 5)")
                .HasColumnName("IC_Timein_Min");
            entity.Property(e => e.NameTcmay)
                .HasMaxLength(150)
                .HasColumnName("Name_TCMay");
        });

        modelBuilder.Entity<TblDetailWofrequency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Deta__3214EC2758808D24");

            entity.ToTable("tbl_DetailWOFrequency");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.Frequency).WithMany(p => p.TblDetailWofrequencies)
                .HasForeignKey(d => d.FrequencyId)
                .HasConstraintName("FK_FrequencyId_DetailWOFrequency");

            entity.HasOne(d => d.Position).WithMany(p => p.TblDetailWofrequencies)
                .HasForeignKey(d => d.PositionId)
                .HasConstraintName("FK_PostionId_DetailWOFrequency");

            entity.HasOne(d => d.WoProcess).WithMany(p => p.TblDetailWofrequencies)
                .HasForeignKey(d => d.WoProcessId)
                .HasConstraintName("FK_WoProcessId_DetailWOFrequency");
        });

        modelBuilder.Entity<TblDetailsPreMaterial>(entity =>
        {
            entity.ToTable("tbl_DetailsPreMaterials");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateImport).HasColumnType("date");
            entity.Property(e => e.IdItemImport).HasColumnName("Id_ItemImport");
            entity.Property(e => e.MaterialCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StatusExported)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdItemImportNavigation).WithMany(p => p.TblDetailsPreMaterials)
                .HasForeignKey(d => d.IdItemImport)
                .HasConstraintName("FK_tbl_DetailsPreMaterials_tbl_DetailsPreMaterials");
        });

        modelBuilder.Entity<TblDivLineForLot>(entity =>
        {
            entity.ToTable("tbl_DivLineForLot");

            entity.Property(e => e.LotDivLine)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblDivLineMcdetail>(entity =>
        {
            entity.HasKey(e => e.DivDetailId).HasName("PK__tbl_DivL__5DE0057328F45F04");

            entity.ToTable("tbl_DivLineMCDetails");

            entity.Property(e => e.DateProd).HasMaxLength(50);
            entity.Property(e => e.MachineShift)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.ProdMcid).HasColumnName("ProdMCId");
            entity.Property(e => e.Remarks).HasMaxLength(255);
            entity.Property(e => e.ShiftLabel)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TypeLabel).HasMaxLength(100);
            entity.Property(e => e.WorkOrder).HasMaxLength(100);

            entity.HasOne(d => d.ProdMc).WithMany(p => p.TblDivLineMcdetails)
                .HasForeignKey(d => d.ProdMcid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DivMC_Details");
        });

        modelBuilder.Entity<TblDivLineProd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_DivL__3214EC272E1A0DFB");

            entity.ToTable("tbl_DivLineProd");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_DivLineProd_ID_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ChangeControl)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Character)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DateProd).HasColumnType("date");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblDivLineProds)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_tbl_DivLineProd_tbl_IdLocation");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblDivLineProds)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_DivLineProd_tbl_Users");
        });

        modelBuilder.Entity<TblDivMaterialPrintLabel>(entity =>
        {
            entity.HasKey(e => e.IdMaterial).HasName("PK__tbl_Mate__94356E586B2A0FEB");

            entity.ToTable("tbl_DivMaterialPrintLabels");

            entity.Property(e => e.LotMaterial).HasMaxLength(100);
            entity.Property(e => e.MachineShift)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaterialCode).HasMaxLength(100);
            entity.Property(e => e.ProdMcid).HasColumnName("ProdMCId");
            entity.Property(e => e.ShiftLabel)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrder).HasMaxLength(100);

            entity.HasOne(d => d.ProdMc).WithMany(p => p.TblDivMaterialPrintLabels)
                .HasForeignKey(d => d.ProdMcid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MaterialPrintLabelDiv_Details");
        });

        modelBuilder.Entity<TblDivMcprod>(entity =>
        {
            entity.HasKey(e => e.ProdMcid).HasName("PK__tbl_DivM__9466A6B2BBCC5E41");

            entity.ToTable("tbl_DivMCProd");

            entity.Property(e => e.ProdMcid).HasColumnName("ProdMCId");
            entity.Property(e => e.Character)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LotNo).HasMaxLength(50);
            entity.Property(e => e.ProductCode).HasMaxLength(100);
            entity.Property(e => e.WorkOrder).HasMaxLength(100);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblDivMcprods)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_DivMC_Location");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblDivMcprods)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_DivMC_User");
        });

        modelBuilder.Entity<TblDivideLineForMaterial>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbl_DivideLineForMaterials");

            entity.Property(e => e.CongDoan).HasMaxLength(50);
            entity.Property(e => e.DivideLineMaterialsId)
                .ValueGeneratedOnAdd()
                .HasColumnName("DivideLineMaterialsID");
            entity.Property(e => e.LotNvl)
                .HasMaxLength(100)
                .HasColumnName("LotNVL");
            entity.Property(e => e.MaNvl)
                .HasMaxLength(100)
                .HasColumnName("MaNVL");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.WorkOrder).HasMaxLength(50);
        });

        modelBuilder.Entity<TblExportWh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Expo__3214EC27FBC33FA5");

            entity.ToTable("tbl_ExportWH");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateImport).HasColumnType("date");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Note1)
                .HasMaxLength(500)
                .HasColumnName("Note_1");
            entity.Property(e => e.Progress)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Whlocation)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("WHLocation");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblExportWhs)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_ExportWH_tbl_Users");
        });

        modelBuilder.Entity<TblHistoryLogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Hist__3214EC272A84D09A");

            entity.ToTable("tbl_HistoryLogin");

            entity.HasIndex(e => e.IdPosition, "IX_tbl_HistoryLogin_ID_Position");

            entity.HasIndex(e => e.IdShift, "IX_tbl_HistoryLogin_ID_Shift");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdPosition).HasColumnName("ID_Position");
            entity.Property(e => e.IdShift).HasColumnName("ID_Shift");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.Remarks).HasMaxLength(250);
            entity.Property(e => e.TimeLogin).HasColumnType("datetime");
            entity.Property(e => e.TimeLogout).HasColumnType("datetime");

            entity.HasOne(d => d.IdShiftNavigation).WithMany(p => p.TblHistoryLogins)
                .HasForeignKey(d => d.IdShift)
                .HasConstraintName("FK_tbl_HistoryLogin_tbl_Shift");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblHistoryLogins)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_HistoryLogin_tbl_Users");
        });

        modelBuilder.Entity<TblImportedItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Impo__3214EC27E5E5B93C");

            entity.ToTable("tbl_ImportedItem");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_ImportedItem_ID_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderShipment)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RequestNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TimeImport).HasColumnType("datetime");
            entity.Property(e => e.TimeSterilization)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblImportedItems)
                .HasForeignKey(d => d.IdLocation)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_tbl_ImportedItem_tbl_Location");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblImportedItems)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_ImportedItem_tbl_Users");
        });

        modelBuilder.Entity<TblInventoryMe>(entity =>
        {
            entity.ToTable("tbl_InventoryMES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Orderno).IsUnicode(false);
        });

        modelBuilder.Entity<TblItemLocation>(entity =>
        {
            entity.ToTable("tbl_Item_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblItemValTc>(entity =>
        {
            entity.ToTable("tbl_Item_ValTC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdNhomTc).HasColumnName("ID_nhomTC");
            entity.Property(e => e.IdValTc).HasColumnName("ID_ValTC");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remark).HasMaxLength(500);
        });

        modelBuilder.Entity<TblLeadTime>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Lead__3214EC27AD7C0495");

            entity.ToTable("tbl_LeadTime");

            entity.HasIndex(e => e.IdShift, "IX_tbl_LeadTime_ID_Shift");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DateOfTime).HasColumnType("date");
            entity.Property(e => e.IdShift).HasColumnName("ID_Shift");

            entity.HasOne(d => d.IdShiftNavigation).WithMany(p => p.TblLeadTimes)
                .HasForeignKey(d => d.IdShift)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_leadtime_shift");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblLeadTimes)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_LeadTime_tbl_Users");
        });

        modelBuilder.Entity<TblLocation>(entity =>
        {
            entity.HasKey(e => e.IdLocation).HasName("PK__tbl_Loca__2F2C70A77FBA6C94");

            entity.ToTable("tbl_Location");

            entity.HasIndex(e => e.IdSection, "IX_tbl_Location_ID_Section");

            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.IdLocationParent).HasColumnName("ID_Location_Parent");
            entity.Property(e => e.IdSection).HasColumnName("ID_Section");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LocationName).HasMaxLength(100);
            entity.Property(e => e.XuatKhac).HasColumnName("Xuat_khac");

            entity.HasOne(d => d.IdSectionNavigation).WithMany(p => p.TblLocations)
                .HasForeignKey(d => d.IdSection)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_location_section");
        });

        modelBuilder.Entity<TblLocationC>(entity =>
        {
            entity.ToTable("tbl_Location_c");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdChaC).HasColumnName("IdCha_c");
            entity.Property(e => e.LocationCodeC)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("LocationCode_c");
            entity.Property(e => e.LocationNameC)
                .HasMaxLength(50)
                .HasColumnName("LocationName_c");

            entity.HasOne(d => d.IdChaCNavigation).WithMany(p => p.TblLocationCs)
                .HasForeignKey(d => d.IdChaC)
                .HasConstraintName("FK_tbl_Location_c_tbl_Location");
        });

        modelBuilder.Entity<TblLocationError>(entity =>
        {
            entity.ToTable("tbl_Location_Error");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdError).HasColumnName("ID_error");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
        });

        modelBuilder.Entity<TblLocationTansuat>(entity =>
        {
            entity.ToTable("tbl_Location_Tansuat");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdLocationc).HasColumnName("ID_locationc");
            entity.Property(e => e.IdTansuat).HasColumnName("ID_tansuat");

            entity.HasOne(d => d.IdLocationcNavigation).WithMany(p => p.TblLocationTansuats)
                .HasForeignKey(d => d.IdLocationc)
                .HasConstraintName("FK_tbl_Location_Tansuat_tbl_Location_c");

            entity.HasOne(d => d.IdTansuatNavigation).WithMany(p => p.TblLocationTansuats)
                .HasForeignKey(d => d.IdTansuat)
                .HasConstraintName("FK_tbl_Location_Tansuat_tbl_Tansuat");
        });

        modelBuilder.Entity<TblMachineValTc>(entity =>
        {
            entity.ToTable("tbl_Machine_ValTC");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdNhomTc).HasColumnName("ID_nhomTC");
            entity.Property(e => e.IdValTc).HasColumnName("ID_ValTC");
            entity.Property(e => e.MachineCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remark).HasMaxLength(100);
        });

        modelBuilder.Entity<TblManageMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Mana__3214EC2779640B54");

            entity.ToTable("tbl_ManageMaterials");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookType).HasMaxLength(50);
            entity.Property(e => e.ItemCate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.RequestNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TimeUpdate).HasColumnType("date");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblManageMaterials)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_ManageBookItem_tbl_Users");
        });

        modelBuilder.Entity<TblMasterCycleTime>(entity =>
        {
            entity.HasKey(e => e.CycleTimeId);

            entity.ToTable("tbl_MasterCycleTime");

            entity.Property(e => e.CycleTimeId).HasColumnName("CycleTimeID");
            entity.Property(e => e.CycleTime).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.ProcessCode).HasMaxLength(100);
        });

        modelBuilder.Entity<TblMasterError>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Mast__3214EC27231B982A");

            entity.ToTable("tbl_MasterError");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ErrorCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ErrorName).HasMaxLength(500);
            entity.Property(e => e.Location)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NameJp)
                .HasMaxLength(500)
                .HasColumnName("NameJP");
            entity.Property(e => e.Remarks).HasMaxLength(500);
        });

        modelBuilder.Entity<TblMasterJig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Mast__3214EC278BA3C3CA");

            entity.ToTable("tbl_MasterJIG");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_MasterJIG_ID_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.IdjigParent)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("IDJigParent");
            entity.Property(e => e.JigCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.JigName).HasMaxLength(500);
            entity.Property(e => e.Progress)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblMasterJigs)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("fk_jig_location");
        });

        modelBuilder.Entity<TblMasterMachine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_MasterMachine");

            entity.ToTable("tbl_MasterMachines");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.LocationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MachineCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MachineName).HasMaxLength(200);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblMasterMachines)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_tbl_MasterMachines_tbl_Location");
        });

        modelBuilder.Entity<TblMasterPosition>(entity =>
        {
            entity.HasKey(e => e.IdPosition).HasName("PK__tbl_Posi__8F963ECE827D37A8");

            entity.ToTable("tbl_MasterPosition");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_MasterPosition_ID_Location");

            entity.Property(e => e.IdPosition).HasColumnName("ID_Position");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PositionDes).HasMaxLength(500);
            entity.Property(e => e.PositionName).HasMaxLength(100);

            entity.HasOne(d => d.IdLineNavigation).WithMany(p => p.TblMasterPositions)
                .HasForeignKey(d => d.IdLine)
                .HasConstraintName("FK_MasterPosition_ProdLine");

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblMasterPositions)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("fk_position_location");

            entity.HasOne(d => d.LocationChild).WithMany(p => p.TblMasterPositions)
                .HasForeignKey(d => d.LocationChildId)
                .HasConstraintName("FK_MasterPosition_LocationChild");
        });

        modelBuilder.Entity<TblMasterProductItem>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PK__tbl_MSIt__87415D0794C6E40F");

            entity.ToTable("tbl_MasterProductItem");

            entity.Property(e => e.IdItem).HasColumnName("ID_Item");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.ItemType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(250);
            entity.Property(e => e.Unit)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblMasterTc>(entity =>
        {
            entity.HasKey(e => e.IdTc);

            entity.ToTable("tbl_MasterTC");

            entity.Property(e => e.IdTc).HasColumnName("ID_tc");
            entity.Property(e => e.Remark).HasMaxLength(150);
            entity.Property(e => e.TcCode)
                .HasMaxLength(50)
                .HasColumnName("TC_Code");
            entity.Property(e => e.TcMay).HasColumnName("TC_May");
            entity.Property(e => e.TenTieuchuan)
                .HasMaxLength(500)
                .HasColumnName("Ten_Tieuchuan");
        });

        modelBuilder.Entity<TblMasterTool>(entity =>
        {
            entity.ToTable("tbl_MasterTools");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_MasterTools_ID_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Descriptions).HasMaxLength(500);
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.ModifyUpdate).HasColumnType("datetime");
            entity.Property(e => e.ToolCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ToolName).HasMaxLength(100);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblMasterTools)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_tbl_MasterTools_tbl_Location");
        });

        modelBuilder.Entity<TblPreImportItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_PreI__3214EC27D05BCE2B");

            entity.ToTable("tbl_PreImportItem");

            entity.HasIndex(e => e.IdLocation, "IX_tbl_PreImportItem_ID_Location");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CharacterAlp)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DateImport).HasColumnType("date");
            entity.Property(e => e.DateProd).HasColumnType("date");
            entity.Property(e => e.IdLocation).HasColumnName("ID_Location");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProgressMes)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProgressMES");
            entity.Property(e => e.StatusEx)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("StatusEX");
            entity.Property(e => e.ValueJson)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblPreImportItems)
                .HasForeignKey(d => d.IdLocation)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_location_preimport");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblPreImportItems)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_PreImportItem_tbl_Users");
        });

        modelBuilder.Entity<TblProdLine>(entity =>
        {
            entity.HasKey(e => e.IdLine).HasName("PK__tbl_Prod__93D00796C55BED72");

            entity.ToTable("tbl_ProdLine");

            entity.Property(e => e.IdLine).HasColumnName("ID_Line");
            entity.Property(e => e.LineCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LineName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Remarks).HasMaxLength(250);
        });

        modelBuilder.Entity<TblRecevingPlme>(entity =>
        {
            entity.HasKey(e => e.NewId).HasName("PK__tbl_Rece__7CC3777EB1AA915B");

            entity.ToTable("tbl_RecevingPLMes");

            entity.Property(e => e.ItemCode)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LocationCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LocationName).HasMaxLength(50);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifyUpdate).HasColumnType("datetime");
            entity.Property(e => e.OrderShipment)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("PK_tbl_Roles_1");

            entity.ToTable("tbl_Roles");

            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblSection>(entity =>
        {
            entity.HasKey(e => e.IdSection).HasName("PK__tbl_Sect__15C8DAE1E2C04CA7");

            entity.ToTable("tbl_Section");

            entity.Property(e => e.IdSection).HasColumnName("ID_Section");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.SectionName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblShift>(entity =>
        {
            entity.HasKey(e => e.IdShift).HasName("PK__tbl_Shif__BF5D0A88DA577C00");

            entity.ToTable("tbl_Shift");

            entity.Property(e => e.IdShift).HasColumnName("ID_Shift");
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.ShiftName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblShiftSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);

            entity.ToTable("tbl_ShiftSchedules");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.LocationCode).HasMaxLength(50);
            entity.Property(e => e.TypeShift).HasMaxLength(50);
        });

        modelBuilder.Entity<TblStock>(entity =>
        {
            entity.ToTable("tbl_Stock");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Date).HasColumnType("date");
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.Lotno).HasMaxLength(50);
            entity.Property(e => e.QtyStock)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Qty_stock");
        });

        modelBuilder.Entity<TblSubMaterial>(entity =>
        {
            entity.ToTable("tbl_SubMaterials");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductName).HasMaxLength(255);
        });

        modelBuilder.Entity<TblTansuat>(entity =>
        {
            entity.ToTable("tbl_Tansuat");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("PK_tbl_Users_1");

            entity.ToTable("tbl_Users");

            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EmployeeNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Has2Faenabled).HasColumnName("Has2FAEnabled");
            entity.Property(e => e.LastPasswordChange).HasColumnType("datetime");
            entity.Property(e => e.LastPingAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryPasswordHash)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdLocationNavigation).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.IdLocation)
                .HasConstraintName("FK_tbl_Users_tbl_Location1");

            entity.HasOne(d => d.IdSectionNavigation).WithMany(p => p.TblUsers)
                .HasForeignKey(d => d.IdSection)
                .HasConstraintName("FK_tbl_Users_tbl_Section1");
        });

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.ToTable("tbl_UserRoles");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_tbl_UserRoles_tbl_Roles");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_tbl_UserRoles_tbl_UserRoles");
        });

        modelBuilder.Entity<TblWorkOrderMe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tbl_Work__3214EC27B7EC604C");

            entity.ToTable("tbl_WorkOrderMES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Character)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DateProd).HasColumnType("date");
            entity.Property(e => e.InputGoodsCodeMes)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.InputGoodsCodeSeq).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ItemName).HasMaxLength(250);
            entity.Property(e => e.LotNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifyDateUpdate).HasColumnType("date");
            entity.Property(e => e.ProgressOrder)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.QtyWo).HasColumnName("QtyWO");
            entity.Property(e => e.Statusname)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TimeCreate).HasColumnType("datetime");
            entity.Property(e => e.TimeEnd).HasColumnType("datetime");
            entity.Property(e => e.TimeStart).HasColumnType("datetime");
            entity.Property(e => e.WorkOrder)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblWorkOrderProcessing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tbl_WorkOrder_Processing");

            entity.ToTable("tbl_WorkOrderProcessing");

            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.LotProcessing)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NextAction).HasMaxLength(200);
            entity.Property(e => e.PositionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProcessingStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.Woprocessing)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("WOProcessing");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
