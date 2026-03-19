using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        // GET: api/members
        [HttpGet]
        public async Task<ActionResult<List<Member>>> GetAllMembers()
        {
            var members = await _memberService.GetAllMembersAsync();
            return Ok(members);
        }

        // GET: api/members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);

            if (member == null)
            {
                return NotFound(new { message = $"Member with ID {id} not found" });
            }

            return Ok(member);
        }

        // POST: api/members
        [HttpPost]
        public async Task<ActionResult<Member>> CreateMember(Member member)
        {
            var result = await _memberService.CreateMemberAsync(member);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetMember), new { id = result.Data.Id }, result.Data);
        }

        // PUT: api/members/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Member>> UpdateMember(int id, Member member)
        {
            var result = await _memberService.UpdateMemberAsync(id, member);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result.Data);
        }

        // DELETE: api/members/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMember(int id)
        {
            var result = await _memberService.DeleteMemberAsync(id);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return NoContent();
        }

        // GET: api/members/search?term=john
        [HttpGet("search")]
        public async Task<ActionResult<List<Member>>> SearchMembers([FromQuery] string term)
        {
            var members = await _memberService.SearchMembersAsync(term);
            return Ok(members);
        }

        // GET: api/members/active
        [HttpGet("active")]
        public async Task<ActionResult<List<Member>>> GetActiveMembers()
        {
            var members = await _memberService.GetActiveMembersAsync();
            return Ok(members);
        }

        // POST: api/members/5/renew?months=12
        [HttpPost("{id}/renew")]
        public async Task<ActionResult> RenewMembership(int id, [FromQuery] int months = 12)
        {
            var result = await _memberService.RenewMembershipAsync(id, months);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(new { message = result.Message });
        }
    }
}