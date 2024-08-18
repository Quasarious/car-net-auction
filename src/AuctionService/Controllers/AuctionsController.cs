using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _ctx;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext ctx, IMapper mapper)
        {
            _ctx = ctx;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions() {
            var auctions = await _ctx.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDTO>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDTO>> GetAuctionByID(Guid id) {
            var auction = await _ctx.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.ID == id);
            
            if (auction == null) 
                return NotFound();

            return _mapper.Map<AuctionDTO>(auction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction(CreateAuctionDTO auctionDTO) {
            var auction = _mapper.Map<Auction>(auctionDTO);
            // TODO: Add current user as seller
            auction.Seller = "test";

            await _ctx.Auctions.AddAsync(auction);

            if (await _ctx.SaveChangesAsync() > 0)
                return CreatedAtAction(nameof(GetAuctionByID), new {auction.ID}, _mapper.Map<AuctionDTO>(auction));
            
            return NotFound("Unable to save changes to db");
            
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO) {
            var auction = await _ctx.Auctions
                .Include(a => a.Item)
                .FirstOrDefaultAsync(a => a.ID == id);
            
            if (auction == null) 
                return NotFound();

            // TODO: seller name matches username

            auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDTO.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDTO.Year ?? auction.Item.Year;
            
            var result = await _ctx.SaveChangesAsync() > 0;

            if (result)
                return Ok();

            return BadRequest("Unable to save changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id) {
            var auction = await _ctx.Auctions.FindAsync(id);

            if (auction == null)
                return NotFound();
            
            // TODO: check seller == user

            _ctx.Auctions.Remove(auction);

            var result = await _ctx.SaveChangesAsync() > 0;

            if (result)
                return Ok();

            return BadRequest("Unable to delete entity");
        }

    }
}