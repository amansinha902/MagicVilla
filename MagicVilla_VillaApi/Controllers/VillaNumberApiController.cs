using AutoMapper;
using MagicVilla_VillaApi.Dto;
using MagicVilla_VillaApi.Models;
using MagicVilla_VillaApi.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_VillaApi.Controllers
{
    // [Route("api/[controller]")] //this is general route.
    [Route("api/VillaNumberApi")] //this is better approach to give route give name of the controller itself.
    [ApiController]
    public class VillaNumberApiController : ControllerBase
    {
        private readonly IVillaNumberRepository _dbVillaNumber; //Dependecy Injection field.
        private readonly IMapper _mapper;
        protected ApiResponse _response;
        private readonly IVillaRepository _dbvilla;
        public VillaNumberApiController(IVillaRepository dbvilla, IVillaNumberRepository dbVillaNumber, IMapper mapper) //Constructor Injection to use DbContext.
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            this._response = new();
            _dbvilla = dbvilla;
        }
        [HttpGet] //getting data form db. 
        [ProducesResponseType(StatusCodes.Status200OK)] // this is response type of api
        public async Task<ActionResult<ApiResponse>> GetVillaNumber()  //asyn await recommended way to use in API by microsoft.
        {
            try
            {
                IEnumerable<VillaNumber> villaList = await _dbVillaNumber.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaNumberDto>>(villaList);
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
        [HttpGet("{id:int}", Name = "GetVillaNumber")] //here we are specifing that this method accept id as parameter.
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(200, Type=typeof(villadto)] -- this is alternate 
        public async Task<ActionResult<ApiResponse>> GetVillaNumber(int id)
        {
            try
            {

                if (id == 0)
                {
                    return BadRequest();
                }
                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    return NotFound();
                }
                _response.Result = _mapper.Map<VillaNumberDto>(villaNumber);
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

        public async Task<ActionResult<ApiResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDto villaNumberCreateDto)
        {
            try
            {

                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == villaNumberCreateDto.VillaNo) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa Number already exist!");
                    return BadRequest(ModelState);
                }
                if (await _dbvilla.GetAsync(u => u.Id == villaNumberCreateDto.VillaID) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa Id is invlaid!");
                    return BadRequest(ModelState);
                }

                if (villaNumberCreateDto == null)
                {
                    return BadRequest(villaNumberCreateDto);
                }


                //need to manuallt convert villadto -> villa model 
                VillaNumber model = _mapper.Map<VillaNumber>(villaNumberCreateDto);


                await _dbVillaNumber.CreateAsync(model);

                _response.Result = _mapper.Map<VillaNumberDto>(model);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = model.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")] //Deleting record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in delete.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse>> DeleteVillaNumber(int id) // here using IActionResult because we are not returning anything.
        {
            try
            {

                if (id == 0)
                {
                    return BadRequest();
                }
                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    return BadRequest();
                }
                await _dbVillaNumber.RemoveAsync(villaNumber);
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
        [HttpPut("{id:int}", Name = "UpdateVillaNumber")] //Updating record in db.
        [ProducesResponseType(StatusCodes.Status204NoContent)] //by default we return no content in Update.
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDto villaNumberUpdateDto)
        {
            try
            {

                if (villaNumberUpdateDto == null || id != villaNumberUpdateDto.VillaNo)
                {
                    return BadRequest();
                }
                if (await _dbvilla.GetAsync(u => u.Id == villaNumberUpdateDto.VillaID) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa Id is invlaid!");
                    return BadRequest(ModelState);
                }
                VillaNumber model = _mapper.Map<VillaNumber>(villaNumberUpdateDto);

                await _dbVillaNumber.UpdateAsync(model);
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




    }
}
