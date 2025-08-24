using System;
using API_BITLIBRO.Context;
using API_BITLIBRO.Interfaces.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace API_BITLIBRO.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _tx;
    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task BeginTransactionAsync()
    {
        _tx ??= await _context.Database.BeginTransactionAsync();
    }
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task CommitAsync()
    {
        if (_tx != null)
        {
            await _tx.CommitAsync();
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_tx != null)
        {
            await _tx.RollbackAsync();
            await _tx.DisposeAsync();
            _tx = null;
        }
    }
}
