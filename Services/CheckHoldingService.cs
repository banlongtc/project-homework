
using Hangfire;
using MPLUS_GW_WebCore.Models;
using Newtonsoft.Json;
using System.Linq;

namespace MPLUS_GW_WebCore.Services
{
    public class CheckHoldingService
    {
        private readonly MplusGwContext _context;

        public CheckHoldingService(MplusGwContext context)
        {
            _context = context;

        }
        public string CheckHolding()
        {
            var hasData = _context.TblImportedItems
                .Where(x => x.Status == "Holding")
                .Select(x => new
                {
                    productCode = x.ItemCode,
                    lotNo = x.LotNo,
                    qty = x.Qty,
                    idRecev = _context.TblRecevingPlmes
                    .Where(r => r.ItemCode == x.ItemCode && r.LotNo == x.LotNo && r.OrderShipment == x.OrderShipment)
                    .Select(s => s.NewId)
                    .FirstOrDefault()
                }).ToList();
            return hasData.Count > 0 ? JsonConvert.SerializeObject(hasData) : "";
        }
        public void CleanupMinuteJobs()
        {
            // Lấy tất cả các job đã lập lịch
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var jobs = monitoringApi.SucceededJobs(0, 2000);
            foreach (var job in jobs)
            {
                // Xóa job
                BackgroundJob.Delete(job.Key);
            }

        }
    }
}
