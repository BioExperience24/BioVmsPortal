using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Repositories;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Infrastructure;

public class UnitOfWorkNotToken : IUnitOfWorkNotToken
{
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    private readonly ApplicationDbContext _context;

    // repositories

    public IUserRepository UserRepository { get; }
    public IBookRepository BookRepository { get; }

    public IMediaRepository MediaRepository { get; }

    public UnitOfWorkNotToken(ApplicationDbContext dbContext)
    {
        _context = dbContext;
        // repositories
        UserRepository = new UserRepository(_context);
        BookRepository = new BookRepository(_context);
        MediaRepository = new MediaRepository(_context);
    }

    // save changes
    public int SaveChanges() => _context.SaveChanges();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    // transaction
    public void BeginTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    // commit
    public void Commit()
    {
        if (_transaction == null)
        {
            throw TransactionException.TransactionNotCommitException();
        }
        try
        {
            _context.SaveChanges();
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
        {
            throw TransactionException.TransactionNotCommitException();
        }

        try
        {
            await _context.SaveChangesAsync();
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    // rollback
    public void Rollback()
    {
        if (_transaction == null)
        {
            throw TransactionException.TransactionNotCommitException();
        }

        _transaction.Rollback();
        _transaction.Dispose();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
        {
            throw TransactionException.TransactionNotCommitException();
        }

        await _transaction.RollbackAsync();
        _transaction.Dispose();
        _transaction = null;
    }

    // dispose
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // execute transaction
    public async Task ExecuteTransactionAsync(Action action)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            action();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw TransactionException.TransactionNotExecuteException(ex);
        }
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw TransactionException.TransactionNotExecuteException(ex);
        }
    }
}
