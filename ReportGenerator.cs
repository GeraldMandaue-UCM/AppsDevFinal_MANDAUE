using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  COMPONENT: ReportGenerator
    //  Generates a clinic report showing summary stats, today's appointments,
    //  upcoming appointments (next 7 days), overdue records, and per-doctor counts.
    //  Report is printed to the console and also saved to the reports folder.
    // =========================================================================
    class ReportGenerator
    {
        private readonly string _reportsFolder = "reports";

        public void GenerateReport(List<AppointmentRecord> records, AuditLogger logger)
        {
            try
            {
                if (!Directory.Exists(_reportsFolder))
                    Directory.CreateDirectory(_reportsFolder);

                string today      = DateTime.Now.ToString("yyyy-MM-dd");
                string reportFile = Path.Combine(_reportsFolder, $"report_{today}.txt");
                DateTime todayDate = DateTime.Today;

                // Counters and grouped lists
                int totalActive = 0, scheduled = 0, completed = 0, cancelled = 0, softDeleted = 0;
                Dictionary<string, int> byDoctor     = new Dictionary<string, int>();
                List<AppointmentRecord> todayList    = new List<AppointmentRecord>();
                List<AppointmentRecord> upcomingList = new List<AppointmentRecord>();
                List<AppointmentRecord> overdueList  = new List<AppointmentRecord>();

                foreach (AppointmentRecord r in records)
                {
                    if (!r.IsActive) { softDeleted++; continue; }

                    totalActive++;
                    if      (r.Status == "Scheduled")  scheduled++;
                    else if (r.Status == "Completed")  completed++;
                    else if (r.Status == "Cancelled")  cancelled++;

                    if (!byDoctor.ContainsKey(r.DoctorName))
                        byDoctor[r.DoctorName] = 0;
                    byDoctor[r.DoctorName]++;

                    if (DateTime.TryParse(r.AppointmentDate, out DateTime apptDate))
                    {
                        if (apptDate.Date == todayDate)
                            todayList.Add(r);
                        else if (apptDate.Date > todayDate && apptDate.Date <= todayDate.AddDays(7))
                            upcomingList.Add(r);
                        else if (apptDate.Date < todayDate && r.Status == "Scheduled")
                            overdueList.Add(r);
                    }
                }

                // Build report lines
                List<string> lines = new List<string>();

                lines.Add("  +------------------------------------------------------+");
                lines.Add("  |           ✚  GERALD'S CLINIC  ✚                     |");
                lines.Add("  |          Daily Appointment Report                    |");
                lines.Add($"  |  Generated : {DateTime.Now:yyyy-MM-dd  hh:mm tt}                     |");
                lines.Add("  +------------------------------------------------------+");
                lines.Add("");
                lines.Add("  --- SUMMARY -------------------------------------------");
                lines.Add($"  Total Active Records       : {totalActive}");
                lines.Add($"  Scheduled                  : {scheduled}");
                lines.Add($"  Completed                  : {completed}");
                lines.Add($"  Cancelled                  : {cancelled}");
                lines.Add($"  Soft-Deleted (Inactive)    : {softDeleted}");
                lines.Add("");
                lines.Add($"  --- TODAY'S APPOINTMENTS  ({today}) -----------------");

                if (todayList.Count == 0)
                {
                    lines.Add("  No appointments scheduled for today.");
                }
                else
                {
                    foreach (AppointmentRecord r in todayList)
                    {
                        string time12 = To12Hour(r.AppointmentTime);
                        lines.Add($"  [{r.RecordId}]  {time12,-10}  {r.PatientName}  with Dr. {r.DoctorName}  |  {r.Status}");
                        lines.Add($"           Reason: {r.Reason}");
                    }
                }

                lines.Add("");
                lines.Add("  --- UPCOMING APPOINTMENTS  (Next 7 Days) ---------------");

                if (upcomingList.Count == 0)
                {
                    lines.Add("  No upcoming appointments in the next 7 days.");
                }
                else
                {
                    foreach (AppointmentRecord r in upcomingList)
                    {
                        string time12 = To12Hour(r.AppointmentTime);
                        lines.Add($"  [{r.RecordId}]  {r.AppointmentDate}  {time12,-10}  {r.PatientName}  with Dr. {r.DoctorName}");
                        lines.Add($"           Reason: {r.Reason}");
                    }
                }

                lines.Add("");
                lines.Add("  --- OVERDUE: Past Date, Still 'Scheduled' --------------");

                if (overdueList.Count == 0)
                {
                    lines.Add("  No overdue appointments.");
                }
                else
                {
                    foreach (AppointmentRecord r in overdueList)
                        lines.Add($"  [!] [{r.RecordId}]  {r.AppointmentDate}  {r.PatientName}  with Dr. {r.DoctorName}");
                }

                lines.Add("");
                lines.Add("  --- APPOINTMENTS PER DOCTOR ----------------------------");

                if (byDoctor.Count == 0)
                {
                    lines.Add("  No doctor records found.");
                }
                else
                {
                    foreach (KeyValuePair<string, int> entry in byDoctor)
                        lines.Add($"  Dr. {entry.Key,-35}  :  {entry.Value} appointment(s)");
                }

                lines.Add("");
                lines.Add("  +------------------------------------------------------+");
                lines.Add("  |                   END OF REPORT                      |");
                lines.Add("  +------------------------------------------------------+");

                // Print to console
                Console.WriteLine();
                foreach (string line in lines)
                    Console.WriteLine(line);

                // Save to file
                File.WriteAllLines(reportFile, lines);
                Console.WriteLine($"\n  [Report saved to: {reportFile}]");
                logger.Log("REPORT", $"Report generated: {reportFile}");
            }
            catch (Exception ex)
            {
                logger.Log("ERROR", $"Report generation failed: {ex.Message}");
                Console.WriteLine($"  [Error] Could not generate report: {ex.Message}");
            }
        }

        // Converts stored 24-hour time (HH:mm) to 12-hour display (e.g. 9:30 AM)
        private string To12Hour(string time24)
        {
            if (DateTime.TryParseExact(time24, "HH:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dt))
                return dt.ToString("h:mm tt");
            return time24;
        }
    }
}
