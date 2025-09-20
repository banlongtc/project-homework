using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MPLUS_GW_WebCore.Models;

public partial class EslsystemContext : DbContext
{
    public EslsystemContext()
    {
    }

    public EslsystemContext(DbContextOptions<EslsystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<Basestationstatus> Basestationstatuses { get; set; }

    public virtual DbSet<Changelog> Changelogs { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Esllog> Esllogs { get; set; }

    public virtual DbSet<EventAction> EventActions { get; set; }

    public virtual DbSet<Eventlog> Eventlogs { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<Labelstatus> Labelstatuses { get; set; }

    public virtual DbSet<Link> Links { get; set; }

    public virtual DbSet<LinksStaging> LinksStagings { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<NhanVienNew> NhanVienNews { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblProductStaging> TblProductStagings { get; set; }

    public virtual DbSet<Unmarkednotification> Unmarkednotifications { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=10.239.1.54;Initial Catalog=ESLsystem;User ID=sa;Password=123456;TrustServerCertificate=True;");

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

        modelBuilder.Entity<Basestationstatus>(entity =>
        {
            entity.HasKey(e => e.Mac).HasName("PK__basestat__C790778D124B5D72");

            entity.ToTable("basestationstatus");

            entity.Property(e => e.Mac)
                .HasMaxLength(17)
                .HasColumnName("MAC");
            entity.Property(e => e.Channel).HasColumnName("CHANNEL");
            entity.Property(e => e.DynamicInts).HasColumnName("DYNAMIC_INTS");
            entity.Property(e => e.Esls).HasColumnName("ESLS");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(15)
                .HasColumnName("IP_ADDRESS");
            entity.Property(e => e.LanId)
                .HasMaxLength(4)
                .HasColumnName("LAN_ID");
            entity.Property(e => e.MaxLoad).HasColumnName("MAX_LOAD");
            entity.Property(e => e.MinLoad).HasColumnName("MIN_LOAD");
            entity.Property(e => e.Model)
                .HasMaxLength(10)
                .HasColumnName("MODEL");
            entity.Property(e => e.Name)
                .HasMaxLength(25)
                .HasColumnName("NAME");
            entity.Property(e => e.PanId)
                .HasMaxLength(4)
                .HasColumnName("PAN_ID");
            entity.Property(e => e.Port).HasColumnName("PORT");
            entity.Property(e => e.RadioTuning).HasColumnName("RADIO_TUNING");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .HasColumnName("STATUS");
            entity.Property(e => e.Version)
                .HasMaxLength(10)
                .HasColumnName("VERSION");
        });

        modelBuilder.Entity<Changelog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__changelo__3214EC277950EEF2");

            entity.ToTable("changelog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Basestation)
                .HasMaxLength(25)
                .HasColumnName("BASESTATION");
            entity.Property(e => e.Esl)
                .HasMaxLength(16)
                .HasColumnName("ESL");
            entity.Property(e => e.From)
                .HasMaxLength(255)
                .HasColumnName("FROM");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
            entity.Property(e => e.To)
                .HasMaxLength(255)
                .HasColumnName("TO");
            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USER");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Esllog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__esllog__3214EC27DB1BF6FA");

            entity.ToTable("esllog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Basestation)
                .HasMaxLength(25)
                .HasColumnName("BASESTATION");
            entity.Property(e => e.Esl)
                .HasMaxLength(16)
                .HasColumnName("ESL");
            entity.Property(e => e.Message)
                .HasMaxLength(256)
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
        });

        modelBuilder.Entity<EventAction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__event_ac__3214EC2745049714");

            entity.ToTable("event_actions");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Action)
                .HasMaxLength(1023)
                .HasColumnName("ACTION");
            entity.Property(e => e.Condition)
                .HasMaxLength(255)
                .HasColumnName("CONDITION");
            entity.Property(e => e.CooldownTime).HasColumnName("COOLDOWN_TIME");
            entity.Property(e => e.Enabled)
                .HasColumnType("decimal(1, 0)")
                .HasColumnName("ENABLED");
            entity.Property(e => e.Event)
                .HasMaxLength(64)
                .HasColumnName("EVENT");
            entity.Property(e => e.Parameters)
                .HasMaxLength(255)
                .HasColumnName("PARAMETERS");
            entity.Property(e => e.Trigger)
                .HasMaxLength(20)
                .HasColumnName("TRIGGER");
        });

        modelBuilder.Entity<Eventlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__eventlog__3214EC27AE1CC77E");

            entity.ToTable("eventlog");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Event)
                .HasMaxLength(64)
                .HasColumnName("EVENT");
            entity.Property(e => e.Mac)
                .HasMaxLength(17)
                .HasColumnName("MAC");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
            entity.Property(e => e.Trigger)
                .HasMaxLength(20)
                .HasColumnName("TRIGGER");
            entity.Property(e => e.User)
                .HasMaxLength(80)
                .HasColumnName("USER");
            entity.Property(e => e.Value)
                .HasMaxLength(64)
                .HasColumnName("VALUE");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => new { e.Mac, e.PageId }).HasName("PK__images__E6B53E07E35A9E57");

            entity.ToTable("images");

            entity.Property(e => e.Mac)
                .HasMaxLength(16)
                .HasColumnName("MAC");
            entity.Property(e => e.PageId).HasColumnName("PAGE_ID");
            entity.Property(e => e.Button)
                .HasMaxLength(1024)
                .HasColumnName("BUTTON");
            entity.Property(e => e.Image1).HasColumnName("Image");
            entity.Property(e => e.ImageId).HasColumnName("IMAGE_ID");
            entity.Property(e => e.Led)
                .HasMaxLength(1024)
                .HasColumnName("LED");
            entity.Property(e => e.Md5)
                .HasMaxLength(32)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MD5");
            entity.Property(e => e.Nfc)
                .HasMaxLength(1024)
                .HasColumnName("NFC");
            entity.Property(e => e.Status).HasColumnName("STATUS");
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

        modelBuilder.Entity<Labelstatus>(entity =>
        {
            entity.HasKey(e => e.Mac).HasName("PK__labelsta__C790778DB2611BE4");

            entity.ToTable("labelstatus");

            entity.Property(e => e.Mac)
                .HasMaxLength(16)
                .HasColumnName("MAC");
            entity.Property(e => e.BaseStation)
                .HasMaxLength(25)
                .HasColumnName("BASE_STATION");
            entity.Property(e => e.BatteryStatus).HasColumnName("BATTERY_STATUS");
            entity.Property(e => e.BatteryVoltage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("BATTERY_VOLTAGE");
            entity.Property(e => e.BootCount).HasColumnName("BOOT_COUNT");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.DisplayOptions).HasColumnName("DISPLAY_OPTIONS");
            entity.Property(e => e.FirmwareStatus).HasColumnName("FIRMWARE_STATUS");
            entity.Property(e => e.FirmwareSubversion)
                .HasMaxLength(10)
                .HasColumnName("FIRMWARE_SUBVERSION");
            entity.Property(e => e.FirmwareVersion)
                .HasMaxLength(10)
                .HasColumnName("FIRMWARE_VERSION");
            entity.Property(e => e.Group)
                .HasMaxLength(50)
                .HasColumnName("GROUP");
            entity.Property(e => e.Id)
                .HasMaxLength(40)
                .HasColumnName("ID");
            entity.Property(e => e.ImageFile)
                .HasMaxLength(80)
                .HasColumnName("IMAGE_FILE");
            entity.Property(e => e.ImageId).HasColumnName("IMAGE_ID");
            entity.Property(e => e.ImageIdLocal).HasColumnName("IMAGE_ID_LOCAL");
            entity.Property(e => e.Lanid)
                .HasMaxLength(4)
                .HasColumnName("LANID");
            entity.Property(e => e.LastImage)
                .HasColumnType("datetime")
                .HasColumnName("LAST_IMAGE");
            entity.Property(e => e.LastInfo)
                .HasColumnType("datetime")
                .HasColumnName("LAST_INFO");
            entity.Property(e => e.LastPoll)
                .HasColumnType("datetime")
                .HasColumnName("LAST_POLL");
            entity.Property(e => e.Lqi).HasColumnName("LQI");
            entity.Property(e => e.LqiRx).HasColumnName("LQI_RX");
            entity.Property(e => e.Panid)
                .HasMaxLength(4)
                .HasColumnName("PANID");
            entity.Property(e => e.PollInterval).HasColumnName("POLL_INTERVAL");
            entity.Property(e => e.PollTimeout).HasColumnName("POLL_TIMEOUT");
            entity.Property(e => e.ScanChannels).HasColumnName("SCAN_CHANNELS");
            entity.Property(e => e.ScanInterval).HasColumnName("SCAN_INTERVAL");
            entity.Property(e => e.Status).HasColumnName("STATUS");
            entity.Property(e => e.Temperature).HasColumnName("TEMPERATURE");
            entity.Property(e => e.Variant)
                .HasMaxLength(16)
                .HasColumnName("VARIANT");
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(e => e.Mac).HasName("PK__links__C790778DD7C52984");

            entity.ToTable("links", tb =>
                {
                    tb.HasTrigger("links_staging_delete");
                    tb.HasTrigger("links_staging_insert");
                    tb.HasTrigger("links_staging_updatedropold");
                    tb.HasTrigger("links_staging_updateinsertnew");
                });

            entity.Property(e => e.Mac)
                .HasMaxLength(16)
                .HasColumnName("MAC");
            entity.Property(e => e.Id)
                .HasMaxLength(40)
                .HasColumnName("ID");
            entity.Property(e => e.Variant).HasMaxLength(16);
        });

        modelBuilder.Entity<LinksStaging>(entity =>
        {
            entity.HasKey(e => e.Mac).HasName("PK__links_st__C790778DA897A4CA");

            entity.ToTable("links_staging");

            entity.Property(e => e.Mac)
                .HasMaxLength(16)
                .HasColumnName("MAC");
            entity.Property(e => e.Delete)
                .HasMaxLength(1)
                .HasColumnName("DELETE");
            entity.Property(e => e.Id)
                .HasMaxLength(40)
                .HasColumnName("ID");
            entity.Property(e => e.Variant).HasMaxLength(16);
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

        modelBuilder.Entity<NhanVienNew>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NhanVien__3214EC27ABA25E69");

            entity.ToTable("NhanVien_New");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3214EC27D09B26C0");

            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("MESSAGE");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("TIME");
            entity.Property(e => e.Type)
                .HasMaxLength(500)
                .HasColumnName("TYPE");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Role1).HasName("PK__roles__44C28DB95A5475D7");

            entity.ToTable("roles");

            entity.Property(e => e.Role1)
                .HasMaxLength(64)
                .HasColumnName("ROLE");
            entity.Property(e => e.ChangeEbssettings).HasColumnName("ChangeEBSSettings");
            entity.Property(e => e.ConnectEbs).HasColumnName("ConnectEBS");
            entity.Property(e => e.DefaultEslimage).HasColumnName("DefaultESLImage");
            entity.Property(e => e.DeleteEsl).HasColumnName("DeleteESL");
            entity.Property(e => e.DisconnectEbs).HasColumnName("DisconnectEBS");
            entity.Property(e => e.EbsenergyScan).HasColumnName("EBSEnergyScan");
            entity.Property(e => e.EbsfirmwareUpdate).HasColumnName("EBSFirmwareUpdate");
            entity.Property(e => e.EbsloadBalancing).HasColumnName("EBSLoadBalancing");
            entity.Property(e => e.EslalterNfcContent).HasColumnName("ESLAlterNfcContent");
            entity.Property(e => e.EslbuzzerLedCommands).HasColumnName("ESLBuzzerLedCommands");
            entity.Property(e => e.EslchangePollSettings).HasColumnName("ESLChangePollSettings");
            entity.Property(e => e.Esldeactivate).HasColumnName("ESLDeactivate");
            entity.Property(e => e.EslfactoryActions).HasColumnName("ESLFactoryActions");
            entity.Property(e => e.EslfirmwareUpdate).HasColumnName("ESLFirmwareUpdate");
            entity.Property(e => e.Esllan).HasColumnName("ESLLan");
            entity.Property(e => e.EslmoveEbs).HasColumnName("ESLMoveEBS");
            entity.Property(e => e.EslsetDisplayOptions).HasColumnName("ESLSetDisplayOptions");
            entity.Property(e => e.EslsetScanChannels).HasColumnName("ESLSetScanChannels");
            entity.Property(e => e.FactoryDefaultEsl).HasColumnName("FactoryDefaultESL");
            entity.Property(e => e.Hierarchy).HasColumnName("HIERARCHY");
            entity.Property(e => e.LinkEsl).HasColumnName("LinkESL");
            entity.Property(e => e.LinkForceEsl).HasColumnName("LinkForceESL");
            entity.Property(e => e.ReactivateEsl).HasColumnName("ReactivateESL");
            entity.Property(e => e.RefreshResendEslimage).HasColumnName("RefreshResendESLImage");
            entity.Property(e => e.RequestEslsettings).HasColumnName("RequestESLSettings");
            entity.Property(e => e.ResetEsl).HasColumnName("ResetESL");
            entity.Property(e => e.SshsystemSettings).HasColumnName("SSHSystemSettings");
            entity.Property(e => e.StoreEslimage).HasColumnName("StoreESLImage");
            entity.Property(e => e.UnlinkEsl).HasColumnName("UnlinkESL");
            entity.Property(e => e.ViewEbstab).HasColumnName("ViewEBSTab");
            entity.Property(e => e.ViewEsltab).HasColumnName("ViewESLTab");
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

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.HasKey(e => e.Iditem).HasName("PK__tblProdu__C9778A10A2652C70");

            entity.ToTable("tblProduct", tb =>
                {
                    tb.HasTrigger("tblProduct_staging_delete");
                    tb.HasTrigger("tblProduct_staging_insert");
                    tb.HasTrigger("tblProduct_staging_updatedropold");
                    tb.HasTrigger("tblProduct_staging_updateinsertnew");
                });

            entity.Property(e => e.Iditem)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("IDItem");
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.ChungLoaiSp)
                .HasMaxLength(50)
                .HasColumnName("ChungLoaiSP");
            entity.Property(e => e.ExpiryDate).HasColumnType("date");
            entity.Property(e => e.ExpiryDateWarning)
                .HasColumnType("date")
                .HasColumnName("ExpiryDate_Warning");
            entity.Property(e => e.HanSuDung).HasMaxLength(50);
            entity.Property(e => e.HeThong).HasMaxLength(50);
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(150);
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.Line).HasMaxLength(50);
            entity.Property(e => e.LotNo).HasMaxLength(50);
            entity.Property(e => e.MaCd)
                .HasMaxLength(50)
                .HasColumnName("MaCD");
            entity.Property(e => e.MoTa).HasMaxLength(50);
            entity.Property(e => e.NguoiThaoTac).HasMaxLength(80);
            entity.Property(e => e.Qrcode)
                .HasMaxLength(150)
                .HasColumnName("QRCode");
            entity.Property(e => e.QtyCdsau).HasColumnName("Qty_CDSau");
            entity.Property(e => e.QtyFloat).HasColumnName("Qty_Float");
            entity.Property(e => e.QtyNg).HasColumnName("Qty_NG");
            entity.Property(e => e.QtyOk).HasColumnName("Qty_OK");
            entity.Property(e => e.QtyPlan).HasColumnName("Qty_Plan");
            entity.Property(e => e.RBit1).HasColumnName("R_bit1");
            entity.Property(e => e.RBit2).HasColumnName("R_bit2");
            entity.Property(e => e.RBit3).HasColumnName("R_bit3");
            entity.Property(e => e.RDatetime1)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime1");
            entity.Property(e => e.RDatetime2)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime2");
            entity.Property(e => e.RDatetime3)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime3");
            entity.Property(e => e.RFloat1).HasColumnName("R_float1");
            entity.Property(e => e.RFloat2).HasColumnName("R_float2");
            entity.Property(e => e.RFloat3).HasColumnName("R_float3");
            entity.Property(e => e.RImg1).HasColumnName("R_img1");
            entity.Property(e => e.RImg2).HasColumnName("R_img2");
            entity.Property(e => e.RInt1).HasColumnName("R_int1");
            entity.Property(e => e.RInt2).HasColumnName("R_int2");
            entity.Property(e => e.RInt3).HasColumnName("R_int3");
            entity.Property(e => e.Remark).HasMaxLength(50);
            entity.Property(e => e.Remark1).HasMaxLength(50);
            entity.Property(e => e.Remark2).HasMaxLength(50);
            entity.Property(e => e.Remark3).HasMaxLength(50);
            entity.Property(e => e.Remark4).HasMaxLength(50);
            entity.Property(e => e.Remark5).HasMaxLength(50);
            entity.Property(e => e.SoMeSx)
                .HasMaxLength(50)
                .HasColumnName("SoMeSX");
            entity.Property(e => e.SoThung).HasMaxLength(50);
            entity.Property(e => e.StatusHold)
                .HasMaxLength(10)
                .HasColumnName("StatusHOLD");
            entity.Property(e => e.StatusNg)
                .HasMaxLength(50)
                .HasColumnName("StatusNG");
            entity.Property(e => e.StatusPassed)
                .HasMaxLength(10)
                .HasColumnName("StatusPASSED");
            entity.Property(e => e.StatusUnderqa)
                .HasMaxLength(10)
                .HasColumnName("StatusUNDERQA");
            entity.Property(e => e.Template).HasMaxLength(50);
            entity.Property(e => e.TenCd)
                .HasMaxLength(50)
                .HasColumnName("TenCD");
            entity.Property(e => e.TenSp)
                .HasMaxLength(50)
                .HasColumnName("TenSP");
            entity.Property(e => e.TonChuaSd).HasColumnName("TonChuaSD");
            entity.Property(e => e.TrangThaiCd)
                .HasMaxLength(50)
                .HasColumnName("TrangThaiCD");
            entity.Property(e => e.TrangThaiSp)
                .HasMaxLength(50)
                .HasColumnName("TrangThaiSP");
            entity.Property(e => e.ViTri).HasMaxLength(50);
            entity.Property(e => e.WorkOder).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProductStaging>(entity =>
        {
            entity.HasKey(e => e.Iditem).HasName("primary_key");

            entity.ToTable("tblProduct_staging");

            entity.Property(e => e.Iditem)
                .ValueGeneratedNever()
                .HasColumnName("IDItem");
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.ChungLoaiSp)
                .HasMaxLength(50)
                .HasColumnName("ChungLoaiSP");
            entity.Property(e => e.Delete)
                .HasMaxLength(1)
                .HasColumnName("DELETE");
            entity.Property(e => e.ExpiryDate).HasColumnType("date");
            entity.Property(e => e.ExpiryDateWarning)
                .HasColumnType("date")
                .HasColumnName("ExpiryDate_Warning");
            entity.Property(e => e.HanSuDung).HasMaxLength(50);
            entity.Property(e => e.HeThong).HasMaxLength(50);
            entity.Property(e => e.Image).HasMaxLength(50);
            entity.Property(e => e.ImagePath).HasMaxLength(150);
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.Line).HasMaxLength(50);
            entity.Property(e => e.LotNo).HasMaxLength(50);
            entity.Property(e => e.MaCd)
                .HasMaxLength(50)
                .HasColumnName("MaCD");
            entity.Property(e => e.MoTa).HasMaxLength(50);
            entity.Property(e => e.NguoiThaoTac).HasMaxLength(80);
            entity.Property(e => e.Qrcode)
                .HasMaxLength(150)
                .HasColumnName("QRCode");
            entity.Property(e => e.QtyCdsau).HasColumnName("Qty_CDSau");
            entity.Property(e => e.QtyFloat).HasColumnName("Qty_Float");
            entity.Property(e => e.QtyNg).HasColumnName("Qty_NG");
            entity.Property(e => e.QtyOk).HasColumnName("Qty_OK");
            entity.Property(e => e.QtyPlan).HasColumnName("Qty_Plan");
            entity.Property(e => e.RBit1).HasColumnName("R_bit1");
            entity.Property(e => e.RBit2).HasColumnName("R_bit2");
            entity.Property(e => e.RBit3).HasColumnName("R_bit3");
            entity.Property(e => e.RDatetime1)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime1");
            entity.Property(e => e.RDatetime2)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime2");
            entity.Property(e => e.RDatetime3)
                .HasColumnType("datetime")
                .HasColumnName("R_datetime3");
            entity.Property(e => e.RFloat1).HasColumnName("R_float1");
            entity.Property(e => e.RFloat2).HasColumnName("R_float2");
            entity.Property(e => e.RFloat3).HasColumnName("R_float3");
            entity.Property(e => e.RImg1).HasColumnName("R_img1");
            entity.Property(e => e.RImg2).HasColumnName("R_img2");
            entity.Property(e => e.RInt1).HasColumnName("R_int1");
            entity.Property(e => e.RInt2).HasColumnName("R_int2");
            entity.Property(e => e.RInt3).HasColumnName("R_int3");
            entity.Property(e => e.Remark).HasMaxLength(50);
            entity.Property(e => e.Remark1).HasMaxLength(50);
            entity.Property(e => e.Remark2).HasMaxLength(50);
            entity.Property(e => e.Remark3).HasMaxLength(50);
            entity.Property(e => e.Remark4).HasMaxLength(50);
            entity.Property(e => e.Remark5).HasMaxLength(50);
            entity.Property(e => e.SoMeSx)
                .HasMaxLength(50)
                .HasColumnName("SoMeSX");
            entity.Property(e => e.SoThung).HasMaxLength(50);
            entity.Property(e => e.StatusHold)
                .HasMaxLength(10)
                .HasColumnName("StatusHOLD");
            entity.Property(e => e.StatusNg)
                .HasMaxLength(50)
                .HasColumnName("StatusNG");
            entity.Property(e => e.StatusPassed)
                .HasMaxLength(10)
                .HasColumnName("StatusPASSED");
            entity.Property(e => e.StatusUnderqa)
                .HasMaxLength(10)
                .HasColumnName("StatusUNDERQA");
            entity.Property(e => e.Template).HasMaxLength(50);
            entity.Property(e => e.TenCd)
                .HasMaxLength(50)
                .HasColumnName("TenCD");
            entity.Property(e => e.TenSp)
                .HasMaxLength(50)
                .HasColumnName("TenSP");
            entity.Property(e => e.TonChuaSd).HasColumnName("TonChuaSD");
            entity.Property(e => e.TrangThaiCd)
                .HasMaxLength(50)
                .HasColumnName("TrangThaiCD");
            entity.Property(e => e.TrangThaiSp)
                .HasMaxLength(50)
                .HasColumnName("TrangThaiSP");
            entity.Property(e => e.ViTri).HasMaxLength(50);
            entity.Property(e => e.WorkOder).HasMaxLength(50);
        });

        modelBuilder.Entity<Unmarkednotification>(entity =>
        {
            entity.HasKey(e => new { e.Notificationid, e.User }).HasName("PK__unmarked__105F65F1F4AB21E1");

            entity.ToTable("unmarkednotifications");

            entity.Property(e => e.Notificationid).HasColumnName("NOTIFICATIONID");
            entity.Property(e => e.User)
                .HasMaxLength(64)
                .HasColumnName("USER");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.User1).HasName("PK__users__AA65E05ED1C22CA0");

            entity.ToTable("users");

            entity.Property(e => e.User1)
                .HasMaxLength(64)
                .HasColumnName("USER");
            entity.Property(e => e.Apikeyhash)
                .HasMaxLength(95)
                .HasColumnName("APIKEYHASH");
            entity.Property(e => e.Autoupdate).HasColumnName("AUTOUPDATE");
            entity.Property(e => e.Colorblind).HasColumnName("COLORBLIND");
            entity.Property(e => e.Img).HasColumnName("IMG");
            entity.Property(e => e.Language)
                .HasMaxLength(64)
                .HasColumnName("LANGUAGE");
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Rights)
                .HasMaxLength(16)
                .HasColumnName("RIGHTS");
            entity.Property(e => e.Role)
                .HasMaxLength(64)
                .HasColumnName("ROLE");
            entity.Property(e => e.Salt)
                .HasMaxLength(40)
                .HasColumnName("SALT");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
