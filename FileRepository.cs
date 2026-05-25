using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  COMPONENT: FileRepository  (Data Service)
    //  Handles all reading, writing, and ID generation for appointment records.
    //  All data is stored in a plain text file using pipe-delimited format.
    // =========================================================================
    class FileRepository
    {
        private readonly string _dataFolder = "data";
        private readonly string _dataFile;
        private AuditLogger _logger;

        public FileRepository()
        {
            _dataFile = Path.Combine(_dataFolder, "appointments.txt");
        }

        // Creates the required folders and files at startup if they are missing
        public void InitializeStorage(AuditLogger logger)
        {
            _logger = logger;
            try
            {
                if (!Directory.Exists(_dataFolder))
                {
                    Directory.CreateDirectory(_dataFolder);
                    _logger.Log("INIT", "Created 'data' folder.");
                }

                if (!File.Exists(_dataFile))
                {
                    File.WriteAllText(_dataFile, string.Empty);
                    _logger.Log("INIT", "Created appointments.txt file.");
                }

                _logger.Log("INIT", "Storage initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.Log("ERROR", $"Storage initialization failed: {ex.Message}");
                Console.WriteLine($"  [Error] Could not initialize storage: {ex.Message}");
            }
        }

        // Reads every record from the file and returns them as a list
        public List<AppointmentRecord> LoadAll()
        {
            List<AppointmentRecord> records = new List<AppointmentRecord>();
            try
            {
                if (!File.Exists(_dataFile)) return records;

                string[] lines = File.ReadAllLines(_dataFile);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    AppointmentRecord record = AppointmentRecord.FromFileLine(line);
                    if (record == null)
                    {
                        _logger.Log("ERROR", $"Malformed record skipped: {line}");
                        continue;
                    }

                    // Log a warning if the checksum does not match (possible tampering)
                    if (!ChecksumHelper.Verify(record))
                        _logger.Log("ERROR", $"Checksum mismatch for ID: {record.RecordId}");

                    records.Add(record);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("ERROR", $"Failed to load records: {ex.Message}");
                Console.WriteLine($"  [Error] Failed to load records: {ex.Message}");
            }
            return records;
        }

        // Overwrites the entire file with the current list of records
        public void SaveAll(List<AppointmentRecord> records)
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (AppointmentRecord r in records)
                    lines.Add(r.ToFileLine());
                File.WriteAllLines(_dataFile, lines);
            }
            catch (Exception ex)
            {
                _logger.Log("ERROR", $"Failed to save records: {ex.Message}");
                Console.WriteLine($"  [Error] Failed to save records: {ex.Message}");
            }
        }

        // Generates the next unique ID by finding the highest existing number
        public string GenerateId(List<AppointmentRecord> records)
        {
            int max = 0;
            foreach (AppointmentRecord r in records)
            {
                if (r.RecordId != null && r.RecordId.StartsWith("APT-"))
                {
                    string numPart = r.RecordId.Substring(4);
                    if (int.TryParse(numPart, out int num))
                        if (num > max) max = num;
                }
            }
            return "APT-" + (max + 1).ToString("D4");
        }
    }
}
