using System;
using System.Collections.Generic;

namespace MPLUS_GW_WebCore.Models;

public partial class Role
{
    public string Role1 { get; set; } = null!;

    public int Hierarchy { get; set; }

    public bool? ConnectEbs { get; set; }

    public bool? DisconnectEbs { get; set; }

    public bool? EbsloadBalancing { get; set; }

    public bool? Roaming { get; set; }

    public bool? EbsfirmwareUpdate { get; set; }

    public bool? EbsenergyScan { get; set; }

    public bool? ChangeEbssettings { get; set; }

    public bool? ReactivateEsl { get; set; }

    public bool? LinkEsl { get; set; }

    public bool? LinkForceEsl { get; set; }

    public bool? UnlinkEsl { get; set; }

    public bool? UploadLinkFile { get; set; }

    public bool? RefreshResendEslimage { get; set; }

    public bool? DefaultEslimage { get; set; }

    public bool? StoreEslimage { get; set; }

    public bool? ResetEsl { get; set; }

    public bool? DeleteEsl { get; set; }

    public bool? FactoryDefaultEsl { get; set; }

    public bool? RequestEslsettings { get; set; }

    public bool? EslmoveEbs { get; set; }

    public bool? Esllan { get; set; }

    public bool? Esldeactivate { get; set; }

    public bool? EslfirmwareUpdate { get; set; }

    public bool? EslfactoryActions { get; set; }

    public bool? EslchangePollSettings { get; set; }

    public bool? EslsetDisplayOptions { get; set; }

    public bool? EslsetScanChannels { get; set; }

    public bool? EslbuzzerLedCommands { get; set; }

    public bool? EslalterNfcContent { get; set; }

    public bool? Dashboard { get; set; }

    public bool? ViewEbstab { get; set; }

    public bool? ViewEsltab { get; set; }

    public bool? ViewAccount { get; set; }

    public bool? ViewUsers { get; set; }

    public bool? ViewLogs { get; set; }

    public bool? ChangeLogs { get; set; }

    public bool? ViewLinksTable { get; set; }

    public bool? ChangeLinks { get; set; }

    public bool? ViewProducts { get; set; }

    public bool? ChangeProducts { get; set; }

    public bool? ViewTemplates { get; set; }

    public bool? ChangeTemplates { get; set; }

    public bool? ViewDatabaseSettings { get; set; }

    public bool? ChangeDatabaseSettings { get; set; }

    public bool? ViewCloudDashboardSettings { get; set; }

    public bool? ChangeCloudDashboardSettings { get; set; }

    public bool? ViewEventHandler { get; set; }

    public bool? ChangeEventHandler { get; set; }

    public bool? ViewCalendar { get; set; }

    public bool? ChangeCalendar { get; set; }

    public bool? ViewRoles { get; set; }

    public bool? ManageRoles { get; set; }

    public bool? ViewGlobalSettings { get; set; }

    public bool? ChangeGlobalSettings { get; set; }

    public bool? ViewLicense { get; set; }

    public bool? ChangeLicense { get; set; }

    public bool? ChangePasswordOwn { get; set; }

    public bool? ChangePasswordLower { get; set; }

    public bool? ChangePasswordEqual { get; set; }

    public bool? RemoveAccountOwn { get; set; }

    public bool? RemoveAccountLower { get; set; }

    public bool? RemoveAccountEqual { get; set; }

    public bool? ChangeAccountDetailsOwn { get; set; }

    public bool? ChangeAccountDetailsLower { get; set; }

    public bool? ChangeAccountDetailsEqual { get; set; }

    public bool? RegisterLower { get; set; }

    public bool? RegisterEqual { get; set; }

    public bool? ChangeRolesLower { get; set; }

    public bool? ChangeRolesEqual { get; set; }

    public bool? ImportExportBackup { get; set; }

    public bool? ImportExportTemplates { get; set; }

    public bool? SystemSettings { get; set; }

    public bool? SshsystemSettings { get; set; }

    public bool? MySqlSystemSettings { get; set; }

    public bool? FirmwareUpdateSystemSettings { get; set; }

    public bool? EslsOffline { get; set; }

    public bool? BaseStationOffline { get; set; }

    public bool? UsersWithNonexistentRoles { get; set; }

    public bool? CloudDashboardOffline { get; set; }

    public bool? CalendarSyncFailed { get; set; }
}
