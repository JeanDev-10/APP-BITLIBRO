using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class GenreController : ControllerBase
    {
        private readonly GenreService _genreService;
        private readonly IValidator<CreateGenreDTO> _createValidator;
        private readonly IValidator<UpdateGenreDTO> _updateValidator;
        public GenreController(GenreService genreService,
            IValidator<CreateGenreDTO> createValidator,
            IValidator<UpdateGenreDTO> updateValidator)
        {
            _genreService = genreService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GenreQueryParams queryParams)
        {
            var result = await _genreService.GetAllGenresAsync(queryParams);
            return Ok(new
            {
                message = "Géneros obtenidos correctamente",
                data = result
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            if (genre == null) return NotFound();

            return Ok(new
            {
                message = "Género obtenido correctamente",
                data = genre
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGenreDTO createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }

            try
            {
                var newGenre = await _genreService.CreateGenreAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newGenre.Id }, newGenre);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return Conflict(new { message = "El nombre del género ya existe" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al crear el género", error = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGenreDTO updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest(new { message = "El ID del género no coincide con el ID proporcionado" });
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }

            try
            {
                var updatedGenre = await _genreService.UpdateGenreAsync(updateDto);
                if (updatedGenre == null) return NotFound();
                return Ok(updatedGenre);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return Conflict(new { message = "El nombre del género ya existe" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al crear el género", error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _genreService.DeleteGenreAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }
    }
}
