using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contact.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        public ContactController(AppDbContext db)
        {
            _dbContext = db;
        }

        [HttpGet("GetContact")]
        public async Task<ActionResult> GetContact()
        {
            var response = new Models.Contact
            {
                Id = 1,
                Name = "Jhonn",
                Telephone = "9888"
            };

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var response = await _dbContext.Contact.ToListAsync();

            return Ok(response);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var contact = await _dbContext.Contact.FirstOrDefaultAsync(x => x.Id == id);

                if (contact == null)
                    return NotFound();

                return Ok(contact);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(Models.Contact model)
        {
            await _dbContext.Contact.AddAsync(model);

            await _dbContext.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut]
        public async Task<ActionResult> Update(Models.Contact model)
        {
            var contact = await _dbContext.Contact.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (contact == null)
                return NotFound();

            contact.Name = model.Name;
            contact.Telephone = model.Telephone;

            await _dbContext.SaveChangesAsync();

            return Ok(contact);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var contact = await _dbContext.Contact.FirstOrDefaultAsync(x => x.Id == id);

            if (contact == null)
                return NotFound();

            _dbContext.Contact.Remove(contact);

            await _dbContext.SaveChangesAsync();

            return Ok(contact);
        }
    }
}