using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Service
{
    public class AddressBookRL
    {
        private readonly ApplicationDbContext _context;
        public AddressBookRL(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AddressBookEntry>> GetAllContact()
        {
            return await _context.AddressBookEntries.ToListAsync();
        }

        public async Task<AddressBookEntry?> GetContactById(int id)
        {
            return await _context.AddressBookEntries.FindAsync(id);
        }

        public async Task AddNewContact(AddressBookEntry entry)
        {
            await _context.AddressBookEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContact(AddressBookEntry entry)
        {
            _context.AddressBookEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContactById(int id)
        {
            var entry = await _context.AddressBookEntries.FindAsync(id);
            if (entry != null)
            {
                _context.AddressBookEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<AddressBookEntry>> GetAddressBookEntries(int id)
        {
            return await _context.AddressBookEntries
           .Where(entry => entry.UserId == id)
           .ToListAsync();
        }
    }



    
}
