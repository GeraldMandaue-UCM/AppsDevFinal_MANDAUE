namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  MODEL: AppointmentRecord
    //  Holds all data for one appointment and handles file serialization.
    // =========================================================================
    class AppointmentRecord
    {
        public string RecordId        { get; set; }
        public string PatientName     { get; set; }
        public string DoctorName      { get; set; }
        public string AppointmentDate { get; set; }  // stored as yyyy-MM-dd
        public string AppointmentTime { get; set; }  // stored as HH:mm (24-hour)
        public string Reason          { get; set; }
        public string Status          { get; set; }  // Scheduled | Completed | Cancelled
        public string CreatedAt       { get; set; }
        public string UpdatedAt       { get; set; }
        public bool   IsActive        { get; set; }
        public string Checksum        { get; set; }

        // Converts the record into a pipe-delimited line for file storage
        public string ToFileLine()
        {
            return string.Join("|",
                RecordId, PatientName, DoctorName,
                AppointmentDate, AppointmentTime, Reason,
                Status, CreatedAt, UpdatedAt,
                IsActive.ToString(), Checksum);
        }

        // Parses a pipe-delimited line from the file back into a record object
        public static AppointmentRecord FromFileLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            string[] parts = line.Split('|');
            if (parts.Length != 11)
                return null;

            try
            {
                return new AppointmentRecord
                {
                    RecordId        = parts[0],
                    PatientName     = parts[1],
                    DoctorName      = parts[2],
                    AppointmentDate = parts[3],
                    AppointmentTime = parts[4],
                    Reason          = parts[5],
                    Status          = parts[6],
                    CreatedAt       = parts[7],
                    UpdatedAt       = parts[8],
                    IsActive        = bool.Parse(parts[9]),
                    Checksum        = parts[10]
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
