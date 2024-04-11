using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InspirinngQuotes.Data;
using InspirinngQuotes.Models;

namespace InspirinngQuotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Quotes
        [HttpGet]
        [Route("AllQuotes")]
        public async Task<ActionResult<List<QuoteModel>>> GetQuotes()
        {
            var quotes = new List<QuoteModel>();
            try
            {
            quotes = await _context.QuoteModel.ToListAsync();
            } catch (Exception ex)
            {
                
            }

            return Ok(quotes);
        }

        // GET: api/Quotes/5
        [Route("GetQuotesbyId/{id}")]
        [HttpGet]
        public async Task<ActionResult<QuoteModel>> GetQuote(int id)
        {
            var quote = await _context.QuoteModel.FindAsync(id);

            if (quote == null)
            {
                return NotFound();
            }

            return quote;
        }

        // POST: api/Quotes
        [HttpPost]
        [Route("CreateQuotesbyId/{id}")]
        public async Task<ActionResult<QuoteModel>> AddQuote(QuoteModel quote)
        {
            _context.QuoteModel.Add(quote); 
            await _context.SaveChangesAsync();

            // Custom response message
            var responseMessage = $"Author details for {quote.Author} added successfully.";

            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, new { Message = responseMessage, Quote = quote });
        }

        // PUT: api/Quotes/5
        [HttpPut]
        [Route("UpdateQuotesbyId/{id}")]
        public async Task<IActionResult> UpdateQuote(int id, QuoteModel quote)
        {
            if (id != quote.Id)
            {
                return BadRequest();
            }

            _context.Entry(quote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var updatedQuote = await _context.QuoteModel.FindAsync(id);
            string updatedAuthorName = updatedQuote.Author;

            var responseMessage = $"Author {updatedAuthorName} updated successfully.";

            return Ok(new { Message = responseMessage });
        }

        // DELETE: api/Quotes/5
        [HttpDelete]
        [Route("DeleteQuotesbyId/{id}")]
        public async Task<IActionResult> DeleteQuote(int id)
        {
            var quote = await _context.QuoteModel.FindAsync(id);
            if (quote == null)
            {
                return NotFound();
            }

            string authorName = quote.Author; 

            _context.QuoteModel.Remove(quote);
            await _context.SaveChangesAsync();

            var responseMessage = $"Author {authorName} deleted successfully.";

            return Ok(new { Message = responseMessage });
        }

        private bool QuoteExists(int id)
        {
            return _context.QuoteModel.Any(e => e.Id == id);
        }

        [HttpGet]
        [Route("SearchQuotes")]
        public ActionResult<IEnumerable<QuoteModel>> SearchQuotes(string author = null, [FromQuery] List<string> tags = null, [FromQuery] List<string> quote = null)
        {
            IQueryable<QuoteModel> query = _context.QuoteModel;

            if (!string.IsNullOrEmpty(author))
            {
                string lowercaseAuthor = author.ToLower();
                query = query.Where(q => q.Author.ToLower().Contains(lowercaseAuthor));
            }

            if (tags != null && tags.Any())
            {
                query = query.Where(q => tags.All(t => q.Tags.Contains(t)));
            }

            if (quote != null && quote.Any())
            {
                List<string> lowercaseQuotes = quote.Select(q => q.ToLower()).ToList();
                query = query.Where(q => q.QuoteText.Any(qt => lowercaseQuotes.Any(lq => qt.ToLower().Contains(lq))));
            }
             
            var matchedQuotes = query.ToList();
            return Ok(matchedQuotes);
        }
    }
}
