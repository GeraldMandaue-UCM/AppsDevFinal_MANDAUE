using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  PROGRAM / MENU CONTROLLER
    //  Entry point of the application.
    //  Contains the main menu loop and all menu action methods.
    // =========================================================================
    class Program
    {
        // W = inner box width. Every BL() call outputs exactly W characters.
        static readonly int W = 58;

        static FileRepository _repository;
        static AuditLogger _logger;
        static ValidationService _validator;
        static ReportGenerator _reporter;

        // ─────────────────────────────────────────────────────────────────────
        //  ENTRY POINT
        // ─────────────────────────────────────────────────────────────────────
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            _logger = new AuditLogger(Path.Combine("logs", "audit.txt"));
            _repository = new FileRepository();
            _validator = new ValidationService();
            _reporter = new ReportGenerator();

            _repository.InitializeStorage(_logger);
            _logger.Log("SYSTEM", "Application started.");
            PrintWelcome();

            bool running = true;
            while (running)
            {
                PrintMenu();
                Prompt("\n  Enter your choice");
                string choice = Console.ReadLine()?.Trim();
                Console.Clear();

                switch (choice)
                {
                    case "1": AddAppointment(); break;
                    case "2": ViewAppointments(); break;
                    case "3": SearchAppointments(); break;
                    case "4": UpdateAppointment(); break;
                    case "5": SoftDeleteAppointment(); break;
                    case "6": HardDeleteAppointment(); break;
                    case "7": _reporter.GenerateReport(_repository.LoadAll(), _logger); break;
                    case "8": ViewAuditLog(); break;
                    case "0":
                        _logger.Log("SYSTEM", "Application exited.");
                        PrintGoodbye();
                        running = false;
                        break;
                    default:
                        PrintError("Invalid choice. Please enter a number from 0 to 8.");
                        _logger.Log("ERROR", $"Invalid menu choice: '{choice}'");
                        break;
                }

                if (running)
                {
                    Console.WriteLine();
                    C("  Press any key to return to the menu...", ConsoleColor.DarkGray);
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        // =========================================================================
        //  BOX DRAWING HELPERS
        //  All box lines output exactly W characters between the border pipes.
        //  This ensures every border is perfectly aligned.
        // =========================================================================

        // Pads or truncates a string to exactly W characters
        static string Pad(string s)
        {
            if (s == null) s = "";
            return s.Length >= W ? s.Substring(0, W) : s.PadRight(W);
        }

        // Centers a string within W characters
        static string Mid(string s)
        {
            if (s == null) s = "";
            if (s.Length >= W) return s.Substring(0, W);
            int left = (W - s.Length) / 2;
            return s.PadLeft(left + s.Length).PadRight(W);
        }

        // Box border lines (top, bottom, middle divider)
        static void BoxTop() => C("  \u2554" + new string('\u2550', W) + "\u2557", ConsoleColor.Cyan, true);
        static void BoxBot() => C("  \u255A" + new string('\u2550', W) + "\u255D", ConsoleColor.Cyan, true);
        static void BoxDiv() => C("  \u2560" + new string('\u2550', W) + "\u2563", ConsoleColor.Cyan, true);

        // Box line: left-aligned content
        static void BL(string content = "", ConsoleColor color = ConsoleColor.White)
        {
            C("  \u2551", ConsoleColor.Cyan);
            C(Pad(content), color);
            C("\u2551", ConsoleColor.Cyan, true);
        }

        // Box line: centered content
        static void BLC(string content = "", ConsoleColor color = ConsoleColor.White)
        {
            C("  \u2551", ConsoleColor.Cyan);
            C(Mid(content), color);
            C("\u2551", ConsoleColor.Cyan, true);
        }

        // Box line: menu item with colored number  e.g.  |  [1]  Label...  |
        static void BLMenu(string num, string label)
        {
            // Layout: "  " (2) + "[N]" (3) + "  label..." padded to (W-5) = W total
            int space = W - 5;
            string rest = "  " + label;
            rest = rest.Length > space ? rest.Substring(0, space) : rest.PadRight(space);

            C("  \u2551", ConsoleColor.Cyan);
            C("  ", ConsoleColor.White);
            C($"[{num}]", ConsoleColor.Yellow);
            C(rest, ConsoleColor.White);
            C("\u2551", ConsoleColor.Cyan, true);
        }

        // =========================================================================
        //  COLOR / OUTPUT HELPERS
        // =========================================================================

        // Core color writer — nl=true adds a newline after the text
        static void C(string text, ConsoleColor color, bool nl = false)
        {
            Console.ForegroundColor = color;
            if (nl) Console.WriteLine(text);
            else Console.Write(text);
            Console.ResetColor();
        }

        static void PrintError(string msg)
        {
            C($"\n  [!]  {msg}", ConsoleColor.Red, true);
        }

        static void PrintSuccess(string msg)
        {
            C($"\n  [OK] {msg}", ConsoleColor.Green, true);
        }

        // Horizontal rule used between records in lists
        static void Rule()
        {
            C("  " + new string('\u2500', W + 2), ConsoleColor.DarkCyan, true);
        }

        // Yellow prompt label followed by a colon
        static void Prompt(string label)
        {
            C($"{label}: ", ConsoleColor.Yellow);
        }

        // Returns the appropriate color for each appointment status
        static ConsoleColor StatusColor(string status)
        {
            if (status == "Scheduled") return ConsoleColor.Cyan;
            if (status == "Completed") return ConsoleColor.Green;
            if (status == "Cancelled") return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        // =========================================================================
        //  WELCOME SCREEN
        // =========================================================================
        static void PrintWelcome()
        {
            Console.Clear();
            Console.WriteLine();
            BoxTop();
            BL();
            BLC("[+]  G E R A L D ' S   C L I N I C  [+]", ConsoleColor.White);
            BLC("- - - - - - - - - - - - - - - - - - - -", ConsoleColor.DarkCyan);
            BLC("Appointment Management System", ConsoleColor.DarkGray);
            BL();
            BLC("\" Your Health Is Our Priority \"", ConsoleColor.DarkGray);
            BL();
            BoxDiv();
            BL($"  Date  :  {DateTime.Now:dddd, MMMM dd, yyyy}", ConsoleColor.Yellow);
            BL($"  Time  :  {DateTime.Now:hh:mm tt}", ConsoleColor.Yellow);
            BoxBot();
            Console.WriteLine();
            C("  Welcome! Press any key to begin...", ConsoleColor.DarkGray);
            Console.ReadKey();
            Console.Clear();
        }

        // =========================================================================
        //  MAIN MENU
        // =========================================================================
        static void PrintMenu()
        {
            Console.WriteLine();
            BoxTop();
            BLC("[+]  G E R A L D ' S   C L I N I C  [+]", ConsoleColor.White);
            BLC("Appointment Management System", ConsoleColor.DarkGray);
            BoxDiv();
            BL($"  {DateTime.Now:ddd, MMM dd yyyy}   |   {DateTime.Now:hh:mm tt}", ConsoleColor.Yellow);
            BoxDiv();
            BLC("M A I N   M E N U", ConsoleColor.White);
            BoxDiv();
            BL();
            BLMenu("1", "Book New Appointment");
            BLMenu("2", "View All Appointments");
            BLMenu("3", "Search Appointments");
            BLMenu("4", "Update Appointment");
            BLMenu("5", "Cancel Appointment      (keeps record)");
            BLMenu("6", "Remove Appointment      (permanent delete)");
            BLMenu("7", "Generate Daily Report");
            BLMenu("8", "View Audit Log");
            BLMenu("0", "Exit System");
            BL();
            BoxBot();
        }

        // Section header used at the top of each action screen
        static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            BoxTop();
            BLC(title, ConsoleColor.Yellow);
            BoxBot();
            Console.WriteLine();
        }

        // =========================================================================
        //  GOODBYE SCREEN
        // =========================================================================
        static void PrintGoodbye()
        {
            Console.Clear();
            Console.WriteLine();
            BoxTop();
            BL();
            BLC("[+]  G E R A L D ' S   C L I N I C  [+]", ConsoleColor.White);
            BL();
            BoxDiv();
            BL();
            BLC("Thank you for using the Appointment System!", ConsoleColor.White);
            BLC("Stay safe and stay healthy.", ConsoleColor.Green);
            BL();
            BLC("-- Gerald's Clinic Team --", ConsoleColor.DarkGray);
            BL();
            BoxBot();
            Console.WriteLine();
        }

        // =========================================================================
        //  UTILITY: TIME & INPUT HELPERS
        // =========================================================================

        // Converts stored 24-hour time (HH:mm) to 12-hour display  e.g. 9:30 AM
        static string To12Hour(string time24)
        {
            if (DateTime.TryParseExact(time24, "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt.ToString("h:mm tt");
            return time24;
        }

        // Keeps re-asking until the user gives valid input for a text field
        static string PromptValid(string label, Func<string, bool> validate, string errorMsg)
        {
            while (true)
            {
                C($"  {label,-22}: ", ConsoleColor.White);
                string input = Console.ReadLine();
                if (validate(input)) return input.Trim();
                PrintError(errorMsg);
            }
        }

        // DATE INPUT: shows hint, accepts YYYY-MM-DD or 8 raw digits (e.g. 20260615)
        static string ReadDateInput(string label)
        {
            while (true)
            {
                C($"  {label,-22}: ", ConsoleColor.White);
                C("(YYYY-MM-DD, e.g. 2026-06-15) ", ConsoleColor.DarkGray);
                string raw = Console.ReadLine()?.Trim();

                // Auto-format 8 raw digits into YYYY-MM-DD
                if (raw != null && raw.Length == 8 && IsAllDigits(raw))
                    raw = raw.Substring(0, 4) + "-" + raw.Substring(4, 2) + "-" + raw.Substring(6, 2);

                if (_validator.IsValidDate(raw)) return raw;
                PrintError("Invalid date. Use YYYY-MM-DD (e.g. 2026-06-15) or type 8 digits like 20260615");
            }
        }

        // Checks whether a string contains only digit characters
        static bool IsAllDigits(string s)
        {
            foreach (char c in s)
                if (!char.IsDigit(c)) return false;
            return true;
        }

        // TIME INPUT: single line  e.g.  9:30 AM  or  2:15 PM
        static string ReadTimeInput(string label)
        {
            while (true)
            {
                C($"  {label,-22}: ", ConsoleColor.White);
                C("(e.g. 9:30 AM  or  2:15 PM) ", ConsoleColor.DarkGray);
                string raw = Console.ReadLine()?.Trim();
                string converted = ParseTime(raw);
                if (converted != null) return converted;
                PrintError("Invalid time. Please type like: 9:30 AM  or  2:15 PM");
            }
        }

        // Tries multiple 12-hour and 24-hour formats and returns HH:mm, or null if invalid
        static string ParseTime(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string[] formats = { "h:mm tt", "hh:mm tt", "h:mmtt", "hh:mmtt",
                                  "h tt",    "hh tt",    "HH:mm",  "H:mm"   };
            foreach (string fmt in formats)
                if (DateTime.TryParseExact(input, fmt,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    return dt.ToString("HH:mm");
            return null;
        }

        // =========================================================================
        //  1. ADD APPOINTMENT
        // =========================================================================
        static void AddAppointment()
        {
            PrintSectionHeader("BOOK NEW APPOINTMENT");
            C("  Fill in all appointment details below.\n", ConsoleColor.DarkGray);
            Rule();
            Console.WriteLine();

            string patientName = PromptValid("Patient Name", _validator.IsValidName, "Name must be 2 to 100 characters.");
            string doctorName = PromptValid("Doctor Name", _validator.IsValidName, "Name must be 2 to 100 characters.");

            Console.WriteLine();
            C("  -- Appointment Schedule --\n", ConsoleColor.Cyan);

            string date = ReadDateInput("Appointment Date");
            string time = ReadTimeInput("Appointment Time");

            Console.WriteLine();
            C("  -- Reason for Visit --\n", ConsoleColor.Cyan);

            string reason = PromptValid("Reason / Purpose", _validator.IsValidReason, "Reason must be 3 to 200 characters.");

            // Build and save the new record
            List<AppointmentRecord> records = _repository.LoadAll();

            AppointmentRecord rec = new AppointmentRecord
            {
                RecordId = _repository.GenerateId(records),
                PatientName = patientName,
                DoctorName = doctorName,
                AppointmentDate = date,
                AppointmentTime = time,
                Reason = reason,
                Status = "Scheduled",
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsActive = true
            };
            rec.Checksum = ChecksumHelper.Compute(rec);

            records.Add(rec);
            _repository.SaveAll(records);
            _logger.Log("ADD", $"ID={rec.RecordId} | Patient={rec.PatientName} | Doctor={rec.DoctorName} | Date={rec.AppointmentDate} | Time={rec.AppointmentTime}");

            Console.WriteLine();
            Rule();
            PrintSuccess("Appointment booked successfully!");
            Console.WriteLine();
            C("  Appointment ID  : ", ConsoleColor.DarkGray); C(rec.RecordId, ConsoleColor.Cyan, true);
            C("  Patient         : ", ConsoleColor.DarkGray); C(rec.PatientName, ConsoleColor.White, true);
            C("  Doctor          : ", ConsoleColor.DarkGray); C("Dr. " + rec.DoctorName, ConsoleColor.White, true);
            C("  Schedule        : ", ConsoleColor.DarkGray); C($"{rec.AppointmentDate}  at  {To12Hour(rec.AppointmentTime)}", ConsoleColor.Yellow, true);
            C("  Status          : ", ConsoleColor.DarkGray); C(rec.Status, ConsoleColor.Cyan, true);
            C("  Checksum        : ", ConsoleColor.DarkGray); C(rec.Checksum, ConsoleColor.DarkGray, true);
            Rule();
        }

        // =========================================================================
        //  2. VIEW ALL APPOINTMENTS
        // =========================================================================
        static void ViewAppointments()
        {
            PrintSectionHeader("ALL ACTIVE APPOINTMENTS");

            List<AppointmentRecord> records = _repository.LoadAll();
            int count = 0;

            C($"  {"ID",-10}  {"Patient",-20}  {"Doctor",-18}  {"Date",-12}  {"Time",-10}  Status\n", ConsoleColor.Yellow);
            Rule();

            foreach (AppointmentRecord r in records)
            {
                if (!r.IsActive) continue;

                string tamper = ChecksumHelper.Verify(r) ? "" : "  [!TAMPERED]";
                string time12 = To12Hour(r.AppointmentTime);

                C($"  {r.RecordId,-10}  {r.PatientName,-20}  {r.DoctorName,-18}  {r.AppointmentDate,-12}  {time12,-10}  ", ConsoleColor.White);
                C(r.Status + tamper, StatusColor(r.Status), true);
                C("  Reason  : ", ConsoleColor.DarkGray);
                C(r.Reason, ConsoleColor.White, true);
                Rule();
                count++;
            }

            if (count == 0)
                C("  No active appointments found.\n", ConsoleColor.DarkGray);
            else
            {
                C($"\n  Total: ", ConsoleColor.DarkGray);
                C($"{count}", ConsoleColor.Cyan);
                C(" active appointment(s).\n", ConsoleColor.DarkGray);
            }

            _logger.Log("READ", $"Viewed all appointments. Active: {count}");
        }

        // =========================================================================
        //  3. SEARCH APPOINTMENTS
        // =========================================================================
        static void SearchAppointments()
        {
            PrintSectionHeader("SEARCH APPOINTMENTS");

            BoxTop();
            BL("  Search by which field?");
            BoxDiv();
            BLMenu("1", "Patient Name");
            BLMenu("2", "Doctor Name");
            BLMenu("3", "Appointment Date   (e.g. 2026-06-15)");
            BLMenu("4", "Status   (Scheduled / Completed / Cancelled)");
            BoxBot();
            Console.WriteLine();

            Prompt("  Enter field number (1-4)");
            string choice = Console.ReadLine()?.Trim();

            if (choice != "1" && choice != "2" && choice != "3" && choice != "4")
            {
                PrintError("Invalid choice. Please enter 1, 2, 3, or 4.");
                _logger.Log("ERROR", $"Invalid search field: '{choice}'");
                return;
            }

            Prompt("  Enter search keyword   ");
            string keyword = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                PrintError("Keyword cannot be empty.");
                return;
            }

            List<AppointmentRecord> records = _repository.LoadAll();
            List<AppointmentRecord> results = new List<AppointmentRecord>();
            string lower = keyword.ToLower();

            foreach (AppointmentRecord r in records)
            {
                if (!r.IsActive) continue;
                bool match = false;
                if (choice == "1") match = r.PatientName.ToLower().Contains(lower);
                else if (choice == "2") match = r.DoctorName.ToLower().Contains(lower);
                else if (choice == "3") match = r.AppointmentDate.Contains(keyword);
                else if (choice == "4") match = r.Status.ToLower().Contains(lower);
                if (match) results.Add(r);
            }

            Console.WriteLine();
            Rule();
            C($"  ", ConsoleColor.White);
            C($"{results.Count}", ConsoleColor.Cyan);
            C($" result(s) found for \"", ConsoleColor.White);
            C(keyword, ConsoleColor.Yellow);
            C("\"\n", ConsoleColor.White);
            Rule();

            if (results.Count == 0)
            {
                C("  No matching appointments found.\n", ConsoleColor.DarkGray);
            }
            else
            {
                C($"  {"ID",-10}  {"Patient",-20}  {"Doctor",-18}  {"Date",-12}  {"Time",-10}  Status\n", ConsoleColor.Yellow);
                Rule();
                foreach (AppointmentRecord r in results)
                {
                    C($"  {r.RecordId,-10}  {r.PatientName,-20}  {r.DoctorName,-18}  {r.AppointmentDate,-12}  {To12Hour(r.AppointmentTime),-10}  ", ConsoleColor.White);
                    C(r.Status, StatusColor(r.Status), true);
                    C("  Reason  : ", ConsoleColor.DarkGray);
                    C(r.Reason, ConsoleColor.White, true);
                    Rule();
                }
            }

            _logger.Log("READ", $"Search: field={choice}, keyword='{keyword}', results={results.Count}");
        }

        // =========================================================================
        //  4. UPDATE APPOINTMENT
        // =========================================================================
        static void UpdateAppointment()
        {
            PrintSectionHeader("UPDATE APPOINTMENT");

            Prompt("  Enter Appointment ID to update");
            string id = Console.ReadLine()?.Trim().ToUpper();

            List<AppointmentRecord> records = _repository.LoadAll();
            int index = -1;

            for (int i = 0; i < records.Count; i++)
                if (records[i].RecordId.ToUpper() == id && records[i].IsActive)
                { index = i; break; }

            if (index == -1)
            {
                PrintError($"No active appointment found with ID: {id}");
                _logger.Log("ERROR", $"Update failed — ID not found: {id}");
                return;
            }

            AppointmentRecord r = records[index];

            Console.WriteLine("\n  Current Record:");
            Rule();
            C("  ID       : ", ConsoleColor.DarkGray); C(r.RecordId, ConsoleColor.Cyan, true);
            C("  Patient  : ", ConsoleColor.DarkGray); C(r.PatientName, ConsoleColor.White, true);
            C("  Doctor   : ", ConsoleColor.DarkGray); C("Dr. " + r.DoctorName, ConsoleColor.White, true);
            C("  Date     : ", ConsoleColor.DarkGray); C(r.AppointmentDate, ConsoleColor.Yellow, true);
            C("  Time     : ", ConsoleColor.DarkGray); C(To12Hour(r.AppointmentTime), ConsoleColor.Yellow, true);
            C("  Reason   : ", ConsoleColor.DarkGray); C(r.Reason, ConsoleColor.White, true);
            C("  Status   : ", ConsoleColor.DarkGray); C(r.Status, StatusColor(r.Status), true);
            Rule();
            C("  (Press Enter to keep the current value)\n\n", ConsoleColor.DarkGray);

            string input;

            C($"  New Patient Name  [{r.PatientName}]: ", ConsoleColor.Yellow);
            input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            { if (_validator.IsValidName(input)) r.PatientName = input; else PrintError("Invalid name. Keeping original."); }

            C($"  New Doctor Name   [{r.DoctorName}]: ", ConsoleColor.Yellow);
            input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            { if (_validator.IsValidName(input)) r.DoctorName = input; else PrintError("Invalid name. Keeping original."); }

            C($"  Change Date?  Current: [{r.AppointmentDate}]  (Y/N): ", ConsoleColor.Yellow);
            if (Console.ReadLine()?.Trim().ToUpper() == "Y")
                r.AppointmentDate = ReadDateInput("New Date");

            C($"  Change Time?  Current: [{To12Hour(r.AppointmentTime)}]  (Y/N): ", ConsoleColor.Yellow);
            if (Console.ReadLine()?.Trim().ToUpper() == "Y")
                r.AppointmentTime = ReadTimeInput("New Time");

            C($"  New Reason  [{r.Reason}]: ", ConsoleColor.Yellow);
            input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            { if (_validator.IsValidReason(input)) r.Reason = input; else PrintError("Invalid reason. Keeping original."); }

            C($"  New Status  [{r.Status}]  (Scheduled/Completed/Cancelled): ", ConsoleColor.Yellow);
            input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                string norm = char.ToUpper(input[0]) + input.Substring(1).ToLower();
                if (_validator.IsValidStatus(norm)) r.Status = norm;
                else PrintError("Invalid status. Keeping original.");
            }

            // Refresh timestamp and recompute checksum after any changes
            r.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            r.Checksum = ChecksumHelper.Compute(r);
            records[index] = r;
            _repository.SaveAll(records);
            _logger.Log("UPDATE", $"ID={r.RecordId} | Status={r.Status} | UpdatedAt={r.UpdatedAt}");
            PrintSuccess("Appointment updated successfully.");
        }

        // =========================================================================
        //  5. SOFT DELETE  (marks inactive — keeps record in file)
        // =========================================================================
        static void SoftDeleteAppointment()
        {
            PrintSectionHeader("CANCEL APPOINTMENT  (Soft Delete)");
            C("  The record will be marked inactive but kept in the file.\n\n", ConsoleColor.DarkGray);
            Prompt("  Enter Appointment ID to cancel");
            string id = Console.ReadLine()?.Trim().ToUpper();

            List<AppointmentRecord> records = _repository.LoadAll();
            bool found = false;

            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].RecordId.ToUpper() == id && records[i].IsActive)
                {
                    records[i].IsActive = false;
                    records[i].UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    records[i].Checksum = ChecksumHelper.Compute(records[i]);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                PrintError($"No active appointment found with ID: {id}");
                _logger.Log("ERROR", $"Soft delete failed — not found: {id}");
                return;
            }

            _repository.SaveAll(records);
            _logger.Log("DELETE", $"Soft delete: {id} marked inactive.");
            PrintSuccess($"Appointment {id} has been cancelled and marked inactive.");
        }

        // =========================================================================
        //  6. HARD DELETE  (permanent — requires typing CONFIRM)
        // =========================================================================
        static void HardDeleteAppointment()
        {
            PrintSectionHeader("REMOVE APPOINTMENT  (Permanent Delete)");
            C("  [!] WARNING: This will PERMANENTLY remove the record.\n", ConsoleColor.Red);
            C("  [!] This action CANNOT be undone.\n\n", ConsoleColor.Red);
            Prompt("  Enter Appointment ID to permanently remove");
            string id = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(id)) { PrintError("ID cannot be empty."); return; }

            C("\n  Type  CONFIRM  to proceed (or anything else to cancel): ", ConsoleColor.Yellow);
            string confirm = Console.ReadLine()?.Trim();

            if (confirm != "CONFIRM")
            {
                C("\n  Permanent delete cancelled. No changes were made.\n", ConsoleColor.DarkGray);
                _logger.Log("DELETE", $"Permanent delete cancelled: {id}");
                return;
            }

            List<AppointmentRecord> records = _repository.LoadAll();
            List<AppointmentRecord> newList = new List<AppointmentRecord>();
            bool found = false;

            foreach (AppointmentRecord r in records)
            { if (r.RecordId.ToUpper() == id) found = true; else newList.Add(r); }

            if (!found)
            {
                PrintError($"No appointment found with ID: {id}");
                _logger.Log("ERROR", $"Permanent delete failed — not found: {id}");
                return;
            }

            _repository.SaveAll(newList);
            _logger.Log("DELETE", $"Permanent delete: {id} removed.");
            PrintSuccess($"Appointment {id} has been permanently removed.");
        }

        // =========================================================================
        //  8. VIEW AUDIT LOG  (shows last 30 entries, color-coded by action type)
        // =========================================================================
        static void ViewAuditLog()
        {
            PrintSectionHeader("AUDIT LOG  —  Last 30 Entries");
            try
            {
                string logFile = Path.Combine("logs", "audit.txt");

                if (!File.Exists(logFile))
                { C("  No audit log file found.\n", ConsoleColor.DarkGray); return; }

                string[] lines = File.ReadAllLines(logFile);

                if (lines.Length == 0)
                { C("  Audit log is currently empty.\n", ConsoleColor.DarkGray); return; }

                int start = Math.Max(0, lines.Length - 30);
                for (int i = start; i < lines.Length; i++)
                {
                    string line = lines[i];
                    ConsoleColor lc = ConsoleColor.White;
                    if (line.Contains("[ERROR")) lc = ConsoleColor.Red;
                    else if (line.Contains("[ADD")) lc = ConsoleColor.Green;
                    else if (line.Contains("[DELETE")) lc = ConsoleColor.DarkYellow;
                    else if (line.Contains("[UPDATE")) lc = ConsoleColor.Yellow;
                    else if (line.Contains("[REPORT")) lc = ConsoleColor.Cyan;
                    C("  " + line, lc, true);
                }

                Console.WriteLine();
                Rule();
                C($"  Showing {lines.Length - start} of {lines.Length} total entries.\n", ConsoleColor.DarkGray);
                C($"  Log file: {Path.GetFullPath(logFile)}\n", ConsoleColor.DarkGray);
            }
            catch (Exception ex)
            {
                PrintError($"Could not read audit log: {ex.Message}");
                _logger.Log("ERROR", $"Failed to read audit log: {ex.Message}");
            }
        }
    }
}