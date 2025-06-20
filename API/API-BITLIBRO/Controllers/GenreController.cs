using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Controllers
{
    [Route("api/genres")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        private readonly IValidator<CreateGenreDTO> _createValidator;
        private readonly IValidator<UpdateGenreDTO> _updateValidator;
        public GenreController(IGenreService genreService,
            IValidator<CreateGenreDTO> createValidator,
            IValidator<UpdateGenreDTO> updateValidator)
        {
            _genreService = genreService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponseData<PagedResponse<GenreResponseDto>>>> GetAll([FromQuery] GenreQueryParams queryParams)
        {
            var result = await _genreService.GetAllGenresAsync(queryParams);
            return Ok(ApiResponseData<PagedResponse<GenreResponseDto>>.Success(result, "Géneros obtenidos correctamente"));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseData<GenreResponseDto>>> GetById(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null) return NotFound(ApiResponse.Fail("Género no encontrado"));
            return Ok(ApiResponseData<GenreResponseDto>.Success(genre, "Género obtenido correctamente"));
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGenreDTO createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var newGenre = await _genreService.CreateGenreAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newGenre.Id }, newGenre);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return Conflict(ApiResponse.Fail("El nombre del género ya existe"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseData<GenreResponseDto>>> Update(int id, [FromBody] UpdateGenreDTO updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(ApiResponse.Fail("El ID del género no coincide con el ID proporcionado"));
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var updatedGenre = await _genreService.UpdateGenreAsync(updateDto);
                if (updatedGenre == null) return NotFound(ApiResponse.Fail("Género no encontrado"));
                return Ok(ApiResponseData<GenreResponseDto>.Success(updatedGenre,"Genero actualizado exitosamente"));
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return Conflict(ApiResponse.Fail("El nombre del género ya existe"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _genreService.DeleteGenreAsync(id);
            if (!result) return NotFound(ApiResponse.Fail("Género no encontrado"));

            return NoContent();
        }
    }
}
