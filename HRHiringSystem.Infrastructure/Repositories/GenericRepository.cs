using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRHiringSystem.Application.Interfaces;
using HRHiringSystem.Infrastructure.Data;
using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : AuditableEntity
{
    protected readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T> AddAsync(T entity, Guid? createdBy = null)
    {
        // Ensure the entity has an Id (Guid) assigned if not set
        var idProp = typeof(T).GetProperty("Id");
        if (idProp != null && idProp.PropertyType == typeof(Guid))
        {
            var current = (Guid)idProp.GetValue(entity)!;
            if (current == Guid.Empty)
            {
                idProp.SetValue(entity, Guid.NewGuid());
            }
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = createdBy;
        entity.IsDeleted = false;

        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _context.Set<T>()
            .Where(e => !EF.Property<bool>(e, "IsDeleted"))
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>()
            .Where(e => !EF.Property<bool>(e, "IsDeleted"))
            .ToListAsync();
    }

    public async Task UpdateAsync(T entity, Guid? updatedBy = null)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid? updatedBy = null)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
    }
}
