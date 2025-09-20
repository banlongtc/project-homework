using MPLUS_GW_WebCore.Controllers.Processing;
using Serilog;
using System.Globalization;

namespace MPLUS_GW_WebCore
{
    public class ProductionCalculator
    {
        public static DateTime CalculateCompletionTime(DateTime startTime, double productionHours, TimeSpan ShiftStart, TimeSpan ShiftEnd, List<BreakSchedule> BreakSchedules)
        {
            DateTime currentTime = startTime;
            TimeSpan remainingWorkTime = TimeSpan.FromHours(productionHours);

            while (remainingWorkTime.TotalSeconds > 0)
            {
                DateTime startOfShiftToday = currentTime.Date.Add(ShiftStart);

                if (currentTime < startOfShiftToday)
                {
                    currentTime = startOfShiftToday;
                }
                DateTime shiftEndDateTime = ShiftEnd > ShiftStart
                    ? currentTime.Date.Add(ShiftEnd)
                    : currentTime.Date.AddDays(1).Add(ShiftEnd);

                TimeSpan totalShiftDurationToday = shiftEndDateTime - currentTime;
                TimeSpan timeForBreaksToday = TimeSpan.Zero;

                // Trừ thời gian nghỉ trong ngày
                foreach (var b in BreakSchedules)
                {
                    if (currentTime.TimeOfDay < b.EndBreakTime)
                    {
                        TimeSpan breakStart = b.StartBreakTime ?? TimeSpan.Zero;
                        TimeSpan breakEnd = b.EndBreakTime ?? TimeSpan.Zero;

                        var breakStartTime = currentTime.Date.Add(breakStart);
                        var breakEndTime = currentTime.Date.Add(breakEnd);

                        if (currentTime < breakEndTime)
                        {
                            var effectiveBreakStart = currentTime > breakStartTime ? currentTime : breakStartTime;
                            var effectiveBreakEnd = breakEndTime > currentTime.Date.Add(ShiftEnd) ? currentTime.Date.Add(ShiftEnd) : breakEndTime;

                            if (effectiveBreakEnd > effectiveBreakStart)
                            {
                                timeForBreaksToday += effectiveBreakEnd - effectiveBreakStart;
                            }
                        }
                    }
                }
                TimeSpan availableWorkTimeToday = totalShiftDurationToday - timeForBreaksToday;
                if (availableWorkTimeToday >= remainingWorkTime)
                {
                   TimeSpan timeToAdd = remainingWorkTime;

                    foreach (var b in BreakSchedules.Where(b => currentTime.TimeOfDay < b.StartBreakTime))
                    {
                        TimeSpan timeUntilBreak = b.StartBreakTime ?? TimeSpan.Zero - currentTime.TimeOfDay;

                        if (timeToAdd > timeUntilBreak)
                        {
                            currentTime = currentTime.Add(timeUntilBreak);
                            currentTime = currentTime.Add(b.Duration ?? TimeSpan.Zero);
                            timeToAdd -= timeUntilBreak;
                        }
                        else
                        {
                            currentTime = currentTime.Add(timeToAdd);
                            remainingWorkTime = TimeSpan.Zero;
                            return currentTime;
                        }
                    }

                    currentTime = currentTime.Add(timeToAdd);
                    remainingWorkTime = TimeSpan.Zero;
                }
                else
                {
                    // Chuyển sang ngày làm việc tiếp theo
                    remainingWorkTime -= availableWorkTimeToday;
                    currentTime = currentTime.Date.AddDays(1).Add(ShiftStart);
                }
            }
            return currentTime;
        }

        public static List<ProductionTimeData> ProcessProductionQueue(List<ProductionTimeData>? productionTimeData, TimeSpan shiftStartTime, TimeSpan shiftEndTime, List<BreakSchedule> breakSchedules)
        {
            var results = new List<ProductionTimeData>();
            if (productionTimeData == null)
            {
                return results;
            }

            string formatDateTime = "dd/MM/yyyy HH:mm";
            DateTime? currentStartTime = null;
            var firstRowItem = productionTimeData.FirstOrDefault();
            var firstProductionLine = firstRowItem?.ProductionLines?.FirstOrDefault();
            if (firstProductionLine != null && !string.IsNullOrEmpty(firstProductionLine.StartDate))
            {
                if (DateTime.TryParseExact(firstProductionLine.StartDate, formatDateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
                {
                    currentStartTime = parsedTime;
                }
                else
                {
                    Log.Fatal("Lỗi chuyển đổi thời gian của dòng đầu tiên.");
                }
            }

            foreach (var proTime in productionTimeData)
            {
                var newItem = new ProductionTimeData
                {
                    IndexNumber = proTime.IndexNumber,
                    WorkOrder = proTime.WorkOrder,
                    ProductCode = proTime.ProductCode,
                    LotNo = proTime.LotNo,
                    QtyWo = proTime.QtyWo,
                    Character = proTime.Character,
                    CycleTime = proTime.CycleTime,
                    ProductionLines = new List<ProductionLineTimeData>(),
                };

                foreach (var itemLineVal in proTime.ProductionLines)
                {
                    DateTime startTimeValue = currentStartTime ?? DateTime.Now;

                    DateTime completionTime = CalculateCompletionTime(startTimeValue, itemLineVal.Time, shiftStartTime, shiftEndTime, breakSchedules);
                   
                    newItem.ProductionLines.Add(new ProductionLineTimeData
                    {
                        DataLine = itemLineVal.DataLine,
                        Time = itemLineVal.Time,
                        Qty = itemLineVal.Qty,
                        StartDate = currentStartTime?.ToString(formatDateTime) ?? "",
                        EndDate = completionTime.ToString(formatDateTime) ?? "",
                        QtyInDay = CalculateProductionQuantity(shiftStartTime, shiftEndTime, breakSchedules, proTime.CycleTime),
                    });
                   
                    currentStartTime = completionTime;
                }
                results.Add(newItem);
            }
            return results;
        }

        public static int CalculateProductionQuantity(TimeSpan shiftStart, TimeSpan shiftEnd, List<BreakSchedule> breaks, double secondsPerUnit)
        {
            TimeSpan shiftDuration = shiftEnd > shiftStart
                ? shiftEnd - shiftStart
                : TimeSpan.FromHours(24) - shiftStart + shiftEnd;

            TimeSpan totalBreakTime = breaks
             .Where(b => b.Duration != null)
             .Select(b => b.Duration != null ? b.Duration.Value : TimeSpan.Zero)
             .Aggregate(TimeSpan.Zero, (a, b) => a + b);

            TimeSpan actualWorkTime = shiftDuration - totalBreakTime;

            double totalSeconds = actualWorkTime.TotalSeconds;
            return (int)(totalSeconds / secondsPerUnit);
        }
    }

    public class WorkSchedule
    {
        public int StartShiftTime { get; set; }
        public int EndShiftTime { get; set; }
    }

    public class BreakSchedule
    {
        public TimeSpan? StartBreakTime { get; set; }
        public TimeSpan? EndBreakTime { get; set; }
        public TimeSpan? Duration => EndBreakTime - StartBreakTime;
    }
}
