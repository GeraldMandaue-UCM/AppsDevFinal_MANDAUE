using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  COMPONENT: ChecksumHelper
    //  Computes and verifies an MD5-based checksum for each record.
    //  This detects if a record was manually tampered with in the file.
    // =========================================================================
    class ChecksumHelper
    {
        // Computes an 8-character checksum from the record's key fields
        public static string Compute(AppointmentRecord record)
        {
            string raw = string.Join("|",
                record.RecordId,
                record.PatientName,
                record.DoctorName,
                record.AppointmentDate,
                record.AppointmentTime,
                record.Reason,
                record.Status);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));

                // Use only the first 4 bytes for a short, readable checksum
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < 4; i++)
                    sb.Append(hashBytes[i].ToString("X2"));

                return sb.ToString();
            }
        }

        // Returns true if the stored checksum still matches the record's data
        public static bool Verify(AppointmentRecord record)
        {
            return record.Checksum == Compute(record);
        }
    }
}
