using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _context;

        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members
                .Include(m => m.BorrowRecords)
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _context.Members
                .Include(m => m.BorrowRecords)
                    .ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ServiceResult<Member>> CreateMemberAsync(Member member)
        {
            // Validation: Check if email already exists
            var emailExists = await _context.Members
                .AnyAsync(m => m.Email == member.Email);

            if (emailExists)
            {
                return ServiceResult<Member>.FailureResult(
                    "Email already exists",
                    new List<string> { "A member with this email already exists" }
                );
            }

            // Generate membership number
            member.MembershipNumber = await GenerateMembershipNumber();

            // Set membership expiry (1 year from now)
            member.MembershipExpiry = DateTime.Now.AddYears(1);

            // Set max books based on membership type
            member.MaxBooksAllowed = member.MembershipType switch
            {
                "Premium" => 5,
                "Student" => 4,
                _ => 3 // Regular
            };

            try
            {
                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                return ServiceResult<Member>.SuccessResult(
                    member,
                    "Member registered successfully"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<Member>.FailureResult(
                    "Failed to register member",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ServiceResult<Member>> UpdateMemberAsync(int id, Member member)
        {
            var existingMember = await _context.Members.FindAsync(id);

            if (existingMember == null)
            {
                return ServiceResult<Member>.FailureResult(
                    "Member not found",
                    new List<string> { $"Member with ID {id} does not exist" }
                );
            }

            // Check email change
            if (existingMember.Email != member.Email)
            {
                var emailExists = await _context.Members
                    .AnyAsync(m => m.Email == member.Email && m.Id != id);

                if (emailExists)
                {
                    return ServiceResult<Member>.FailureResult(
                        "Email already in use",
                        new List<string> { "Another member is using this email" }
                    );
                }
            }

            // Update properties
            existingMember.FirstName = member.FirstName;
            existingMember.LastName = member.LastName;
            existingMember.Email = member.Email;
            existingMember.PhoneNumber = member.PhoneNumber;
            existingMember.Address = member.Address;
            existingMember.MembershipType = member.MembershipType;
            existingMember.IsActive = member.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return ServiceResult<Member>.SuccessResult(
                    existingMember,
                    "Member updated successfully"
                );
            }
            catch (Exception ex)
            {
                return ServiceResult<Member>.FailureResult(
                    "Failed to update member",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<ServiceResult> DeleteMemberAsync(int id)
        {
            var member = await _context.Members
                .Include(m => m.BorrowRecords)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return ServiceResult.FailureResult(
                    "Member not found",
                    new List<string> { $"Member with ID {id} does not exist" }
                );
            }

            // Check for active borrows
            var hasActiveBorrows = member.BorrowRecords
                .Any(br => br.ReturnDate == null);

            if (hasActiveBorrows)
            {
                return ServiceResult.FailureResult(
                    "Cannot delete member",
                    new List<string> { "Member has active book borrows. Return all books first." }
                );
            }

            try
            {
                // Clean up any historical borrow records to satisfy SQL foreign key constraints
                if (member.BorrowRecords != null && member.BorrowRecords.Any())
                {
                    _context.BorrowRecords.RemoveRange(member.BorrowRecords);
                }

                _context.Members.Remove(member);
                await _context.SaveChangesAsync();

                return ServiceResult.SuccessResult("Member deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Failed to delete member",
                    new List<string> { ex.Message }
                );
            }
        }

        public async Task<List<Member>> SearchMembersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllMembersAsync();
            }

            return await _context.Members
                .Where(m => m.FirstName.Contains(searchTerm) ||
                           m.LastName.Contains(searchTerm) ||
                           m.Email.Contains(searchTerm) ||
                           m.MembershipNumber.Contains(searchTerm))
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<List<Member>> GetActiveMembersAsync()
        {
            return await _context.Members
                .Where(m => m.IsActive && m.MembershipExpiry > DateTime.Now)
                .OrderBy(m => m.LastName)
                .ToListAsync();
        }

        public async Task<ServiceResult> RenewMembershipAsync(int memberId, int months)
        {
            var member = await _context.Members.FindAsync(memberId);

            if (member == null)
            {
                return ServiceResult.FailureResult("Member not found");
            }

            // Extend membership
            member.MembershipExpiry = member.MembershipExpiry > DateTime.Now
                ? member.MembershipExpiry.AddMonths(months)
                : DateTime.Now.AddMonths(months);

            member.IsActive = true;

            await _context.SaveChangesAsync();

            return ServiceResult.SuccessResult(
                $"Membership renewed until {member.MembershipExpiry:yyyy-MM-dd}"
            );
        }

        private async Task<string> GenerateMembershipNumber()
        {
            var year = DateTime.Now.Year;
            var count = await _context.Members.CountAsync() + 1;
            return $"MEM{year}{count:D4}";
        }
    }
}