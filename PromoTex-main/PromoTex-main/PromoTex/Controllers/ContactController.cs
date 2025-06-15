using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoTex.Data_Access;
using PromoTex.Models;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ContactRequest>> SubmitContact(ContactRequest request)
        {
            _context.ContactRequests.Add(request);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Contact request submitted successfully." });
        }
    }
}
