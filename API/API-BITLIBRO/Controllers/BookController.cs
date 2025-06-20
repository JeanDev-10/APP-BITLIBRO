using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.Interfaces;
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
        private readonly IBookService _bookService;
        private readonly IValidator<CreateBookDTO> _createValidator;
        private readonly IValidator<UpdateBookDTO> _updateValidator;
        private readonly IImageService _imageService;

        public BookController(
            IBookService bookService,
            IValidator<CreateBookDTO> createValidator,
            IValidator<UpdateBookDTO> updateValidator, IImageService imageService)
        {
            _bookService = bookService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _imageService = imageService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<ApiResponseData<PagedResponse<ResponseBookDTO>>>> GetAll([FromQuery] QueryParamsBookDTO queryParams)
        {
            var result = await _bookService.GetAllBooksAsync(queryParams);
            return Ok(ApiResponseData<PagedResponse<ResponseBookDTO>>.Success(result, "Libros obtenidos correctamente"));
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<ApiResponseData<ResponseBookDTO>>> GetById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound(ApiResponse.Fail("Libro no encontrado"));
            return Ok(ApiResponseData<ResponseBookDTO>.Success(book, "Libro obtenido exitosamente"));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateBookDTO createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var newBook = await _bookService.CreateBookAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newBook.Id }, newBook);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseData<string>.Fail("Errores de validación", ex.Message));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }

        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseData<ResponseBookDTO>>> Update(int id, [FromForm] UpdateBookDTO updateDto)
        {
            if (updateDto.Id != id)
            {
                return BadRequest(ApiResponse.Fail("El ID del libro no coincide con el ID en el cuerpo de la solicitud."));
            }
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(updateDto);
                if (updatedBook == null) return NotFound(ApiResponse.Fail("Libro no encontrado"));
                return Ok(ApiResponseData<ResponseBookDTO>.Success(updatedBook, "Libro actualizado correctamente"));
            }
            catch (ValidationException ex)
            {
                return BadRequest(ApiResponseData<string>.Fail("Errores de validación", ex.Message));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return NotFound(ApiResponse.Fail("Libro no encontrado"));
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
                return NotFound(ApiResponse.Fail(ex.Message));
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
                return NotFound(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
