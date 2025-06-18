using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly IValidator<CreateBookDTO> _createValidator;
        private readonly IValidator<UpdateBookDTO> _updateValidator;
        private readonly ImageService _imageService;

        public BookController(
            BookService bookService,
            IValidator<CreateBookDTO> createValidator,
            IValidator<UpdateBookDTO> updateValidator, ImageService imageService)
        {
            _bookService = bookService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _imageService = imageService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAll([FromQuery] QueryParamsBookDTO queryParams)
        {
            var result = await _bookService.GetAllBooksAsync(queryParams);
            return Ok(new 
            {
                message = "Libros obtenidos correctamente",
                data = result
            });
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(new
            {
                message = "Libro obtenido correctamente",
                data = book
            });
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateBookDTO createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }

            try
            {
                var newBook = await _bookService.CreateBookAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newBook.Id }, newBook);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBookDTO updateDto)
        {
            if (updateDto.Id != id)
            {
                return BadRequest("El ID del libro no coincide con el ID en el cuerpo de la solicitud.");
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Errors = errores });
            }

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(updateDto);
                if (updatedBook == null) return NotFound();
                return Ok(updatedBook);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        //Eliminar una imágen de un book
        [HttpDelete("{bookId}/images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(int bookId, int imageId)
        {
            try
            {
                await _imageService.DeleteImageAsync(imageId);

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        //eliminar todas las imagenes de un book
        [HttpDelete("{bookId}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllImages(int bookId)
        {
            try
            {
                await _imageService.DeleteAllImagesAsync(bookId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
