using AutoMapper;
using MagicVilla_VillaApi.Data;
using MagicVilla_VillaApi.Dto;
using MagicVilla_VillaApi.Models;
using MagicVilla_VillaApi.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaApi.Controllers
{
    // [Route("api/[controller]")] //this is general route.
    [Route("api/VillaApi")] //this is better approach to give route give name of the controller itself.
    [ApiController]
    public class VillaApiController : ControllerBase
    {
        private readonly IVillaRepository _dbVilla; //Dependecy Injection field.
        private readonly IMapper _mapper;
        protected ApiResponse _response;
        public VillaApiController(IVillaRepository dbVilla, IMapper mapper) //Constructor Injection to use DbContext.
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._response = new();
        }
        [HttpGet] //getting data form db. 
        [ProducesResponseType(StatusCodes.Status200OK)] // this is response type of api
        public async Task<ActionResult<ApiResponse>> GetVillas()  //asyn await recommended way to use in API by microsoft.
        {
            try
            {
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaDto>>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return Ok(_response);
        }
        [HttpGet("{id:int}", Name = "GetVilla")] //here we are specifing that this method accept id as parameter.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type=typeof(villadto)] -- this is alternate 
        public async Task<ActionResult<ApiResponse>> GetVilla(int id)
        {
            try
            {

                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                _response.Result = _mapper.Map<VillaDto>(villa);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return Ok(_response);

        }

        [HttpPost] //creating new record in db.
        [ProducesResponseType(StatusCodes.Status201Created)] // we have to know before all the responses that an Api can produce and thus specify responsetype.
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse>> CreateVilla([FromBody] VillaCreateDto villaDto)
        {
            try
            {

                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == villaDto.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa already exist!");
                    return BadRequest(ModelState);
                }

                if (villaDto == null)
                {
                    return BadRequest(villaDto);
                }


                //need to manuallt convert villadto -> villa model 
                Villa model = _mapper.Map<Villa>(villaDto); // using auto mapper we lot of time as we dont need to map values manually.
                                                            //Villa model = new Villa() //converting conventional way.
                                                            //{
                                                            //    Amenity = villaDto.Amenity,
                                                            //    Details = villaDto.Details,
                                                            //    ImageUrl = villaDto.ImageUrl,
                                                            //    Name = villaDto.Name,
                                                            //    Occupancy = villaDto.Occupancy,
                                                            //    Rate = villaDto.Rate,
                                                            //    Sqft = villaDto.Sqft
                                                            //};

                //Add changes to villa model using ef core ADD METHOD.

                await _dbVilla.CreateAsync(model);

                _response.Result = _mapper.Map<VillaDto>(model);
                _response.StatusCode = HttpStatusCode.Created;
                return Ok(_response);

                return CreatedAtRoute("GetVilla", new { id = model.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpDelete] //Deleting record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in delete.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> DeleteVilla(int id) // here using IActionResult because we are not returning anything.
        {
            try
            {

                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    return BadRequest();
                }
                await _dbVilla.RemoveAsync(villa);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }
        [HttpPut("{id:int}", Name = "UpdateVilla")] //Updating record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in Update.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDto villaDto)
        {
            try
            {

                if (villaDto == null || id != villaDto.Id)
                {
                    return BadRequest();
                }
                var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false);
                //villa.Name = villaDto.Name; // we have to convert if not using ef core.
                //need to manuallt convert villadto -> villa model -- 
                Villa model = _mapper.Map<Villa>(villaDto);

                await _dbVilla.UpdateAsync(model);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in Update..
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDto> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked: false); // AsNoTracking this tells efcore not to track the id.
            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);


            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaDto, ModelState);
            Villa model = _mapper.Map<Villa>(villaDto);

            await _dbVilla.UpdateAsync(model);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }


    }
}
