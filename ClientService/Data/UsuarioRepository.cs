using ClientService.Data;
using ClientService.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClientService.Data
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Usuario>> GetAllUsuariosAsync()
        {
            return await _context.Usuarios.Where(u => u.Estado).ToListAsync();
        }

        public async Task<Usuario> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Estado);
        }

        public async Task CreateUsuarioAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUsuarioAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsuarioAsync(int id)
        {
            var usuario = await GetUsuarioByIdAsync(id);
            if (usuario != null)
            {
                usuario.Estado = false;
                await UpdateUsuarioAsync(usuario);
            }
        }

        public async Task<bool> VerificarUsuarioExisteAsync(int usuarioId)
        {
            var usuario = await GetUsuarioByIdAsync(usuarioId);

            return usuario != null && usuario.Estado;
        }
    }
}
