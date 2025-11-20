using cmcs.Models;
using cmcs.Data;

namespace cmcs.Services
{
    public class ClaimValidationService
    {
        private readonly ApplicationDbContext _context;

        public ClaimValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<string> ValidateClaim(Claim claim)
        {
            var errors = new List<string>();

            // Check hourly rate against policy
            if (claim.RatePerHour > 800)
            {
                errors.Add("⚠️ Hourly rate exceeds maximum allowed rate of R800");
            }

            // Check total hours against policy
            if (claim.TotalHours > 160)
            {
                errors.Add("⚠️ Total hours exceed maximum allowed hours (160) per month");
            }

            // Check for unusually high amounts
            if (claim.TotalAmount > 50000)
            {
                errors.Add("⚠️ Total amount seems unusually high - please verify");
            }

            // Check for duplicate claims in same month
            var duplicateClaims = _context.Claims
                .Where(c => c.LecturerEmail == claim.LecturerEmail
                         && c.ClaimMonth.Month == claim.ClaimMonth.Month
                         && c.ClaimMonth.Year == claim.ClaimMonth.Year
                         && c.ClaimId != claim.ClaimId)
                .ToList();

            if (duplicateClaims.Any())
            {
                errors.Add("⚠️ Possible duplicate claim found for the same month");
            }

            // Check if claim month is in the future
            if (claim.ClaimMonth > DateTime.Now)
            {
                errors.Add("⚠️ Claim month cannot be in the future");
            }

            // Check if supporting documents are uploaded for large amounts
            if (claim.TotalAmount > 10000 && (!claim.SupportingDocuments?.Any() ?? true))
            {
                errors.Add("ℹ️ Supporting documents recommended for claims over R10,000");
            }

            return errors;
        }
    }
}