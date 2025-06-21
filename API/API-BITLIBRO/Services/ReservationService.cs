using System;
using System.Security.Claims;
using API_BITLIBRO.Context;
using API_BITLIBRO.DTOs;
using API_BITLIBRO.DTOs.Auth.Me;
using API_BITLIBRO.DTOs.Book;
using API_BITLIBRO.DTOs.Client;
using API_BITLIBRO.DTOs.Employee;
using API_BITLIBRO.DTOs.Genre;
using API_BITLIBRO.DTOs.Reservation;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API_BITLIBRO.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ReservationService(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<PagedResponse<ReservationResponseDTO>> GetAllReservationsAsync(ReservationQueryParamsDTO queryParams, ClaimsPrincipal user, bool isAdmin)
    {
        var query = _context.Reservations
            .Include(r => r.Employee)
            .Include(r => r.Client)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .AsQueryable();

        // Aplicar filtros
        if (!string.IsNullOrEmpty(queryParams.Status))
            query = query.Where(r => r.Status == queryParams.Status);

        if (!string.IsNullOrEmpty(queryParams.ClientName))
            query = query.Where(r =>
                r.Client!.Name.Contains(queryParams.ClientName) ||
                r.Client.LastName.Contains(queryParams.ClientName));

        if (isAdmin && !string.IsNullOrEmpty(queryParams.EmployeeName))
            query = query.Where(r =>
                r.Employee!.Name.Contains(queryParams.EmployeeName) ||
                r.Employee.LastName.Contains(queryParams.EmployeeName));

        // Si es empleado, solo ver sus reservas
        if (!isAdmin)
        {
            var employeeId = _userManager.GetUserId(user);
            query = query.Where(r => r.EmployeeId == employeeId);
        }

        // Contar total antes de paginar
        var totalRecords = await query.CountAsync();

        // Aplicar paginación
        var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(r => new ReservationResponseDTO
            {
                Id = r.Id,
                Status = r.Status,
                Employee = new EmployeeResponseDTO
                {
                    Id = r.Employee!.Id,
                    Name = r.Employee.Name,
                    LastName = r.Employee.LastName,
                    Ci = r.Employee!.Ci,
                    Email = r.Employee.Email!,
                    CreatedAt = r.Employee.CreatedAt,
                    UpdatedAt = r.Employee.UpdatedAt
                },
                Book = new ResponseBookDTO
                {
                    Id = r.Book!.Id,
                    Name = r.Book.Name,
                    ISBN = r.Book.ISBN,
                    YearPublished = r.Book.YearPublished,
                    Editorial = r.Book.Editorial,
                    Author = r.Book.Author,
                    CreatedAt = r.Book.CreatedAt,
                    UpdatedAt = r.Book.UpdatedAt,
                    Genres = r.Book.BookGenres!.Select(bg => new GenreResponseDto
                    {
                        Id = bg.Genre!.Id,
                        Name = bg.Genre.Name,
                        CreatedAt = bg.Genre.CreatedAt,
                        UpdatedAt = bg.Genre.UpdatedAt
                    }).ToList(),
                    ImageUrls = r.Book.Images!.Select(i => new ResponseImageDTO
                    {
                        Id = i.Id,
                        Url = i.Url
                    }).ToList()
                },
                Client = new ClientResponseDTO
                {
                    Id = r.Client!.Id,
                    Name = r.Client.Name,
                    LastName = r.Client.LastName,
                    Ci = r.Client.Ci,
                    CreatedAt = r.Client.CreatedAt
                },
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return new PagedResponse<ReservationResponseDTO>
        {
            Data = reservations,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)queryParams.PageSize)
        };
    }

    public async Task<ReservationResponseDTO> GetReservationByIdAsync(int id, ClaimsPrincipal user, bool isAdmin)
    {
        var query = _context.Reservations
            .Include(r => r.Employee)
            .Include(r => r.Client)
            .Include(r => r.Book)
                .ThenInclude(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
            .Include(r => r.Book)
                .ThenInclude(b => b.Images)
            .Where(r => r.Id == id);

        // Si no es admin, solo puede ver sus propias reservas
        if (!isAdmin)
        {
            var employeeId = _userManager.GetUserId(user);
            query = query.Where(r => r.EmployeeId == employeeId);
        }

        var reservation = await query.FirstOrDefaultAsync();
        if (reservation == null) return null;
        return new ReservationResponseDTO
        {
            Id = reservation.Id,
            Status = reservation.Status,
            Employee = new EmployeeResponseDTO
            {
                Id = reservation.Employee!.Id,
                Name = reservation.Employee!.Name,
                LastName = reservation.Employee!.LastName,
                Ci = reservation.Employee.Ci,
                Email = reservation.Employee!.Email,
                CreatedAt = reservation.Employee.CreatedAt,
                UpdatedAt = reservation.Employee.UpdatedAt

            },
            Book = new ResponseBookDTO
            {
                Id = reservation.Book!.Id,
                Name = reservation.Book.Name,
                ISBN = reservation.Book.ISBN,
                YearPublished = reservation.Book.YearPublished,
                Editorial = reservation.Book.Editorial,
                Author = reservation.Book.Author,
                CreatedAt = reservation.Book.CreatedAt,
                UpdatedAt = reservation.Book.UpdatedAt,
                Genres = reservation.Book.BookGenres!.Select(bg => new GenreResponseDto
                {
                    Id = bg.Genre!.Id,
                    Name = bg.Genre.Name,
                    CreatedAt = bg.Genre.CreatedAt,
                    UpdatedAt = bg.Genre.UpdatedAt
                }).ToList(),
                ImageUrls = reservation.Book.Images!.Select(i => new ResponseImageDTO
                {
                    Id = i.Id,
                    Url = i.Url
                }).ToList()
            },
            Client = new ClientResponseDTO
            {
                Id = reservation.Client!.Id,
                Name = reservation.Client!.Name,
                LastName = reservation.Client.LastName,
                Ci = reservation.Client.Ci,
                CreatedAt = reservation.Client.CreatedAt
            },
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            CreatedAt = reservation.CreatedAt,
            UpdatedAt = reservation.UpdatedAt
        };
    }
    public async Task<ReservationResponseDTO> CreateReservationAsync(CreateReservationDTO createDto, string employeeId)
    {
        // Verificar que el libro existe y está disponible
        var book = await _context.Books.FindAsync(createDto.BookId);
        if (book == null)
            throw new Exception("El libro no existe");

        // Verificar si el libro ya está reservado en esas fechas
        var overlappingReservation = await _context.Reservations
            .Where(r => r.BookId == createDto.BookId && r.Status == "Pendiente" &&
                       (r.StartDate <= createDto.EndDate && r.EndDate >= createDto.StartDate))
            .AnyAsync();

        if (overlappingReservation)
            throw new Exception("El libro ya está reservado en ese período");


        User user;

        // Caso 1: Se proporcionó UserId (usuario existente)
        if (createDto.UserId != null)
        {
            user = await _userManager.FindByIdAsync(createDto.UserId);
            if (user == null)
                throw new Exception("Usuario no encontrado");
        }
        // Caso 2: Se proporcionaron datos de usuario (nuevo usuario)
        else
        {
            user = new User
            {
                UserName = createDto.Ci,
                Ci = createDto.Ci!,
                Name = createDto.Name!,
                LastName = createDto.LastName!,
                Email = $"{createDto.Ci}@fake.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw new Exception($"Error al crear usuario: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "Client");
        }
        var reservation = new Reservation
        {
            Status = "Pendiente",
            EmployeeId = employeeId,
            BookId = createDto.BookId,
            ClientId = user.Id,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return await GetReservationByIdAsync(reservation.Id, null, true); // es null porque esta acción la hace admin entonces no hace falta los claims
    }

    public async Task<ReservationResponseDTO> UpdateReservationAsync(UpdateReservationDTO updateDto, string employeeId, bool isAdmin)
    {
        var reservation = await _context.Reservations.FindAsync(updateDto.Id);
        if (reservation == null)
            throw new Exception("Reserva no encontrada");

        // Solo el empleado que creó la reserva o un admin puede editarla
        if (!isAdmin && reservation.EmployeeId != employeeId)
            throw new Exception("No tienes permiso para editar esta reserva");

        // Si se cambia el libro, verificar disponibilidad
        if (updateDto.BookId != 0 && updateDto.BookId != reservation.BookId)
        {
            var book = await _context.Books.FindAsync(updateDto.BookId);
            if (book == null)
                throw new Exception("El nuevo libro no existe");

            var datesToCheck = new
            {
                EndDate = updateDto.EndDate
            };

            var overlappingReservation = await _context.Reservations
                .Where(r => r.BookId == updateDto.BookId && r.Status == "Pendiente" &&
                           r.Id != updateDto.Id &&
                           (r.StartDate <= datesToCheck.EndDate && r.EndDate >= r.StartDate))
                .AnyAsync();

            if (overlappingReservation)
                throw new Exception("El nuevo libro ya está reservado en ese período");

            reservation.BookId = updateDto.BookId;
        }

        if (!string.IsNullOrEmpty(updateDto.Status))
            reservation.Status = updateDto.Status;
        reservation.EndDate = updateDto.EndDate;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetReservationByIdAsync(reservation.Id, null, true);
    }
}
