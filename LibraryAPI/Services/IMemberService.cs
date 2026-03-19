using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member> GetMemberByIdAsync(int id);
        Task<ServiceResult<Member>> CreateMemberAsync(Member member);
        Task<ServiceResult<Member>> UpdateMemberAsync(int id, Member member);
        Task<ServiceResult> DeleteMemberAsync(int id);
        Task<List<Member>> SearchMembersAsync(string searchTerm);
        Task<List<Member>> GetActiveMembersAsync();
        Task<ServiceResult> RenewMembershipAsync(int memberId, int months);
    }
}