using MagicVilla_VillaApi.Data;
using MagicVilla_VillaApi.Dto;
using MagicVilla_VillaApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaApi.Controllers
{
   // [Route("api/[controller]")] //this is general route.
    [Route("api/VillaApi")] //this is better approach to give route give name of the controller itself.
    [ApiController]
    public class VillaApiController : ControllerBase
    { 
        private readonly ApplicationDbContext _db; //Dependecy Injection field.

        public VillaApiController(ApplicationDbContext db) //Constructor Injection to use DbContext.
        {
           _db = db;
        }
        [HttpGet] //getting data form db. 
        [ProducesResponseType(StatusCodes.Status200OK)] // this is response type of api
        public async Task<ActionResult<IEnumerable<VillaDto>>>GetVillas()  //asyn await recommended way to use in API by microsoft.
        {
            return Ok(await _db.Villas.ToListAsync());
        }
        [HttpGet("{id:int}", Name="GetVilla")] //here we are specifing that this method accept id as parameter.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type=typeof(villadto)] -- this is alternate 
        public async Task <ActionResult<VillaDto>> GetVilla(int id)
        {
            if(id == 0)
            {
                return BadRequest();
            }
            var villa =await _db.Villas.FirstOrDefaultAsync(u =>u.Id == id);
            if(villa == null)
            {
                return NotFound();
            }

            return Ok(villa); //we are returning ok as response with villa object.

        }

        [HttpPost] //creating new record in db.
        [ProducesResponseType(StatusCodes.Status201Created)] // we have to know before all the responses that an Api can produce and thus specify responsetype.
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<VillaDto>> CreateVilla([FromBody] VillaCreateDto villaDto)
        {
            if(await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa already exist!");
                return BadRequest(ModelState);
            }

            if(villaDto == null)
            {
                return BadRequest(villaDto);    
            }
            

            //need to manuallt convert villadto -> villa model -- later we will use auto mapper.
            Villa model = new Villa() //converting
            {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft
            };

            //Add changes to villa model using ef core ADD METHOD.

           await _db.Villas.AddAsync(model);
           await  _db.SaveChangesAsync();

            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
        }

        [HttpDelete] //Deleting record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in delete.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVilla(int id ) // here using IActionResult because we are not returning anything.
        {
            
            if(id == 0)
            {
                return BadRequest();
            }
            var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if(villa == null)
            {
                return BadRequest();
            }
             _db.Villas.Remove(villa);
           await  _db.SaveChangesAsync();
            return NoContent();
        }
        [HttpPut("{id:int}",Name ="UpdateVilla")] //Updating record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in Update.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVilla(int id , [FromBody] VillaUpdateDto villaDto)
        {
            if(villaDto == null || id != villaDto.Id)
            {
                return BadRequest();
            }
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            //villa.Name = villaDto.Name; // we have to convert if not using ef core.
            //need to manuallt convert villadto -> villa model -- later we will use auto mapper.
            Villa model = new () //converting
            {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft
            };
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id:int}",Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in Update..
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id); // AsNoTracking this tells efcore not to track the id.

            VillaUpdateDto villaDto = new() // since we are updating from dto to model.
            {
                Amenity = villa.Amenity,
                Details = villa.Details,
                Id = villa.Id,
                ImageUrl = villa.ImageUrl,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Rate = villa.Rate,
                Sqft = villa.Sqft
            };

            if(villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaDto,ModelState);
            Villa model = new() //converting back.
            {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft
            };
            _db.Villas.Update(model);
           await _db.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }


    }
}
